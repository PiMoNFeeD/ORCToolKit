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
			TYPE_INT32_STORED = 1,	// (u?)int32 that is stored in the file
			TYPE_FLOAT_STORED = 2,	// float that is stored in the file
			TYPE_STRING = 3,		// string that is stored in the file
									// I imagine a TYPE_BYTE_STORED would be somewhere here, but there isn't a single asset with a byte type in the game to check...
			TYPE_ENTRY_TABLE = 6,	// a table that stores key-value entries
			TYPE_INT32_ZERO = 7,	// (u?)int32 that is always zero, not stored in the file (0)
			TYPE_INT32_ONE = 8,		// (u?)int32 that is always one, not stored in the file (1)
			TYPE_FLOAT_ZERO = 9,	// float that is always zero, not stored in the file (0.0)
			TYPE_FLOAT_ONE = 10,	// float that is always one, not stored in the file (1.0)
			TYPE_BYTE_ZERO = 11,	// byte that is always zero, not stored in the file (false)
			TYPE_BYTE_ONE = 12,		// byte that is always one, not stored in the file (true)
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

			public Entry( EntryTable? parent ) { ParentTable = parent; }

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

					switch ( _valueType )
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
					if ( Value is Array arrayValue )
						return arrayValue.Length;

					return 0;
				}
				set
				{
					Int32 newValueLength = value;
					if ( newValueLength < 1 )
						newValueLength = 1;

					if ( Value is Array arrayValue )
					{
						// resize the array by creating a new one
						Array newArrayValue = Array.CreateInstance( arrayValue.GetType().GetElementType(), newValueLength );
						Array.Copy( arrayValue, newArrayValue, Math.Min( ValueLength, newValueLength ) );
						Value = newArrayValue;
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
					string result = string.Empty;

					const Int32 MaxElementsToDisplay = 10;
					switch ( ValueType )
					{
						case EntryValueType.TYPE_INT32_STORED:
						case EntryValueType.TYPE_FLOAT_STORED:
						case EntryValueType.TYPE_STRING:
						case EntryValueType.TYPE_STRING_HASH:
						{
							Debug.Assert( Value != null );

							Array values = Value as Array;
							if ( values.Length == 1 )
							{
								result += values.GetValue( 0 );
							}
							else
							{
								result += "[";
								Int32 length = Math.Min( MaxElementsToDisplay, values.Length );
								for ( Int32 i = 0; i < length; i++ )
								{
									result += values.GetValue( i );
									if ( i == (length - 1) )
									{
										if ( length < values.Length )
											result += ":...";
									}
									else
									{
										result += StringValueSeparator;
									}
								}
								result += "]";
							}
							break;
						}
						case EntryValueType.TYPE_ENTRY_TABLE:
						{
							result += Value.ToString();
							break;
						}
						// this weird logic allows me to automatically propogate default values without having to worry about manually typing them out
						case EntryValueType.TYPE_INT32_ZERO:
						case EntryValueType.TYPE_INT32_ONE:
						{
							Debug.Assert( Value == null );
							result += Convert.ToInt32( ValueType - EntryValueType.TYPE_INT32_ZERO );
							break;
						}
						case EntryValueType.TYPE_FLOAT_ZERO:
						case EntryValueType.TYPE_FLOAT_ONE:
						{
							Debug.Assert( Value == null );
							result += Convert.ToSingle( ValueType - EntryValueType.TYPE_FLOAT_ZERO );
							break;
						}
						case EntryValueType.TYPE_BYTE_ZERO:
						case EntryValueType.TYPE_BYTE_ONE:
						{
							Debug.Assert( Value == null );
							result += Convert.ToBoolean( ValueType - EntryValueType.TYPE_BYTE_ZERO );
							break;
						}
						default:
						{
							if ( Value != null )
								goto case EntryValueType.TYPE_STRING_HASH; // don't copy paste code
							break;
						}
					}

					return result;
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
							string finalString = string.Empty;
							string[] strings = (Value as string[]);
							for ( Int32 i = 0; i < strings.Length; i++ )
							{
								finalString += strings[i];
								if ( i != (strings.Length - 1) )
									finalString += StringValueSeparator;
							}
							return MathExtensions.AlignValue( Encoding.UTF8.GetByteCount( finalString ) + 1, StringAlignment ); // +1 for null terminator
						}
						case EntryValueType.TYPE_ENTRY_TABLE:
						{
							Debug.Assert( Value != null );
							Int32 size = Marshal.SizeOf<SWB2EntryTableHeader>();
							foreach ( Entry entry in (Value as EntryTable).Entries )
							{
								size += Marshal.SizeOf<SWB2EntryHeader>();
								size += entry.ValueSize;
							}
							return size;
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

			public void LoadFromBuffer( Stream bufferStream, Endianness endian )
			{
				using ( BinaryReader reader = new BinaryReader( bufferStream, Encoding.UTF8, true ) )
				{
					Int32 valueSize = 0;
					{
						UInt32 key = reader.ReadUInt32( endian );
						if ( key == 2952159793 )
						{
							// HACK: for some reason, entries with this key store a different layout?!?!?
							// according to the disassembly of the game, it doesn't even parse this entry, it just skips it!
							// possibly some debug info of the file?
							Key = key;
							ValueType = EntryValueType.TYPE_INT32_STORED; // all values appear to be a mix of ints and floats, always byteswaped in batches of 4 bytes... slamming to int32 just so byteswapping is correct
							Flags = 0;
							valueSize = reader.ReadInt32( endian );
						}
						else
						{
							reader.BaseStream.Position -= Marshal.SizeOf<UInt32>();

							SWB2EntryHeader entryHeader = reader.ReadType<SWB2EntryHeader>( endian );
							Key = entryHeader.Key;
							ValueType = entryHeader.ValueType;
							Flags = entryHeader.Unknown;
							valueSize = entryHeader.ValueSize;
						}
					}

					if ( valueSize > 0 )
					{
						switch ( ValueType )
						{
							case EntryValueType.TYPE_INT32_STORED:
							{
								if ( (valueSize % Marshal.SizeOf<Int32>()) != 0 )
									throw new FileFormatException();

								Value = reader.ReadInt32Array( valueSize / Marshal.SizeOf<Int32>(), endian );
								break;
							}
							case EntryValueType.TYPE_FLOAT_STORED:
							{
								if ( (valueSize % Marshal.SizeOf<Single>()) != 0 )
									throw new FileFormatException();

								Value = reader.ReadSingleArray( valueSize / Marshal.SizeOf<Single>(), endian );
								break;
							}
							case EntryValueType.TYPE_STRING:
							{
								string value = reader.ReadFixedLengthString( Encoding.UTF8, valueSize );
								if ( value.Contains( ';' ) )
									StringValueSeparator = ';';
								Value = value.Split( StringValueSeparator );
								break;
							}
							case EntryValueType.TYPE_ENTRY_TABLE:
							{
								Int64 oldPosition = reader.BaseStream.Position;
								EntryTable value = new EntryTable( ParentTable );
								value.LoadFromBuffer( bufferStream, endian );
								Value = value;

								if ( (reader.BaseStream.Position - oldPosition) != valueSize )
									throw new FileFormatException();
								break;
							}
							case EntryValueType.TYPE_STRING_HASH:
							{
								if ( (valueSize % Marshal.SizeOf<CRC32>()) != 0 )
									throw new FileFormatException();

								Value = reader.ReadTypeArray<CRC32>( valueSize / Marshal.SizeOf<CRC32>(), endian );
								break;
							}
							default:
							{
								Value = reader.ReadBytes( valueSize );
								break;
							}
						}
					}
					else
					{
						switch ( ValueType )
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

			public void SaveToBuffer( Stream bufferStream, Endianness endian )
			{
				using ( BinaryWriter writer = new BinaryWriter( bufferStream, Encoding.UTF8, true ) )
				{
					UInt16 valueSize = Convert.ToUInt16( ValueSize );
					if ( Key == 2952159793 )
					{
						// HACK: for some reason, entries with this key store a different layout?!?!?
						// according to the disassembly of the game, it doesn't even parse this entry, it just skips it!
						// possibly some debug info of the file?
						writer.Write( Key );
						writer.Write( Convert.ToUInt32( valueSize ), endian );
					}
					else
					{
						SWB2EntryHeader entryHeader = new SWB2EntryHeader();
						entryHeader.Key = Key;
						entryHeader.ValueType = ValueType;
						entryHeader.Unknown = Flags;
						entryHeader.ValueSize = valueSize;
						writer.WriteType( entryHeader, endian );
					}

					switch ( ValueType )
					{
						case EntryValueType.TYPE_INT32_STORED:
						{
							foreach ( Int32 value in (Value as Int32[]) )
								writer.Write( value, endian );
							break;
						}
						case EntryValueType.TYPE_FLOAT_STORED:
						{
							foreach ( Single value in (Value as Single[]) )
								writer.Write( value, endian );
							break;
						}
						case EntryValueType.TYPE_STRING:
						{
							string finalString = string.Empty;
							string[] strings = (Value as string[]);
							for ( Int32 i = 0; i < strings.Length; i++ )
							{
								finalString += strings[i];
								if ( i != (strings.Length - 1) )
									finalString += StringValueSeparator;
							}

							byte[] value = Encoding.UTF8.GetBytes( finalString );
							writer.Write( value );
							writer.Write( (Byte)0 ); // null terminator
							Int32 alignmentCount = MathExtensions.AlignValue( valueSize, StringAlignment ) - (value.Length + 1);
							for ( Int32 i = 0; i < alignmentCount; i++ )
								writer.Write( (Byte)0 ); // alignment
							break;
						}
						case EntryValueType.TYPE_ENTRY_TABLE:
						{
							(Value as EntryTable).SaveToBuffer( bufferStream, endian );
							break;
						}
						case EntryValueType.TYPE_STRING_HASH:
						{
							foreach ( CRC32 value in (Value as CRC32[]) )
								writer.Write( value, endian );
							break;
						}
						case EntryValueType.TYPE_INT32_ZERO:
						case EntryValueType.TYPE_INT32_ONE:
						case EntryValueType.TYPE_FLOAT_ZERO:
						case EntryValueType.TYPE_FLOAT_ONE:
						case EntryValueType.TYPE_BYTE_ZERO:
						case EntryValueType.TYPE_BYTE_ONE:
						{
							Debug.Assert( Value == null );
							break;
						}
						default:
						{
							if ( Value != null )
								writer.Write( Value as Byte[] );
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
			public EntryTable( EntryTable? parent ) { ParentTable = parent; }

			[Browsable( false )]
			public EntryTable? ParentTable { get; private set; }

			public List<Entry> Entries = new List<Entry>();

			[LocalizedDisplayName( "SWB2DatabaseFile.EntryTable.Identifier.Name" ), LocalizedDescription( "SWB2DatabaseFile.EntryTable.Identifier.Description" )]
			public CRC32 Identifier { get; set; } = 0;

			[LocalizedDisplayName( "SWB2DatabaseFile.EntryTable.NumEntries.Name" )]
			public Int32 NumEntries { get { return Entries.Count; } }

			public void LoadFromBuffer( Stream bufferStream, Endianness endian )
			{
				using ( BinaryReader reader = new BinaryReader( bufferStream, Encoding.UTF8, true ) )
				{
					SWB2EntryTableHeader entryTableHeader = reader.ReadType<SWB2EntryTableHeader>( endian );

					Identifier = entryTableHeader.Identifier;
					Entries.Capacity = entryTableHeader.NumEntries;
					for ( UInt32 j = 0; j < entryTableHeader.NumEntries; j++ )
					{
						Entry entry = new Entry( this );
						entry.LoadFromBuffer( bufferStream, endian );
						Entries.Add( entry );
					}
				}
			}
			public void SaveToBuffer( Stream bufferStream, Endianness endian )
			{
				using ( BinaryWriter writer = new BinaryWriter( bufferStream, Encoding.UTF8, true ) )
				{
					SWB2EntryTableHeader entryTableHeader = new SWB2EntryTableHeader();

					entryTableHeader.Identifier = Identifier;
					entryTableHeader.NumEntries = Entries.Count;
					writer.WriteType<SWB2EntryTableHeader>( entryTableHeader, endian );

					foreach ( Entry entry in Entries )
						entry.SaveToBuffer( bufferStream, endian );
				}
			}

			public override string ToString()
			{
				return $"{Identifier.ToString()} ({Entries.Count})";
			}
		}

		public List<EntryTable> EntryTables = new List<EntryTable>();
		private Endianness _fileEndianness = Endianness.Little;

		public void Reset()
		{
			EntryTables.Clear();
		}

		public void LoadFromBuffer( Stream bufferStream )
		{
			Reset();

			try
			{
				using ( BinaryReader reader = new BinaryReader( bufferStream ) )
				{
					// check if this is a valid file
					UInt32 magic = reader.ReadUInt32();
					if ( magic == 1179011411 ) // "SIFF"
						_fileEndianness = Endianness.Little;
					else if ( magic == 1397311046 ) // "FFIS"
						_fileEndianness = Endianness.Big;
					else
						throw new FileFormatException();

					reader.BaseStream.Position = 0;

					SWB2Header header = reader.ReadType<SWB2Header>( _fileEndianness );
					if ( header.Magic1 != 1179011411 || // "SIFF" or "FFIS"
						header.Magic2 != 1262701639 )   // "GLCK" or "KCLG"
						throw new FileFormatException();

					if ( (reader.BaseStream.Length - reader.BaseStream.Position) != header.SizeOfTableData )
						throw new FileFormatException();

					// these are assumption I made when looking at game files...
					// and after going through *every* SWB2 file in the game, these never assert!
					Debug.Assert( header.Unknown1[0] == 0 );
					Debug.Assert( header.Unknown1[1] == 0 );
					Debug.Assert( header.Unknown1[2] == 1 );
					Debug.Assert( header.Unknown1[3] == 0 );
					Debug.Assert( header.Unknown2 == 16 );
					Debug.Assert( header.Unknown3 == 0 );
					Debug.Assert( header.Unknown4[0] == 0 );
					Debug.Assert( header.Unknown4[1] == 0 );
					Debug.Assert( header.Unknown4[2] == 0 );

					EntryTables.Capacity = header.NumTables;
					for ( UInt32 i = 0; i < header.NumTables; i++ )
					{
						EntryTable entryTable = new EntryTable( null );
						entryTable.LoadFromBuffer( bufferStream, _fileEndianness );
						EntryTables.Add( entryTable );
					}

					if ( reader.BaseStream.Position != (Marshal.SizeOf<SWB2Header>() + header.SizeOfTableData) )
						throw new FileFormatException();
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToOpenFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		public void SaveToBuffer( Stream bufferStream )
		{
			try
			{
				SWB2Header header = new SWB2Header();
				header.Magic1 = 1179011411; // "SIFF"
				header.Unknown1 = new Byte[4];
				header.Unknown1[0] = header.Unknown1[1] = header.Unknown1[3] = 0;
				header.Unknown1[2] = 1;
				header.Unknown2 = 16;
				header.Unknown3 = 0;
				header.Unknown4 = new UInt32[3];
				header.Unknown4[0] = header.Unknown4[1] = header.Unknown4[2] = 0;

				header.Magic2 = 1262701639; // "GLCK"
				header.NumTables = EntryTables.Count;

				EntryTables.ForEach( entryTable =>
				{
					header.SizeOfTableData += Marshal.SizeOf<SWB2EntryTableHeader>();
					entryTable.Entries.ForEach( entry =>
					{
						header.SizeOfTableData += Marshal.SizeOf<SWB2EntryHeader>();
						header.SizeOfTableData += entry.ValueSize;
					} );
				} );

				using ( BinaryWriter writer = new BinaryWriter( bufferStream, Encoding.UTF8, true ) )
				{
					writer.WriteType<SWB2Header>( header, _fileEndianness );
					foreach ( EntryTable entryTable in EntryTables )
						entryTable.SaveToBuffer( bufferStream, _fileEndianness );
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, Resources.ORCToolKit.UnableToSaveFileTitle, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}
	}
}
