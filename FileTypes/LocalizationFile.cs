using ORCToolKit.Extensions;
using System.Text;

namespace ORCToolKit.FileTypes
{
	public class LocalizationFile: IFileType
	{
		public class TextEntry
		{
			public required string Name { get; set; }
			public required string Text { get; set; }

			public UInt32 GetHeaderOffset( UInt32 alignment )
			{
				return 4 * (Name.ToLower().GetCRC32() % alignment);
			}
			public Int32 GetAlignedSize()
			{
				return MathExtensions.AlignValue(
						4 +     // recursive text entry offset
						1 +     // length of text entry name
						MathExtensions.AlignValue( Encoding.UTF8.GetByteCount( Name ), 2 ) +
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

		public void LoadFromBuffer( Stream bufferStream )
		{
			Reset();
			TextEntries.Suspend();

			try
			{
				using ( BinaryReader reader = new BinaryReader( bufferStream ) )
				{
					UInt32 numOfEntriesAllocated = reader.ReadUInt32();
					UInt32 magicNumber = reader.ReadUInt32();

					// check if this is a valid file
					if ( !MathExtensions.IsPowerOfTwo( numOfEntriesAllocated ) || magicNumber != 0 )
						throw new FileFormatException();

					reader.BaseStream.Position = 8 + 4 * numOfEntriesAllocated; // start of text entries
					while ( reader.BaseStream.Position < bufferStream.Length )
					{
						UInt32 entryOffset = Convert.ToUInt32( reader.BaseStream.Position );

						reader.BaseStream.Position += 4; // skip recursive offset to other string with the same crc32

						byte nameLength = reader.ReadByte();
						string name = reader.ReadFixedLengthString( Encoding.UTF8, nameLength );
						reader.BaseStream.Position++; // skip null terminator

						string text = reader.ReadNullTerminatedString( Encoding.UTF8 );

						TextEntry newTextEntry = new TextEntry()
						{
							Name = name,
							Text = text,
							EntryOffset = entryOffset,
						};
						TextEntries.Add( newTextEntry );

						// align position to the next entry
						reader.BaseStream.Position = MathExtensions.AlignValue( reader.BaseStream.Position + 1, 4 );
					}

					if ( TextEntries.Count < 1 )
						throw new FileFormatException();
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			finally
			{
				TextEntries.Resume();
			}
		}

		public void SaveToBuffer( Stream bufferStream )
		{
			try
			{
				UInt32 numOfEntriesAllocated = MathExtensions.RoundUpToPowerOf2( Convert.ToUInt32( TextEntries.Count ) );
				UInt32 totalFileLength = 8 + (4 * numOfEntriesAllocated);

				Dictionary<uint, int> headerOffsetDictionary = new Dictionary<uint, int>();
				for ( int i = 0; i < TextEntries.Count; i++ )
				{
					// recalculate entry offset
					TextEntries[i].EntryOffset = Convert.ToUInt32( totalFileLength );
					totalFileLength += Convert.ToUInt32( TextEntries[i].GetAlignedSize() );

					// calculate recursive text entries in the most confusing way possible
					// for each text entry, check all entries that came before it backwards for colliding crc32 (header offset)...
					// ...which is VERY slow, so it's now a dictionary instead with just one loop :)
					TextEntries[i].RecursiveTextEntry = null;
					UInt32 headerOffset = TextEntries[i].GetHeaderOffset( numOfEntriesAllocated );
					if ( headerOffsetDictionary.TryGetValue( headerOffset, out int index ) )
						TextEntries[i].RecursiveTextEntry = TextEntries[index];

					headerOffsetDictionary[headerOffset] = i;
				}

				bufferStream.SetLength( totalFileLength );
				using ( BinaryWriter writer = new BinaryWriter( bufferStream, Encoding.UTF8, true ) )
				{
					writer.Write( numOfEntriesAllocated );

					// write header offsets first
					foreach ( TextEntry textEntry in TextEntries )
					{
						writer.BaseStream.Position = 8 + textEntry.GetHeaderOffset( numOfEntriesAllocated );
						writer.Write( textEntry.EntryOffset );
					}

					// write entries
					writer.BaseStream.Position = 8 + (4 * numOfEntriesAllocated);
					foreach ( TextEntry textEntry in TextEntries )
					{
						// recursive offset to a colliding crc32 entry name
						if ( textEntry.RecursiveTextEntry != null )
							writer.Write( textEntry.RecursiveTextEntry.EntryOffset );
						else
							writer.Write( 0 );

						// entry name length
						Int32 entryNameLength = MathExtensions.AlignValue( Encoding.UTF8.GetByteCount( textEntry.Name ), 2 );
						writer.Write( Convert.ToByte( entryNameLength ) );

						// entry name
						writer.Write( Encoding.UTF8.GetBytes( textEntry.Name ) );
						for ( int i = textEntry.Name.Length; i < entryNameLength; i++ )
							writer.Write( (byte)0 ); // alignment
						writer.Write( (byte)0 ); // null terminator

						// entry text
						writer.Write( Encoding.UTF8.GetBytes( textEntry.Text ) );
						writer.Write( (byte)0 ); // null terminator

						// align everything to 4 bytes
						Int32 bytesWrittenSoFar = 4 + 1 + entryNameLength + 1 + Encoding.UTF8.GetByteCount( textEntry.Text ) + 1;
						writer.BaseStream.Position += textEntry.GetAlignedSize() - bytesWrittenSoFar - 1;
						writer.Write( (byte)0 );
					}
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToSaveFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}
	}
}
