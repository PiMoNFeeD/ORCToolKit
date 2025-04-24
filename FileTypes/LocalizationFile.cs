using System.Numerics;
using System.Text;
using System.IO.Hashing;
using System.ComponentModel;

namespace ORCToolKit
{
	namespace FileTypes
	{
		public class LocalizationFile
		{
			public class TextEntry
			{
				public required string Name { get; set; }
				public required string Text { get; set; }

				public UInt32 GetHeaderOffset( UInt32 Alignment )
				{
					return 4 * (~Crc32.HashToUInt32( Encoding.UTF8.GetBytes( Name.ToLower() ) ) % Alignment);
				}
				public Int32 GetAlignedSize()
				{
					return AlignValue(
							4 +     // recursive text entry offset
							1 +     // length of text entry name
							AlignValue( Encoding.UTF8.GetByteCount( Name ), 2 ) +
							1 +     // null terminator
							Encoding.UTF8.GetByteCount( Text ) +
							1 +     // null terminator
							1       // rounded up for no reason???
							, 4 );  // aligned to 4 bytes
				}

				public UInt32 EntryOffset = 0;
				public TextEntry? RecursiveTextEntry = null;
			};

			public SuspendableBindingList<TextEntry> TextEntries = new SuspendableBindingList<TextEntry>();

			public void Reset()
			{
				TextEntries.Clear();
			}

			public void LoadFromFile( FileInfo LocalizationFileInfo )
			{
				Reset();
				TextEntries.Suspend();

				try
				{
					using ( FileStream File = LocalizationFileInfo.OpenRead() )
					{
						using ( BinaryReader Reader = new BinaryReader( File ) )
						{
							UInt32 NumOfEntriesAllocated = Reader.ReadUInt32();
							UInt32 MagicNumber = Reader.ReadUInt32();

							// check if this is a valid file
							if ( !MathExtensions.IsPowerOfTwo( NumOfEntriesAllocated ) || MagicNumber != 0 )
								throw new FileFormatException();

							Reader.BaseStream.Position = 8 + 4 * NumOfEntriesAllocated; // start of text entries
							while ( Reader.BaseStream.Position < File.Length )
							{
								UInt32 EntryOffset = Convert.ToUInt32( Reader.BaseStream.Position );

								Reader.BaseStream.Position += 4; // skip recursive offset to other string with the same crc32

								byte NameLength = Reader.ReadByte();
								string Name = Reader.ReadFixedLengthString( Encoding.UTF8, NameLength );
								Reader.BaseStream.Position++; // skip null terminator

								string Text = Reader.ReadNullTerminatedString( Encoding.UTF8 );

								TextEntry NewTextEntry = new TextEntry()
								{
									Name = Name,
									Text = Text,
									EntryOffset = EntryOffset,
								};
								TextEntries.Add( NewTextEntry );

								// align position to the next entry
								Reader.BaseStream.Position = AlignValue( Reader.BaseStream.Position + 1, 4 );
							}

							if ( TextEntries.Count < 1 )
								throw new FileFormatException();
						}
					}
				}
				catch ( Exception ex )
				{
					MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
				}

				TextEntries.Resume();
			}

			public void SaveToFile( FileInfo LocalizationFileInfo )
			{
				try
				{
					UInt32 NumOfEntriesAllocated = RoundUpToPowerOf2( Convert.ToUInt32( TextEntries.Count ) );
					UInt32 TotalFileLength = 8 + (4 * NumOfEntriesAllocated);

					Dictionary<uint, int> HeaderOffsetDictionary = new Dictionary<uint, int>();
					for ( int i = 0; i < TextEntries.Count; i++ )
					{
						// recalculate entry offset
						TextEntries[i].EntryOffset = Convert.ToUInt32( TotalFileLength );
						TotalFileLength += Convert.ToUInt32( TextEntries[i].GetAlignedSize() );

						// calculate recursive text entries in the most confusing way possible
						// for each text entry, check all entries that came before it backwards for colliding crc32 (header offset)...
						// ...which is VERY slow, so it's now a dictionary instead with just one loop :)
						TextEntries[i].RecursiveTextEntry = null;
						UInt32 HeaderOffset = TextEntries[i].GetHeaderOffset( NumOfEntriesAllocated );
						if ( HeaderOffsetDictionary.TryGetValue( HeaderOffset, out int Index ) )
							TextEntries[i].RecursiveTextEntry = TextEntries[Index];

						HeaderOffsetDictionary[HeaderOffset] = i;
					}

					using ( FileStream File = new FileStream( LocalizationFileInfo.FullName, FileMode.Create, FileAccess.Write ) )
					{
						File.SetLength( TotalFileLength );
						using ( BinaryWriter Writer = new BinaryWriter( File ) )
						{
							Writer.Write( NumOfEntriesAllocated );

							// write header offsets first
							foreach ( TextEntry TextEntry in TextEntries )
							{
								Writer.BaseStream.Position = 8 + TextEntry.GetHeaderOffset( NumOfEntriesAllocated );
								Writer.Write( TextEntry.EntryOffset );
							}

							// write entries
							Writer.BaseStream.Position = 8 + (4 * NumOfEntriesAllocated);
							foreach ( TextEntry TextEntry in TextEntries )
							{
								// recursive offset to a colliding crc32 entry name
								if ( TextEntry.RecursiveTextEntry != null )
									Writer.Write( TextEntry.RecursiveTextEntry.EntryOffset );
								else
									Writer.Write( 0 );

								// entry name length
								Int32 EntryNameLength = AlignValue( Encoding.UTF8.GetByteCount( TextEntry.Name ), 2 );
								Writer.Write( Convert.ToByte( EntryNameLength ) );

								// entry name
								Writer.Write( Encoding.UTF8.GetBytes( TextEntry.Name ), 0, Encoding.UTF8.GetByteCount( TextEntry.Name ) );
								for ( int i = TextEntry.Name.Length; i < EntryNameLength; i++ )
									Writer.Write( (byte)0 ); // alignment
								Writer.Write( (byte)0 ); // null terminator

								// entry text
								Int32 EntryTextLength = Encoding.UTF8.GetByteCount( TextEntry.Text );
								Writer.Write( Encoding.UTF8.GetBytes( TextEntry.Text ), 0, EntryTextLength );
								Writer.Write( (byte)0 ); // null terminator

								// align everything to 4 bytes
								Int32 BytesWrittenSoFar = 4 + 1 + EntryNameLength + 1 + EntryTextLength + 1;
								for ( int i = BytesWrittenSoFar; i < TextEntry.GetAlignedSize(); i++ )
									Writer.Write( (byte)0 ); // alignment
							}
						}
					}
				}
				catch ( Exception ex )
				{
					MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToSaveFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
			}

			private static T AlignValue<T>( T Value, T AlignTo ) where T : INumber<T>
			{
				return Value + ((AlignTo - Value % AlignTo) % AlignTo);
			}
			private static UInt32 RoundUpToPowerOf2( UInt32 Value )
			{
				// Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
				--Value;
				Value |= Value >> 1;
				Value |= Value >> 2;
				Value |= Value >> 4;
				Value |= Value >> 8;
				Value |= Value >> 16;
				return Value + 1;
			}
		}
	}
}
