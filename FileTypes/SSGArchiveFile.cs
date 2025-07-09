using System.Text;
using System.IO.Hashing;
using System.IO.Compression;
using System.Runtime.InteropServices;
using ORCToolKit.Extensions;

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
			public Int32 NameCRC32; // -crc32 - 1
			public Int32 NameOffset; // relative to section names
			public Int32 UncompressedSize;
			public Int32 Unknown; // appears to always be 1

			public Int32 DataOffset; // only reliable for uncompressed, for compressed need to use the size of all uncompressed sections that came before this one
			public UInt32 Type;
			public Int32 UncompressedCRC32; // -crc32 - 1
			public Int32 CompressedSize; // this might not actually be the size of one section, but a size of multiple compressed sections
		}

		public class DataSection
		{
			public string Name = string.Empty;
			public Int32 NameCRC32 = 0; // TODO: should NOT be stored, but in some instances it does not match the actual CRC32 of a name string!
			public byte[] RawData = Array.Empty<byte>();
			public UInt32 Type = 0;
			public bool IsCompressed = false;
		}

		private UInt32 FileVersion = 6;
		private UInt16 FileAlignment = 16;
		private Endianness FileEndianness = Endianness.Little;
		private bool SplitCompression = false; // by default, assume we don't split, unless the file we open does split
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

		public void LoadFromBuffer( Stream BufferStream )
		{
			Reset();

			FileStream? LogFile = null;
			StreamWriter? LogWriter = null;
#if DEBUG
			if ( BufferStream is FileStream BufferFileStream )
			{
				LogFile = File.OpenWrite( BufferFileStream.Name + "_log" );
				LogWriter = new StreamWriter( LogFile );
			}
#endif

			try
			{
				using ( BinaryReader Reader = new BinaryReader( BufferStream, Encoding.UTF8, true ) )
				{
					// determine endianness, the way the game does it
					SSGHeader Header = Reader.ReadType<SSGHeader>();
					FileEndianness = Header.Version > 0xFFFF ? Endianness.Big : Endianness.Little;

					// read header again
					Reader.BaseStream.Position -= Marshal.SizeOf<SSGHeader>();
					Header = Reader.ReadType<SSGHeader>(FileEndianness);
					if ( Header.Version != 6 && Header.Version != 5 ) // RE:ORC only has version 5 and 6 archives
						throw new FileFormatException();

					if ( Header.Padding1 != 0 ) // make sure it's 0 to know we have the right file
						throw new FileFormatException();

					FileVersion = Header.Version;
					if ( FileVersion == 5 )
					{
						FileAlignment = 16; // this line of code is brought to you by reverse-engineering the game
						Header.CompressedBlocksSize = 0; // speculation, it's not 0 but there is no compression
					}

					LogWriter?.WriteLine( "SSG Header:" );
					LogWriter?.WriteLine( $"Version:              {Header.Version}" );
					LogWriter?.WriteLine( $"SectionInfosSize:     {Header.SectionInfosSize}" );
					LogWriter?.WriteLine( $"SectionNamesSize:     {Header.SectionNamesSize}" );
					LogWriter?.WriteLine( $"SectionDataSize:      {Header.SectionDataSize}" );
					LogWriter?.WriteLine( $"CompressedBlocksSize: {Header.CompressedBlocksSize}" );
					LogWriter?.WriteLine( $"Alignment:            {Header.Alignment}\n" );

					// TODO: console archives are using unknown compression, but it's the same between PS3 and XBOX 360
					if ( FileEndianness == Endianness.Big && Header.CompressedBlocksSize > 0 )
						throw new FileFormatException( Resources.ORCToolKit.FileSSGArchiveFileCompressedConsoleNotSupported );

					// read compressed blocks
					// they come after section infos, but we need them earlier to be able to decompress data
					List<Int32> CompressedBlockSizesList = new List<Int32>();
					Reader.BaseStream.Position = Marshal.SizeOf<SSGHeader>() + Header.SectionInfosSize;
					for ( Int32 BytesRead = 0; BytesRead < Header.CompressedBlocksSize; BytesRead += 4 )
					{
						Int32 Size = Reader.ReadInt32( FileEndianness );
						if ( Size == 0 )
							break;
						CompressedBlockSizesList.Add( Size );
					}

					// read sections last, after we know compressed block sizes, to decompress them on the fly
					if ( Header.SectionInfosSize % Marshal.SizeOf<SSGSection>() != 0 )
						throw new FileFormatException();
					Int32 SectionCount = Header.SectionInfosSize / Marshal.SizeOf<SSGSection>();

					// compressed data comes right after uncompressed, and we need to know when it actually starts
					Int32 CompressedDataOffset = 0;
					List<byte> DecompressedData = new List<byte>();

					Int32 LastNegativeCompressedSize = 0;
					for ( int i = 0; i < SectionCount; i++ )
					{
						Reader.BaseStream.Position = Marshal.SizeOf<SSGHeader>() + Marshal.SizeOf<SSGSection>() * i;

						SSGSection Section = Reader.ReadType<SSGSection>( FileEndianness );

						// this is a very dumb hack to determine whether we need to split compressed blocks of sections or not
						// doesn't seem to actually affect whether the file loads or not by the game, but I want num of compressed blocks
						// to match the original file!
						if ( Section.CompressedSize < 0 )
						{
							if ( LastNegativeCompressedSize == 0 )
								LastNegativeCompressedSize = Section.CompressedSize;
							else if ( LastNegativeCompressedSize != Section.CompressedSize )
								SplitCompression = true;
						}

						// this block of code is brought to you by reverse-engineering the game
						if ( Header.CompressedBlocksSize == 0 )
							Section.CompressedSize = 0;

						DataSection DataSection = new DataSection();
						DataSection.Type = Section.Type;
						DataSection.IsCompressed = (Section.CompressedSize != 0); // it can be negative, which means it shares a compressed block with the previous section

						// read file name
						Reader.BaseStream.Position = Marshal.SizeOf<SSGHeader>() + Header.SectionInfosSize + Header.CompressedBlocksSize + Section.NameOffset;
						DataSection.Name = Reader.ReadNullTerminatedString( Encoding.UTF8 );
						DataSection.NameCRC32 = Section.NameCRC32;

						if ( DataSection.IsCompressed )
						{
							if ( DecompressedData.Count == 0 )
							{
								// uncompress all compressed blocks
								Int32 CompressedDataReadSoFar = 0;
								foreach ( Int32 CompressedBlockSize in CompressedBlockSizesList )
								{
									// need to open a new zlibstream every time, because reading one block results in the whole stream being marked as "finished"
									using ( ZLibStream CompressionStream = new ZLibStream( BufferStream, CompressionMode.Decompress, true ) )
									{
										// guess the position of the next compressed block in file
										CompressionStream.BaseStream.Position = Marshal.SizeOf<SSGHeader>() +
											Header.SectionInfosSize +
											Header.CompressedBlocksSize +
											Header.SectionNamesSize +
											CompressedDataOffset + CompressedDataReadSoFar;

										using ( MemoryStream DecompressedStream = new MemoryStream() )
										{
											CompressionStream.CopyTo( DecompressedStream );
											DecompressedData.AddRange( DecompressedStream.GetBuffer() );
											LogWriter?.WriteLine( $"Compressed block: compressed {CompressedBlockSize}, decompressed {DecompressedStream.Length}" );
										}

										CompressedDataReadSoFar += CompressedBlockSize;
									}
								}
								CompressedDataOffset = 0; // revert this to zero, as it's now going to be an offset within compressed data, not within all data
							}

							DataSection.RawData = new byte[Section.UncompressedSize];
							DecompressedData.CopyTo( CompressedDataOffset, DataSection.RawData, 0, Section.UncompressedSize );
						}
						else
						{
							Reader.BaseStream.Position = Marshal.SizeOf<SSGHeader>() +
								Header.SectionInfosSize + Header.CompressedBlocksSize + Header.SectionNamesSize + Section.DataOffset;
							DataSection.RawData = Reader.ReadBytes( Section.UncompressedSize );
						}

						// do not rely on data offset for compressed sections: data is often shared between multiple sections
						// and can point to wrong offsets, so use uncompressed size instead
						CompressedDataOffset += MathExtensions.AlignValue( Section.UncompressedSize, FileAlignment );

						LogWriter?.WriteLine( $"\nSection {DataSections.Count + 1}:" );
						LogWriter?.WriteLine( $"NameCRC32:         {Section.NameCRC32}" );
						LogWriter?.WriteLine( $"NameOffset:        {Section.NameOffset}" );
						LogWriter?.WriteLine( $"UncompressedSize:  {Section.UncompressedSize}" );
						LogWriter?.WriteLine( $"DataOffset:        {Section.DataOffset}" );
						LogWriter?.WriteLine( $"Type:              {Section.Type}" );
						LogWriter?.WriteLine( $"UncompressedCRC32: {Section.UncompressedCRC32}" );
						LogWriter?.WriteLine( $"CompressedSize:    {Section.CompressedSize}" );

						DataSections.Add( DataSection );
					}
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			finally
			{
				LogWriter?.Close();
				LogFile?.Close();
			}
		}

		public void SaveToBuffer( Stream BufferStream )
		{
			try
			{
				List<SSGSection> Sections = new List<SSGSection>( DataSections.Count );
				Dictionary<string, Int32> SectionNames = new Dictionary<string, Int32>();
				Int32 NextNameOffset = 0;

				foreach ( DataSection DataSection in DataSections )
				{
					// fill name dictionary
					if ( !SectionNames.TryGetValue( DataSection.Name, out Int32 NameOffset ) )
					{
						SectionNames.Add( DataSection.Name, NextNameOffset );
						NameOffset = NextNameOffset;
						NextNameOffset += Encoding.UTF8.GetByteCount( DataSection.Name ) + 1;
					}
				}

				// remember this data to compress sections on the fly in one pass
				Int32 ParentCompressedSectionIndex = -1;
				Int32 NumCompressedBlocksProcessed = 0;
				List<MemoryStream> CompressedBlocks = new List<MemoryStream>();
				MemoryStream DataToCompress = new MemoryStream( MaxCompressionSize );

				// in case there are multiple sections named the same, we need to split compression precisely where we meet a name that already exists
				// particularly cutscene files, because they store raw animation data for each object in the scene, split in groups of frames
				HashSet<string> CompressedSectionsWritten = new HashSet<string>();

				Int32 NextDataOffset = 0;
				for ( Int32 SectionIndex = 0; SectionIndex < DataSections.Count; SectionIndex++ )
				{
					SSGSection Section = new SSGSection();
					//Section.NameCRC32 = -(Int32)Crc32.HashToUInt32( Encoding.UTF8.GetBytes( DataSections[SectionIndex].Name ) ) - 1; // TODO: UI/Gallery.ssg mismatch for GalleryImages section!
					Section.NameCRC32 = DataSections[SectionIndex].NameCRC32;
					Section.NameOffset = SectionNames[DataSections[SectionIndex].Name];
					Section.UncompressedSize = DataSections[SectionIndex].RawData.Length;
					Section.Unknown = 1;

					Section.Type = DataSections[SectionIndex].Type;
					Section.UncompressedCRC32 = -(Int32)Crc32.HashToUInt32( DataSections[SectionIndex].RawData.ToArray() ) - 1;

					if ( DataSections[SectionIndex].IsCompressed )
					{
						// group together sections up to 4 compressed chunks that will all share one compressed size, then split
						if ( SplitCompression && ParentCompressedSectionIndex != -1 )
						{
							//bool SplitData = (DataToCompress.Length != 0) && (DataToCompress.Length + MathExtensions.AlignValue( Section.UncompressedSize, FileAlignment ) > DataToCompress.Capacity);
							bool SplitData = CompressedSectionsWritten.Contains( DataSections[SectionIndex].Name );
							if ( SplitData )
							{
								// compress data now
								Int32 CompressedSizeForCurrentChunkBlock = 0;
								DataToCompress.Position = 0;
								while ( DataToCompress.Position < DataToCompress.Length )
								{
									MemoryStream CompressedBlock = new MemoryStream();
									using ( ZLibStream CompressedStream = new ZLibStream( CompressedBlock, CompressionLevel.Fastest, true ) )
									{
										byte[] DataChunk = new byte[CompressionSize];
										Int32 BytesInChunk = DataToCompress.Read( DataChunk, 0, CompressionSize );
										CompressedStream.Write( DataChunk, 0, BytesInChunk );
									}
									CompressedSizeForCurrentChunkBlock += Convert.ToInt32( CompressedBlock.Length );
									CompressedBlocks.Add( CompressedBlock );
								}

								Sections[ParentCompressedSectionIndex].CompressedSize = CompressedSizeForCurrentChunkBlock;
								Sections[ParentCompressedSectionIndex].DataOffset = NextDataOffset;
								NextDataOffset += CompressedSizeForCurrentChunkBlock;

								// mark all previous sections to share compressed data with parent
								for ( Int32 CompressedSectionIndex = ParentCompressedSectionIndex + 1; CompressedSectionIndex < SectionIndex; CompressedSectionIndex++ )
								{
									Sections[CompressedSectionIndex].DataOffset = NextDataOffset;
									// this is wrong, but it seems to work in-game, as long as it's negative and a unique value for each batch
									// which means it might not actually matter what the value is, as long as it's unique..?
									Sections[CompressedSectionIndex].CompressedSize = -NumCompressedBlocksProcessed - 1;
								}

								// clear all data for the next block of compressed sections
								CompressedSectionsWritten.Clear();
								DataToCompress.SetLength( 0 );
								DataToCompress.Position = 0;
								ParentCompressedSectionIndex = -1;
								NumCompressedBlocksProcessed++;
							}
						}

						if ( ParentCompressedSectionIndex == -1 )
							ParentCompressedSectionIndex = SectionIndex;

						DataToCompress.Write( DataSections[SectionIndex].RawData );
						CompressedSectionsWritten.Add( DataSections[SectionIndex].Name );

						// align it
						Int64 AlignmentBytes = DataToCompress.Position - MathExtensions.AlignValue( DataToCompress.Position, FileAlignment );
						if ( AlignmentBytes > 0 )
							DataToCompress.Write( new byte[AlignmentBytes] );
					}
					else
					{
						Section.DataOffset = NextDataOffset;
						Section.CompressedSize = 0;
						NextDataOffset += MathExtensions.AlignValue( Section.UncompressedSize, FileAlignment );
					}

					Sections.Add( Section );
				}

				// compress whatever data we have that didn't go beyond MaxCompressionSize
				if ( ParentCompressedSectionIndex != -1 )
				{
					Int32 CompressedSizeForCurrentChunkBlock = 0;
					DataToCompress.Position = 0;
					while ( DataToCompress.Position < DataToCompress.Length )
					{
						MemoryStream CompressedBlock = new MemoryStream();
						using ( ZLibStream CompressedStream = new ZLibStream( CompressedBlock, CompressionLevel.Fastest, true ) )
						{
							byte[] DataChunk = new byte[CompressionSize];
							Int32 BytesInChunk = DataToCompress.Read( DataChunk, 0, CompressionSize );
							CompressedStream.Write( DataChunk, 0, BytesInChunk );
						}
						CompressedSizeForCurrentChunkBlock += Convert.ToInt32( CompressedBlock.Length );
						CompressedBlocks.Add( CompressedBlock );
					}

					Sections[ParentCompressedSectionIndex].CompressedSize = CompressedSizeForCurrentChunkBlock;
					Sections[ParentCompressedSectionIndex].DataOffset = NextDataOffset;
					NextDataOffset += CompressedSizeForCurrentChunkBlock;

					// mark all previous sections to share compressed data with parent
					for ( Int32 CompressedSectionIndex = ParentCompressedSectionIndex + 1; CompressedSectionIndex < Sections.Count; CompressedSectionIndex++ )
					{
						Sections[CompressedSectionIndex].DataOffset = NextDataOffset;
						// this is wrong, but it seems to work in-game, as long as it's negative and a unique value for each batch
						// which means it might not actually matter what the value is, as long as it's unique..?
						Sections[CompressedSectionIndex].CompressedSize = -NumCompressedBlocksProcessed - 1;
					}
				}

				SSGHeader Header = new SSGHeader();
				Header.Version = FileVersion;
				Header.Padding1 = 0;
				Header.SectionInfosSize = Marshal.SizeOf<SSGSection>() * DataSections.Count;
				Header.SectionNamesSize = MathExtensions.AlignValue( NextNameOffset, FileAlignment );

				DataSections.ForEach( Section => { if ( !Section.IsCompressed ) Header.SectionDataSize += MathExtensions.AlignValue( Section.RawData.Length, FileAlignment ); } );
				CompressedBlocks.ForEach( CompressedBlock => { Header.SectionDataSize += Convert.ToInt32( CompressedBlock.Length ); } ); // these aren't aligned
				Header.Padding2 = 0;
				Header.CompressedBlocksSize = MathExtensions.AlignValue( sizeof( Int32 ) * CompressedBlocks.Count, FileAlignment );
				if ( FileVersion == 5 )
					Header.Alignment = 0; // version 5 doesn't appear to be storing alignment, and instead forces 16 byte alignment
				else
					Header.Alignment = FileAlignment;
				Header.Padding3 = 0;

				using ( BinaryWriter Writer = new BinaryWriter( BufferStream, Encoding.UTF8, true ) )
				{
					// write header
					Writer.WriteType( Header, FileEndianness );
					Writer.BaseStream.Position = MathExtensions.AlignValue( Writer.BaseStream.Position, FileAlignment );

					// write section infos
					foreach ( SSGSection Section in Sections )
						Writer.WriteType( Section, FileEndianness );
					Writer.BaseStream.Position = MathExtensions.AlignValue( Writer.BaseStream.Position, FileAlignment );

					// write compressed blocks
					foreach ( MemoryStream CompressedBlock in CompressedBlocks )
						Writer.Write( Convert.ToInt32( CompressedBlock.Length ), FileEndianness );
					Writer.BaseStream.Position = MathExtensions.AlignValue( Writer.BaseStream.Position, FileAlignment );

					// write section names
					foreach ( string SectionName in SectionNames.Keys )
					{
						Writer.Write( Encoding.UTF8.GetBytes( SectionName ) );
						Writer.Write( (byte)0 ); // null terminator
					}
					Writer.BaseStream.Position = MathExtensions.AlignValue( Writer.BaseStream.Position, FileAlignment );

					// write actual data
					foreach ( DataSection Section in DataSections )
					{
						// we only write uncompressed data here, compressed data comes later in chunks
						if ( Section.IsCompressed )
							break;

						Writer.Write( Section.RawData );

						// align the file (in case we don't have compressed data)
						Int64 AlignmentBytes = Writer.BaseStream.Position - MathExtensions.AlignValue( Writer.BaseStream.Position, FileAlignment );
						if ( AlignmentBytes > 0 )
							Writer.Write( new byte[AlignmentBytes] );
					}
					foreach ( MemoryStream CompressedBlock in CompressedBlocks )
						Writer.Write( CompressedBlock.GetBuffer(), 0, Convert.ToInt32( CompressedBlock.Length ) );
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
