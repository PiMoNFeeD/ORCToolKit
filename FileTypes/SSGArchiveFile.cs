using ORCToolKit.Extensions;
using System.IO.Compression;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Text;

namespace ORCToolKit.FileTypes
{
	public class SSGArchiveFile: IFileType
	{
		// NOTE: sign-ness in this structure do not represent whether the actual data
		// in the file is signed/unsigned, it's only to make code complain less and having to do less Convert.To(U)Int32 calls
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		private class SSGHeader
		{
			public UInt32 Version;
			public UInt32 Padding1; // always 0
			public Int32 SectionInfosSize;
			public Int32 SectionNamesSize;

			public Int32 SectionDataSize; // can contain both compressed and uncompressed data
			public UInt32 Padding2; // always 0
			public Int32 CompressedBlocksSize; // size of all compressed block sizes (each compressed block is exactly 64kb of uncompressed data)
			public UInt16 Alignment;
			public UInt16 Padding3; // always 0
		}

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		private class SSGSection
		{
			public UInt32 NameCRC32;
			public Int32 NameOffset; // relative to section names
			public Int32 UncompressedSize;
			public Int32 Unknown; // appears to always be 1

			public Int32 DataOffset; // only reliable for uncompressed, for compressed need to use the size of all uncompressed sections that came before this one
			public UInt32 Type;
			public UInt32 UncompressedCRC32;
			public Int32 CompressedSize; // this might not actually be the size of one section, but a size of multiple compressed sections
		}

		public class DataSection
		{
			public string Name = string.Empty;
			public CRC32 NameCRC32 = 0; // TODO: should NOT be stored, but in some instances it does not match the actual CRC32 of a name string!
			public byte[] RawData = Array.Empty<byte>();
			public UInt32 Type = 0;
			public bool IsCompressed = false;
		}

		private UInt32 _fileVersion = 6;
		private UInt16 _fileAlignment = 16;
		private Endianness _fileEndianness = Endianness.Little;
		private bool _splitCompression = false; // by default, assume we don't split, unless the file we open does split
		public List<DataSection> DataSections = new List<DataSection>();

		// raw data is compressed in blocks of this size
		private const Int32 CompressionSize = 64 * 1024;
		// sections share compressed blocks of data only up to this amount of uncompressed data, beyond that they get split
		// TODO: different files split at different sizes! but how to calculate that???
		private const Int32 MaxCompressionSize = CompressionSize * 4;

		public void Reset()
		{
			DataSections.Clear();
		}

