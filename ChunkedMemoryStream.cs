using System.Diagnostics;

namespace ORCToolKit
{
	// why the fuck does Stream mix between 32 and 64 bit integers for counting data???
	public class ChunkedMemoryStream: Stream
	{
		private readonly List<byte[]> _chunks = new List<byte[]>();
		private readonly Int32 _chunkSize;
		private Int32 _position = 0;
		private Int32 _length = 0;

		public ChunkedMemoryStream( Int32 ChunkSize, Int32 Capacity = 0 )
		{
			_chunkSize = ChunkSize;
			EnsureChunksAllocated( Capacity );
		}

		public override bool CanRead { get { return true; } }
		public override bool CanWrite { get { return true; } }
		public override bool CanSeek { get { return true; } }

		public override Int64 Length { get { return _length; } }
		public override Int64 Position
		{
			get { return _position; }
			set { _position = Convert.ToInt32( value ); }
		}

		public override Int64 Seek( Int64 Offset, SeekOrigin Location )
		{
			switch ( Location )
			{
				case SeekOrigin.Begin:
				{
					if ( Offset < 0 )
						throw new IOException();
					_position = Convert.ToInt32( Offset );
					break;
				}
				case SeekOrigin.Current:
				{
					if ( _position + Offset < 0 )
						throw new IOException();
					_position += Convert.ToInt32( Offset );
					break;
				}
				case SeekOrigin.End:
				{
					if ( _length + Offset < 0 )
						throw new IOException();
					_position = Convert.ToInt32( _length + Offset );
					break;
				}
				default:
					throw new ArgumentException();
			}

			Debug.Assert( _position >= 0, "_position >= 0" );
			return _position;
		}

		public override Int32 Read( byte[] ByteBuffer, Int32 Offset, Int32 Count )
		{
			EnsureChunksAllocated( _length );

			Int32 BytesToRead = Convert.ToInt32( _length - _position );
			if ( BytesToRead > Count )
				BytesToRead = Count;
			if ( BytesToRead <= 0 )
				return 0;

			Debug.Assert( _position + BytesToRead >= 0, "_position + BytesToRead >= 0" );

			Int32 DataReadSoFar = 0;
			while ( DataReadSoFar < BytesToRead )
			{
				Int32 ChunkNum = _position / _chunkSize;
				Int32 ChunkPosition = _position % _chunkSize;
				Int32 ChunkCapacity = Int32.Min( _length - (ChunkNum * _chunkSize), _chunkSize );
				Int32 ReadableDataThisChunk = Int32.Min( Count - DataReadSoFar, ChunkCapacity - ChunkPosition );

				Buffer.BlockCopy( _chunks[ChunkNum], ChunkPosition, ByteBuffer, DataReadSoFar, ReadableDataThisChunk );

				DataReadSoFar += ReadableDataThisChunk;
				_position += ReadableDataThisChunk;
			}

			Debug.Assert( DataReadSoFar <= Count, "DataReadSoFar <= Count" );

			return DataReadSoFar;
		}

		public override void Write( byte[] ByteBuffer, Int32 Offset, Int32 Count )
		{
			if ( _length < _position )
				_length = _position;

			_length += Count;
			EnsureChunksAllocated( _length );

			Int32 DataWroteSoFar = 0;
			while ( DataWroteSoFar < Count )
			{
				Int32 ChunkNum = _position / _chunkSize;
				Int32 ChunkPosition = _position % _chunkSize;
				Int32 WritableDataThisChunk = Int32.Min( Count - DataWroteSoFar, _chunkSize - ChunkPosition );

				Buffer.BlockCopy( ByteBuffer, DataWroteSoFar, _chunks[ChunkNum], ChunkPosition, WritableDataThisChunk );

				DataWroteSoFar += WritableDataThisChunk;
				_position += WritableDataThisChunk;
			}

			Debug.Assert( DataWroteSoFar <= Count, "DataWroteSoFar <= Count" );
		}

		public override void Flush() { }

		public override void SetLength( Int64 NewLength )
		{
			if ( NewLength < 0 )
				throw new ArgumentOutOfRangeException( nameof( NewLength ) );

			_length = Convert.ToInt32( NewLength );
			EnsureChunksAllocated( _length );
		}

		public Int32 GetChunksCount() => _chunks.Count;

		private void EnsureChunksAllocated( Int32 Size )
		{
			// NOTE: this does not remove redundant chunks!
			while ( Size > _chunks.Count * _chunkSize )
				_chunks.Add( new byte[_chunkSize] );
		}
	}
}
