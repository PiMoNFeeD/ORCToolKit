using ORCToolKit.Extensions;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ORCToolKit.FileTypes
{
	public class SWB2DatabaseFile: IFileType
	{
		[TypeConverter( typeof( OutOfBoundsEnumConverter ) )]
		public enum EntryValueType: Byte
		{
			TYPE_INT32_STORED = 1,  // (u?)int32 that is stored in the file
			TYPE_FLOAT_STORED = 2,  // float that is stored in the file
			TYPE_STRING = 3,        // string that is stored in the file
									// I imagine a TYPE_BYTE_STORED would be somewhere here, but there isn't a single asset with a byte type in the game to check...
			TYPE_ENTRY_TABLE = 6,   // a table that stores key-value entries
			TYPE_INT32_ZERO = 7,    // (u?)int32 that is always zero, not stored in the file (0)
			TYPE_INT32_ONE = 8,     // (u?)int32 that is always one, not stored in the file (1)
			TYPE_FLOAT_ZERO = 9,    // float that is always zero, not stored in the file (0.0)
			TYPE_FLOAT_ONE = 10,    // float that is always one, not stored in the file (1.0)
			TYPE_BYTE_ZERO = 11,    // byte that is always zero, not stored in the file (false)
			TYPE_BYTE_ONE = 12,     // byte that is always one, not stored in the file (true)
			TYPE_STRING_HASH = 13,	// the code path that reads this type either reads TYPE_STRING and then extracts a CRC32 of that string,
									// or reads a uint32 directly; this appears correct after inspecting assets with this type
		};
		private const Int32 StringAlignment = 4;

		// NOTE: sign-ness in this structure do not represent whether the actual data
		// in the file is signed/unsigned, it's only to make code complain less and having to do less Convert.To(U)Int32 calls
		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		private class SWB2Header
		{
			public UInt32 Magic1;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
			public Byte[] Unknown1;
			public UInt32 Unknown2; // possibly alignment? but the tables don't appear to be aligned..?
			public UInt32 Unknown3; // always zero?

			public UInt32 Magic2;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 )]
			public UInt32[] Unknown4; // always zero?

			public Int32 SizeOfTableData;
			public Int32 NumTables;
		};

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		private class SWB2EntryTableHeader
		{
			public UInt32 Identifier;
			public Int32 NumEntries;
		};

		[StructLayout( LayoutKind.Sequential, Pack = 1 )]
		private class SWB2EntryHeader
		{
			public UInt32 Key; // this is CRC32 of a string :(
			public EntryValueType ValueType;
			public Byte Unknown; // flags? so far I've seen only 1, 3 and 4 as values (likely bit flags), with 3 and 4 exclusive to TYPE_FLOAT_STORED (characters.swb2)
			public UInt16 ValueSize;
		};

		public class Entry
		{
			private EntryValueType _valueType = EntryValueType.TYPE_BYTE_ZERO;

			public Entry( EntryTable? Parent ) { ParentTable = Parent; }

			[Browsable( false )]
			public EntryTable? ParentTable { get; private set; }

			[LocalizedDisplayName( "SWB2DatabaseFile.Entry.Key.Name" ), LocalizedDescription( "SWB2DatabaseFile.Entry.Key.Description" )]
			public CRC32 Key { get; set; } = 0;

			[LocalizedDisplayName( "SWB2DatabaseFile.Entry.ValueType.Name" ), LocalizedDescription( "SWB2DatabaseFile.Entry.ValueType.Description" )]
			public EntryValueType ValueType
			{
				get { return _valueType; }
				set
				{
					_valueType = value;

					switch (_valueType )
					{
						case EntryValueType.TYPE_INT32_STORED:
							Value = new Int32[1] { 0 };
							break;
						case EntryValueType.TYPE_FLOAT_STORED:
							Value = new Single[1] { 0.0f };
							break;
						case EntryValueType.TYPE_STRING:
							Value = new string[1] { string.Empty };
							break;
						case EntryValueType.TYPE_ENTRY_TABLE:
							Value = new EntryTable( ParentTable );
							break;
						case EntryValueType.TYPE_STRING_HASH:
							Value = new CRC32[1] { 0 };
							break;
						case EntryValueType.TYPE_INT32_ZERO:
						case EntryValueType.TYPE_INT32_ONE:
						case EntryValueType.TYPE_FLOAT_ZERO:
						case EntryValueType.TYPE_FLOAT_ONE:
						case EntryValueType.TYPE_BYTE_ZERO:
						case EntryValueType.TYPE_BYTE_ONE:
							Value = null;
							return;
						default:
							Value = new Byte[1] { 0 };
							break;
					}
				}
			}

			[LocalizedDisplayName( "SWB2DatabaseFile.Entry.Flags.Name" ), LocalizedDescription( "SWB2DatabaseFile.Entry.Flags.Description" ), ReadOnly( true )]
			public Byte Flags { get; set; } = 0;

			// hack for UI... couldn't get ArrayEditor to work properly with type 'object' for Value
			[LocalizedDisplayName( "SWB2DatabaseFile.Entry.ValueLength.Name" )]
			public Int32 ValueLength
			{
				get
				{
					if ( Value is Array ArrayValue )
						return ArrayValue.Length;

					return 0;
				}
				set
				{
					Int32 NewValueLength = value;
					if ( NewValueLength < 1 )
						NewValueLength = 1;

					if ( Value is Array ArrayValue )
					{
						// resize the array by creating a new one
						Array NewArrayValue = Array.CreateInstance( ArrayValue.GetType().GetElementType(), NewValueLength );
						Array.Copy( ArrayValue, NewArrayValue, Math.Min( ValueLength, NewValueLength ) );
						Value = NewArrayValue;
					}
				}
			}

			// this will always be an array for UI; if type is TYPE_ENTRY_TABLE, it will be class EntryTable instead of Entry!
			[TypeConverter( typeof( ArrayConverter ) )]
			[LocalizedDisplayName( "SWB2DatabaseFile.Entry.Value.Name" )]
			public object? Value { get; set; } = null;

#if !DEBUG
			[Browsable( false )]
#endif
			public string ValueAsString
			{
				get
				{
					string Result = string.Empty;

					const Int32 MaxElementsToDisplay = 10;
					switch ( ValueType )
					{
						case EntryValueType.TYPE_INT32_STORED:
						case EntryValueType.TYPE_FLOAT_STORED:
						case EntryValueType.TYPE_STRING:
						case EntryValueType.TYPE_STRING_HASH:
						{
							Debug.Assert( Value != null );

							Array Values = Value as Array;
							if ( Values.Length == 1 )
							{
								Result += Values.GetValue( 0 );
							}
							else
							{
								Result += "[";
								Int32 Length = Math.Min( MaxElementsToDisplay, Values.Length );
								for ( Int32 i = 0; i < Length; i++ )
								{
									Result += Values.GetValue( i );
									if ( i == (Length - 1) )
									{
										if ( Length < Values.Length )
											Result += ":...";
									}
									else
									{
										Result += StringValueSeparator;
									}
								}
								Result += "]";
							}
							break;
						}
						case EntryValueType.TYPE_ENTRY_TABLE:
						{
							Result += Value.ToString();
							break;
						}
						// this weird logic allows me to automatically propogate default values without having to worry about manually typing them out
						case EntryValueType.TYPE_INT32_ZERO:
						case EntryValueType.TYPE_INT32_ONE:
						{
							Debug.Assert( Value == null );
							Result += Convert.ToInt32( ValueType - EntryValueType.TYPE_INT32_ZERO );
							break;
						}
						case EntryValueType.TYPE_FLOAT_ZERO:
						case EntryValueType.TYPE_FLOAT_ONE:
						{
							Debug.Assert( Value == null );
							Result += Convert.ToSingle( ValueType - EntryValueType.TYPE_FLOAT_ZERO );
							break;
						}
						case EntryValueType.TYPE_BYTE_ZERO:
						case EntryValueType.TYPE_BYTE_ONE:
						{
							Debug.Assert( Value == null );
							Result += Convert.ToBoolean( ValueType - EntryValueType.TYPE_BYTE_ZERO );
							break;
						}
						default:
						{
							if ( Value != null )
								goto case EntryValueType.TYPE_STRING_HASH; // don't copy paste code
							break;
						}
					}

					return Result;
				}
			}

			public Char StringValueSeparator = ':'; // for some FUCKING reason, some strings are separated by ':', and some by ';'?!?!?!?!?

			[Browsable( false )]
			public Int32 ValueSize
			{
				get
				{
					switch ( ValueType )
					{
						case EntryValueType.TYPE_INT32_STORED:
						{
							Debug.Assert( Value != null );
							return Marshal.SizeOf<Int32>() * (Value as Int32[]).Length;
						}
						case EntryValueType.TYPE_FLOAT_STORED:
						{
							Debug.Assert( Value != null );
							return Marshal.SizeOf<Int32>() * (Value as Single[]).Length;
						}
						case EntryValueType.TYPE_STRING:
						{
							Debug.Assert( Value != null );
							string FinalString = string.Empty;
							string[] Strings = (Value as string[]);
							for ( Int32 i = 0; i < Strings.Length; i++ )
							{
								FinalString += Strings[i];
								if ( i != (Strings.Length - 1) )
									FinalString += StringValueSeparator;
							}
							return MathExtensions.AlignValue( Encoding.UTF8.GetByteCount( FinalString ) + 1, StringAlignment ); // +1 for null terminator
						}
						case EntryValueType.TYPE_ENTRY_TABLE:
						{
							Debug.Assert( Value != null );
							Int32 Size = Marshal.SizeOf<SWB2EntryTableHeader>();
							foreach ( Entry Entry in (Value as EntryTable).Entries )
							{
								Size += Marshal.SizeOf<SWB2EntryHeader>();
								Size += Entry.ValueSize;
							}
							return Size;
						}
						case EntryValueType.TYPE_STRING_HASH:
						{
							Debug.Assert( Value != null );
							return Marshal.SizeOf<CRC32>() * (Value as CRC32[]).Length;
						}
						case EntryValueType.TYPE_INT32_ZERO:
						case EntryValueType.TYPE_INT32_ONE:
						case EntryValueType.TYPE_FLOAT_ZERO:
						case EntryValueType.TYPE_FLOAT_ONE:
						case EntryValueType.TYPE_BYTE_ZERO:
						case EntryValueType.TYPE_BYTE_ONE:
						{
							Debug.Assert( Value == null );
							return 0;
						}
						default:
						{
							if ( Value != null )
								return (Value as Byte[]).Length;
							return 0;
						}
					}
				}
			}

			public void LoadFromBuffer( Stream BufferStream, Endianness Endian )
			{
				using ( BinaryReader Reader = new BinaryReader( BufferStream, Encoding.UTF8, true ) )
				{
					Int32 ValueSize = 0;
					{
						UInt32 Key = Reader.ReadUInt32( Endian );
						if ( Key == 2952159793 )
						{
							// HACK: for some reason, entries with this key store a different layout?!?!?
							// according to the disassembly of the game, it doesn't even parse this entry, it just skips it!
							// possibly some debug info of the file?
							this.Key = Key;
							this.ValueType = EntryValueType.TYPE_INT32_STORED; // all values appear to be a mix of ints and floats, always byteswaped in batches of 4 bytes... slamming to int32 just so byteswapping is correct
							this.Flags = 0;
							ValueSize = Reader.ReadInt32( Endian );
						}
						else
						{
							Reader.BaseStream.Position -= Marshal.SizeOf<UInt32>();

							SWB2EntryHeader EntryHeader = Reader.ReadType<SWB2EntryHeader>( Endian );
							this.Key = EntryHeader.Key;
							this.ValueType = EntryHeader.ValueType;
							this.Flags = EntryHeader.Unknown;
							ValueSize = EntryHeader.ValueSize;
						}
					}

					if ( ValueSize > 0 )
					{
						switch ( this.ValueType )
						{
							case EntryValueType.TYPE_INT32_STORED:
							{
								if ( (ValueSize % Marshal.SizeOf<Int32>()) != 0 )
									throw new FileFormatException();

								this.Value = Reader.ReadInt32Array( ValueSize / Marshal.SizeOf<Int32>(), Endian );
								break;
							}
							case EntryValueType.TYPE_FLOAT_STORED:
							{
								if ( (ValueSize % Marshal.SizeOf<Single>()) != 0 )
									throw new FileFormatException();

								this.Value = Reader.ReadSingleArray( ValueSize / Marshal.SizeOf<Single>(), Endian );
								break;
							}
							case EntryValueType.TYPE_STRING:
							{
								string Value = Reader.ReadFixedLengthString( Encoding.UTF8, ValueSize );
								if ( Value.Contains( ';' ) )
									StringValueSeparator = ';';
								this.Value = Value.Split( StringValueSeparator );
								break;
							}
							case EntryValueType.TYPE_ENTRY_TABLE:
							{
								Int64 OldPosition = Reader.BaseStream.Position;
								EntryTable Value = new EntryTable( ParentTable );
								Value.LoadFromBuffer( BufferStream, Endian );
								this.Value = Value;

								if ( (Reader.BaseStream.Position - OldPosition) != ValueSize )
									throw new FileFormatException();
								break;
							}
							case EntryValueType.TYPE_STRING_HASH:
							{
								if ( (ValueSize % Marshal.SizeOf<CRC32>()) != 0 )
									throw new FileFormatException();

								this.Value = Reader.ReadTypeArray<CRC32>( ValueSize / Marshal.SizeOf<CRC32>(), Endian );
								break;
							}
							default:
							{
								this.Value = Reader.ReadBytes( ValueSize );
								break;
							}
						}
					}
					else
					{
						switch ( this.ValueType )
						{
							case EntryValueType.TYPE_INT32_STORED:
							case EntryValueType.TYPE_FLOAT_STORED:
							case EntryValueType.TYPE_STRING:
							case EntryValueType.TYPE_ENTRY_TABLE:
							case EntryValueType.TYPE_STRING_HASH:
								throw new FileFormatException();
							// only these types can store no value!
							case EntryValueType.TYPE_INT32_ZERO:
							case EntryValueType.TYPE_INT32_ONE:
							case EntryValueType.TYPE_FLOAT_ZERO:
							case EntryValueType.TYPE_FLOAT_ONE:
							case EntryValueType.TYPE_BYTE_ZERO:
							case EntryValueType.TYPE_BYTE_ONE:
							// ... but there are also a bunch of unknown, file-specific types
							// so just allow anything that isn't blacklisted
							default:
								break;
						}
					}
				}
			}

			public void SaveToBuffer( Stream BufferStream, Endianness Endian )
			{
				using ( BinaryWriter Writer = new BinaryWriter( BufferStream, Encoding.UTF8, true ) )
				{
					UInt16 ValueSize = Convert.ToUInt16( this.ValueSize );
					if ( this.Key == 2952159793 )
					{
						// HACK: for some reason, entries with this key store a different layout?!?!?
						// according to the disassembly of the game, it doesn't even parse this entry, it just skips it!
						// possibly some debug info of the file?
						Writer.Write( this.Key );
						Writer.Write( Convert.ToUInt32( ValueSize ), Endian );
					}
					else
					{
						SWB2EntryHeader EntryHeader = new SWB2EntryHeader();
						EntryHeader.Key = this.Key;
						EntryHeader.ValueType = this.ValueType;
						EntryHeader.Unknown = this.Flags;
						EntryHeader.ValueSize = ValueSize;
						Writer.WriteType( EntryHeader, Endian );
					}

					switch ( this.ValueType )
					{
						case EntryValueType.TYPE_INT32_STORED:
						{
							foreach ( Int32 Value in (this.Value as Int32[]) )
								Writer.Write( Value, Endian );
							break;
						}
						case EntryValueType.TYPE_FLOAT_STORED:
						{
							foreach ( Single Value in (this.Value as Single[]) )
								Writer.Write( Value, Endian );
							break;
						}
						case EntryValueType.TYPE_STRING:
						{
							string FinalString = string.Empty;
							string[] Strings = (this.Value as string[]);
							for ( Int32 i = 0; i < Strings.Length; i++ )
							{
								FinalString += Strings[i];
								if ( i != (Strings.Length - 1) )
									FinalString += StringValueSeparator;
							}

							byte[] Value = Encoding.UTF8.GetBytes( FinalString );
							Writer.Write( Value );
							Writer.Write( (Byte)0 ); // null terminator
							Int32 AlignmentCount = MathExtensions.AlignValue( ValueSize, StringAlignment ) - (Value.Length + 1);
							for ( Int32 i = 0; i < AlignmentCount; i++ )
								Writer.Write( (Byte)0 ); // alignment
							break;
						}
						case EntryValueType.TYPE_ENTRY_TABLE:
						{
							(this.Value as EntryTable).SaveToBuffer( BufferStream, Endian );
							break;
						}
						case EntryValueType.TYPE_STRING_HASH:
						{
							foreach ( CRC32 Value in (this.Value as CRC32[]) )
								Writer.Write( Value, Endian );
							break;
						}
						case EntryValueType.TYPE_INT32_ZERO:
						case EntryValueType.TYPE_INT32_ONE:
						case EntryValueType.TYPE_FLOAT_ZERO:
						case EntryValueType.TYPE_FLOAT_ONE:
						case EntryValueType.TYPE_BYTE_ZERO:
						case EntryValueType.TYPE_BYTE_ONE:
						{
							Debug.Assert( this.Value == null );
							break;
						}
						default:
						{
							if ( this.Value != null )
								Writer.Write( this.Value as Byte[] );
							break;
						}
					}
				}
			}

			public override string ToString()
			{
				return $"{Key.ToString()} ({ValueType.ToString()}): {ValueAsString}";
			}
		}

		public class EntryTable
		{
			public EntryTable( EntryTable? Parent ) { ParentTable = Parent; }
			
			[Browsable( false )]
			public EntryTable? ParentTable { get; private set; }

			public List<Entry> Entries = new List<Entry>();

			[LocalizedDisplayName( "SWB2DatabaseFile.EntryTable.Identifier.Name" ), LocalizedDescription( "SWB2DatabaseFile.EntryTable.Identifier.Description" )]
			public CRC32 Identifier { get; set; } = 0;

			[LocalizedDisplayName( "SWB2DatabaseFile.EntryTable.NumEntries.Name" )]
			public Int32 NumEntries { get { return Entries.Count; } }

			public void LoadFromBuffer( Stream BufferStream, Endianness Endian )
			{
				using ( BinaryReader Reader = new BinaryReader( BufferStream, Encoding.UTF8, true ) )
				{
					SWB2EntryTableHeader EntryTableHeader = Reader.ReadType<SWB2EntryTableHeader>( Endian );

					this.Identifier = EntryTableHeader.Identifier;
					this.Entries.Capacity = EntryTableHeader.NumEntries;
					for ( UInt32 j = 0; j < EntryTableHeader.NumEntries; j++ )
					{
						Entry Entry = new Entry( this );
						Entry.LoadFromBuffer( BufferStream, Endian );
						this.Entries.Add( Entry );
					}
				}
			}
			public void SaveToBuffer( Stream BufferStream, Endianness Endian )
			{
				using ( BinaryWriter Writer = new BinaryWriter( BufferStream, Encoding.UTF8, true ) )
				{
					SWB2EntryTableHeader EntryTableHeader = new SWB2EntryTableHeader();

					EntryTableHeader.Identifier = this.Identifier;
					EntryTableHeader.NumEntries = this.Entries.Count;
					Writer.WriteType<SWB2EntryTableHeader>( EntryTableHeader, Endian );

					foreach ( Entry Entry in Entries )
						Entry.SaveToBuffer( BufferStream, Endian );
				}
			}

			public override string ToString()
			{
				return $"{Identifier.ToString()} ({Entries.Count})";
			}
		}

		public List<EntryTable> EntryTables = new List<EntryTable>();
		private Endianness FileEndianness = Endianness.Little;

		public void Reset()
		{
			EntryTables.Clear();
		}

		public void LoadFromBuffer( Stream BufferStream )
		{
			Reset();

			try
			{
				using ( BinaryReader Reader = new BinaryReader( BufferStream ) )
				{
					// check if this is a valid file
					UInt32 Magic = Reader.ReadUInt32();
					if ( Magic == 1179011411 ) // "SIFF"
						FileEndianness = Endianness.Little;
					else if ( Magic == 1397311046 ) // "FFIS"
						FileEndianness = Endianness.Big;
					else
						throw new FileFormatException();

					Reader.BaseStream.Position = 0;

					SWB2Header Header = Reader.ReadType<SWB2Header>( FileEndianness );
					if ( Header.Magic1 != 1179011411 || // "SIFF" or "FFIS"
						Header.Magic2 != 1262701639 )   // "GLCK" or "KCLG"
						throw new FileFormatException();

					if ( (Reader.BaseStream.Length - Reader.BaseStream.Position) != Header.SizeOfTableData )
						throw new FileFormatException();

					// these are assumption I made when looking at game files... hopefully this never gets tripped!
					Debug.Assert( Header.Unknown1[0] == 0 );
					Debug.Assert( Header.Unknown1[1] == 0 );
					Debug.Assert( Header.Unknown1[2] == 1 );
					Debug.Assert( Header.Unknown1[3] == 0 );
					Debug.Assert( Header.Unknown2 == 16 );
					Debug.Assert( Header.Unknown3 == 0 );
					Debug.Assert( Header.Unknown4[0] == 0 );
					Debug.Assert( Header.Unknown4[1] == 0 );
					Debug.Assert( Header.Unknown4[2] == 0 );

					EntryTables.Capacity = Header.NumTables;
					for ( UInt32 i = 0; i < Header.NumTables; i++ )
					{
						EntryTable EntryTable = new EntryTable( null );
						EntryTable.LoadFromBuffer( BufferStream, FileEndianness );
						EntryTables.Add( EntryTable );
					}

					if ( Reader.BaseStream.Position != (Marshal.SizeOf<SWB2Header>() + Header.SizeOfTableData) )
						throw new FileFormatException();
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		public void SaveToBuffer( Stream BufferStream )
		{
			try
			{
				SWB2Header Header = new SWB2Header();
				Header.Magic1 = 1179011411; // "SIFF"
				Header.Unknown1 = new Byte[4];
				Header.Unknown1[0] = Header.Unknown1[1] = Header.Unknown1[3] = 0;
				Header.Unknown1[2] = 1;
				Header.Unknown2 = 16;
				Header.Unknown3 = 0;
				Header.Unknown4 = new UInt32[3];
				Header.Unknown4[0] = Header.Unknown4[1] = Header.Unknown4[2] = 0;

				Header.Magic2 = 1262701639; // "GLCK"
				Header.NumTables = EntryTables.Count;

				EntryTables.ForEach( EntryTable => {
					Header.SizeOfTableData += Marshal.SizeOf<SWB2EntryTableHeader>();
					EntryTable.Entries.ForEach( Entry => {
						Header.SizeOfTableData += Marshal.SizeOf<SWB2EntryHeader>();
						Header.SizeOfTableData += Entry.ValueSize;
					} );
				} );

				using ( BinaryWriter Writer = new BinaryWriter( BufferStream, Encoding.UTF8, true ) )
				{
					Writer.WriteType<SWB2Header>( Header, FileEndianness );
					foreach ( EntryTable EntryTable in EntryTables )
						EntryTable.SaveToBuffer( BufferStream, FileEndianness );
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToSaveFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}
	}
}