		public void LoadFromBuffer( Stream bufferStream )
		{
			Reset();

			FileStream? logFile = null;
			StreamWriter? logWriter = null;
#if DEBUG
			if ( bufferStream is FileStream bufferFileStream )
			{
				logFile = File.OpenWrite( bufferFileStream.Name + "_log" );
				logWriter = new StreamWriter( logFile );
			}
#endif

			try
			{
				using ( BinaryReader reader = new BinaryReader( bufferStream, Encoding.UTF8, true ) )
				{
					// determine endianness, the way the game does it
					UInt32 headerVersion = reader.ReadUInt32( Endianness.Little );
					_fileEndianness = headerVersion > 0xFFFF ? Endianness.Big : Endianness.Little;

					reader.BaseStream.Position -= Marshal.SizeOf<UInt32>();
					SSGHeader header = reader.ReadType<SSGHeader>( _fileEndianness );
					if ( header.Version != 6 && header.Version != 5 ) // RE:ORC only has version 5 and 6 archives
						throw new FileFormatException();

					if ( header.Padding1 != 0 ) // make sure it's 0 to know we have the right file
						throw new FileFormatException();

					_fileVersion = header.Version;
					if ( _fileVersion == 5 )
					{
						_fileAlignment = 16; // this line of code is brought to you by reverse-engineering the game
						header.CompressedBlocksSize = 0; // speculation, it's not 0 but there is no compression
					}

					logWriter?.WriteLine( "SSG Header:" );
					logWriter?.WriteLine( $"Version:              {header.Version}" );
					logWriter?.WriteLine( $"SectionInfosSize:     {header.SectionInfosSize}" );
					logWriter?.WriteLine( $"SectionNamesSize:     {header.SectionNamesSize}" );
					logWriter?.WriteLine( $"SectionDataSize:      {header.SectionDataSize}" );
					logWriter?.WriteLine( $"CompressedBlocksSize: {header.CompressedBlocksSize}" );
					logWriter?.WriteLine( $"Alignment:            {header.Alignment}\n" );

					// TODO: console archives are using unknown compression, but it's the same between PS3 and XBOX 360
					if ( _fileEndianness == Endianness.Big && header.CompressedBlocksSize > 0 )
						throw new FileFormatException( Resources.ORCToolKit.FileSSGArchiveFileCompressedConsoleNotSupported );

					// read compressed blocks
					// they come after section infos, but we need them earlier to be able to decompress data
					List<Int32> compressedBlockSizesList = new List<Int32>();
					reader.BaseStream.Position = Marshal.SizeOf<SSGHeader>() + header.SectionInfosSize;
					for ( Int32 bytesRead = 0; bytesRead < header.CompressedBlocksSize; bytesRead += 4 )
					{
						Int32 size = reader.ReadInt32( _fileEndianness );
						if ( size == 0 )
							break;
						compressedBlockSizesList.Add( size );
					}

					// read sections last, after we know compressed block sizes, to decompress them on the fly
					if ( header.SectionInfosSize % Marshal.SizeOf<SSGSection>() != 0 )
						throw new FileFormatException();
					Int32 sectionCount = header.SectionInfosSize / Marshal.SizeOf<SSGSection>();

					// compressed data comes right after uncompressed, and we need to know when it actually starts
					Int32 compressedDataOffset = 0;
					List<byte> decompressedData = new List<byte>();

					Int32 lastNegativeCompressedSize = 0;
					for ( int i = 0; i < sectionCount; i++ )
					{
						reader.BaseStream.Position = Marshal.SizeOf<SSGHeader>() + Marshal.SizeOf<SSGSection>() * i;

						SSGSection section = reader.ReadType<SSGSection>( _fileEndianness );

						// this is a very dumb hack to determine whether we need to split compressed blocks of sections or not
						// doesn't seem to actually affect whether the file loads or not by the game, but I want num of compressed blocks
						// to match the original file!
						if ( section.CompressedSize < 0 )
						{
							if ( lastNegativeCompressedSize == 0 )
								lastNegativeCompressedSize = section.CompressedSize;
							else if ( lastNegativeCompressedSize != section.CompressedSize )
								_splitCompression = true;
						}

						// this block of code is brought to you by reverse-engineering the game
						if ( header.CompressedBlocksSize == 0 )
							section.CompressedSize = 0;

						DataSection dataSection = new DataSection();
						dataSection.Type = section.Type;
						dataSection.IsCompressed = (section.CompressedSize != 0); // it can be negative, which means it shares a compressed block with the previous section

						// read file name
						reader.BaseStream.Position = Marshal.SizeOf<SSGHeader>() + header.SectionInfosSize + header.CompressedBlocksSize + section.NameOffset;
						dataSection.Name = reader.ReadNullTerminatedString( Encoding.UTF8 );
						dataSection.NameCRC32 = section.NameCRC32;

						if ( dataSection.IsCompressed )
						{
							if ( decompressedData.Count == 0 )
							{
								// uncompress all compressed blocks
								Int32 compressedDataReadSoFar = 0;
								foreach ( Int32 compressedBlockSize in compressedBlockSizesList )
								{
									// need to open a new zlibstream every time, because reading one block results in the whole stream being marked as "finished"
									using ( ZLibStream compressionStream = new ZLibStream( bufferStream, CompressionMode.Decompress, true ) )
									{
										// guess the position of the next compressed block in file
										compressionStream.BaseStream.Position = Marshal.SizeOf<SSGHeader>() +
											header.SectionInfosSize +
											header.CompressedBlocksSize +
											header.SectionNamesSize +
											compressedDataOffset + compressedDataReadSoFar;

										using ( MemoryStream decompressedStream = new MemoryStream() )
										{
											compressionStream.CopyTo( decompressedStream );
											decompressedData.AddRange( decompressedStream.GetBuffer() );
											logWriter?.WriteLine( $"Compressed block: compressed {compressedBlockSize}, decompressed {decompressedStream.Length}" );
										}

										compressedDataReadSoFar += compressedBlockSize;
									}
								}
								compressedDataOffset = 0; // revert this to zero, as it's now going to be an offset within compressed data, not within all data
							}

							dataSection.RawData = new byte[section.UncompressedSize];
							decompressedData.CopyTo( compressedDataOffset, dataSection.RawData, 0, section.UncompressedSize );
						}
						else
						{
							reader.BaseStream.Position = Marshal.SizeOf<SSGHeader>() +
								header.SectionInfosSize + header.CompressedBlocksSize + header.SectionNamesSize + section.DataOffset;
							dataSection.RawData = reader.ReadBytes( section.UncompressedSize );
						}

						// do not rely on data offset for compressed sections: data is often shared between multiple sections
						// and can point to wrong offsets, so use uncompressed size instead
						compressedDataOffset += MathExtensions.AlignValue( section.UncompressedSize, _fileAlignment );

						logWriter?.WriteLine( $"\nSection {DataSections.Count + 1}:" );
						logWriter?.WriteLine( $"NameCRC32:         {section.NameCRC32}" );
						logWriter?.WriteLine( $"NameOffset:        {section.NameOffset}" );
						logWriter?.WriteLine( $"UncompressedSize:  {section.UncompressedSize}" );
						logWriter?.WriteLine( $"DataOffset:        {section.DataOffset}" );
						logWriter?.WriteLine( $"Type:              {section.Type}" );
						logWriter?.WriteLine( $"UncompressedCRC32: {section.UncompressedCRC32}" );
						logWriter?.WriteLine( $"CompressedSize:    {section.CompressedSize}" );

						DataSections.Add( dataSection );
					}
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			finally
			{
				logWriter?.Close();
				logFile?.Close();
			}
		}

		public void SaveToBuffer( Stream bufferStream )
		{
			try
			{
				List<SSGSection> sections = new List<SSGSection>( DataSections.Count );
				Dictionary<string, Int32> sectionNames = new Dictionary<string, Int32>();
				Int32 nextNameOffset = 0;

				foreach ( DataSection dataSection in DataSections )
				{
					// fill name dictionary
					if ( !sectionNames.TryGetValue( dataSection.Name, out Int32 nameOffset ) )
					{
						sectionNames.Add( dataSection.Name, nextNameOffset );
						nameOffset = nextNameOffset;
						nextNameOffset += Encoding.UTF8.GetByteCount( dataSection.Name ) + 1;
					}
				}

				// remember this data to compress sections on the fly in one pass
				Int32 parentCompressedSectionIndex = -1;
				Int32 numCompressedBlocksProcessed = 0;
				List<MemoryStream> compressedBlocks = new List<MemoryStream>();
				MemoryStream dataToCompress = new MemoryStream( MaxCompressionSize );

				// in case there are multiple sections named the same, we need to split compression precisely where we meet a name that already exists
				// particularly cutscene files, because they store raw animation data for each object in the scene, split in groups of frames
				HashSet<string> compressedSectionsWritten = new HashSet<string>();

				Int32 nextDataOffset = 0;
				for ( Int32 sectionIndex = 0; sectionIndex < DataSections.Count; sectionIndex++ )
				{
					SSGSection section = new SSGSection();
					//Section.NameCRC32 = DataSections[sectionIndex].Name.GetCRC32(); // TODO: UI/Gallery.ssg mismatch for GalleryImages section!
					section.NameCRC32 = DataSections[sectionIndex].NameCRC32;
					section.NameOffset = sectionNames[DataSections[sectionIndex].Name];
					section.UncompressedSize = DataSections[sectionIndex].RawData.Length;
					section.Unknown = 1;

					section.Type = DataSections[sectionIndex].Type;
					section.UncompressedCRC32 = ~Crc32.HashToUInt32( DataSections[sectionIndex].RawData.ToArray() );

					if ( DataSections[sectionIndex].IsCompressed )
					{
						// group together sections up to 4 compressed chunks that will all share one compressed size, then split
						if ( _splitCompression && parentCompressedSectionIndex != -1 )
						{
							//bool SplitData = (dataToCompress.Length != 0) && (dataToCompress.Length + MathExtensions.AlignValue( Section.UncompressedSize, _fileAlignment ) > dataToCompress.Capacity);
							bool splitData = compressedSectionsWritten.Contains( DataSections[sectionIndex].Name );
							if ( splitData )
							{
								// compress data now
								Int32 compressedSizeForCurrentChunkBlock = 0;
								dataToCompress.Position = 0;
								while ( dataToCompress.Position < dataToCompress.Length )
								{
									MemoryStream compressedBlock = new MemoryStream();
									using ( ZLibStream compressedStream = new ZLibStream( compressedBlock, CompressionLevel.Fastest, true ) )
									{
										byte[] dataChunk = new byte[CompressionSize];
										Int32 bytesInChunk = dataToCompress.Read( dataChunk, 0, CompressionSize );
										compressedStream.Write( dataChunk, 0, bytesInChunk );
									}
									compressedSizeForCurrentChunkBlock += Convert.ToInt32( compressedBlock.Length );
									compressedBlocks.Add( compressedBlock );
								}

								sections[parentCompressedSectionIndex].CompressedSize = compressedSizeForCurrentChunkBlock;
								sections[parentCompressedSectionIndex].DataOffset = nextDataOffset;
								nextDataOffset += compressedSizeForCurrentChunkBlock;

								// mark all previous sections to share compressed data with parent
								for ( Int32 compressedSectionIndex = parentCompressedSectionIndex + 1; compressedSectionIndex < sectionIndex; compressedSectionIndex++ )
								{
									sections[compressedSectionIndex].DataOffset = nextDataOffset;
									// this is wrong, but it seems to work in-game, as long as it's negative and a unique value for each batch
									// which means it might not actually matter what the value is, as long as it's unique..?
									sections[compressedSectionIndex].CompressedSize = -numCompressedBlocksProcessed - 1;
								}

								// clear all data for the next block of compressed sections
								compressedSectionsWritten.Clear();
								dataToCompress.SetLength( 0 );
								dataToCompress.Position = 0;
								parentCompressedSectionIndex = -1;
								numCompressedBlocksProcessed++;
							}
						}

						if ( parentCompressedSectionIndex == -1 )
							parentCompressedSectionIndex = sectionIndex;

						dataToCompress.Write( DataSections[sectionIndex].RawData );
						compressedSectionsWritten.Add( DataSections[sectionIndex].Name );

						// align it
						Int64 alignmentBytes = dataToCompress.Position - MathExtensions.AlignValue( dataToCompress.Position, _fileAlignment );
						if ( alignmentBytes > 0 )
							dataToCompress.Write( new byte[alignmentBytes] );
					}
					else
					{
						section.DataOffset = nextDataOffset;
						section.CompressedSize = 0;
						nextDataOffset += MathExtensions.AlignValue( section.UncompressedSize, _fileAlignment );
					}

					sections.Add( section );
				}

				// compress whatever data we have that didn't go beyond MaxCompressionSize
				if ( parentCompressedSectionIndex != -1 )
				{
					Int32 compressedSizeForCurrentChunkBlock = 0;
					dataToCompress.Position = 0;
					while ( dataToCompress.Position < dataToCompress.Length )
					{
						MemoryStream compressedBlock = new MemoryStream();
						using ( ZLibStream compressedStream = new ZLibStream( compressedBlock, CompressionLevel.Fastest, true ) )
						{
							byte[] dataChunk = new byte[CompressionSize];
							Int32 bytesInChunk = dataToCompress.Read( dataChunk, 0, CompressionSize );
							compressedStream.Write( dataChunk, 0, bytesInChunk );
						}
						compressedSizeForCurrentChunkBlock += Convert.ToInt32( compressedBlock.Length );
						compressedBlocks.Add( compressedBlock );
					}

					sections[parentCompressedSectionIndex].CompressedSize = compressedSizeForCurrentChunkBlock;
					sections[parentCompressedSectionIndex].DataOffset = nextDataOffset;
					nextDataOffset += compressedSizeForCurrentChunkBlock;

					// mark all previous sections to share compressed data with parent
					for ( Int32 compressedSectionIndex = parentCompressedSectionIndex + 1; compressedSectionIndex < sections.Count; compressedSectionIndex++ )
					{
						sections[compressedSectionIndex].DataOffset = nextDataOffset;
						// this is wrong, but it seems to work in-game, as long as it's negative and a unique value for each batch
						// which means it might not actually matter what the value is, as long as it's unique..?
						sections[compressedSectionIndex].CompressedSize = -numCompressedBlocksProcessed - 1;
					}
				}

				SSGHeader header = new SSGHeader();
				header.Version = _fileVersion;
				header.Padding1 = 0;
				header.SectionInfosSize = Marshal.SizeOf<SSGSection>() * DataSections.Count;
				header.SectionNamesSize = MathExtensions.AlignValue( nextNameOffset, _fileAlignment );

				DataSections.ForEach( section => { if ( !section.IsCompressed ) header.SectionDataSize += MathExtensions.AlignValue( section.RawData.Length, _fileAlignment ); } );
				compressedBlocks.ForEach( compressedBlock => { header.SectionDataSize += Convert.ToInt32( compressedBlock.Length ); } ); // these aren't aligned
				header.Padding2 = 0;
				header.CompressedBlocksSize = MathExtensions.AlignValue( sizeof( Int32 ) * compressedBlocks.Count, _fileAlignment );
				if ( _fileVersion == 5 )
					header.Alignment = 0; // version 5 doesn't appear to be storing alignment, and instead forces 16 byte alignment
				else
					header.Alignment = _fileAlignment;
				header.Padding3 = 0;

				using ( BinaryWriter writer = new BinaryWriter( bufferStream, Encoding.UTF8, true ) )
				{
					// write header
					writer.WriteType( header, _fileEndianness );
					writer.BaseStream.Position = MathExtensions.AlignValue( writer.BaseStream.Position, _fileAlignment );

					// write section infos
					foreach ( SSGSection section in sections )
						writer.WriteType( section, _fileEndianness );
					writer.BaseStream.Position = MathExtensions.AlignValue( writer.BaseStream.Position, _fileAlignment );

					// write compressed blocks
					foreach ( MemoryStream compressedBlock in compressedBlocks )
						writer.Write( Convert.ToInt32( compressedBlock.Length ), _fileEndianness );
					writer.BaseStream.Position = MathExtensions.AlignValue( writer.BaseStream.Position, _fileAlignment );

					// write section names
					foreach ( string sectionName in sectionNames.Keys )
					{
						writer.Write( Encoding.UTF8.GetBytes( sectionName ) );
						writer.Write( (byte)0 ); // null terminator
					}
					writer.BaseStream.Position = MathExtensions.AlignValue( writer.BaseStream.Position, _fileAlignment );

					// write actual data
					foreach ( DataSection section in DataSections )
					{
						// we only write uncompressed data here, compressed data comes later in chunks
						if ( section.IsCompressed )
							break;

						writer.Write( section.RawData );

						// align the file (in case we don't have compressed data)
						Int64 alignmentBytes = MathExtensions.AlignValue( writer.BaseStream.Position, _fileAlignment ) - writer.BaseStream.Position;
						if ( alignmentBytes > 0 )
							writer.Write( new byte[alignmentBytes] );
					}
					foreach ( MemoryStream compressedBlock in compressedBlocks )
						writer.Write( compressedBlock.GetBuffer(), 0, Convert.ToInt32( compressedBlock.Length ) );
					// compressed data is not aligned
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToSaveFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}
	}
}
