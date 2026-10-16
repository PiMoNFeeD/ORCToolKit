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

		public ChunkedMemoryStream( Int32 chunkSize, Int32 capacity = 0 )
		{
			_chunkSize = chunkSize;
			EnsureChunksAllocated( capacity );
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

		public override Int64 Seek( Int64 offset, SeekOrigin location )
		{
			switch ( location )
			{
				case SeekOrigin.Begin:
				{
					if ( offset < 0 )
						throw new IOException();
					_position = Convert.ToInt32( offset );
					break;
				}
				case SeekOrigin.Current:
				{
					if ( _position + offset < 0 )
						throw new IOException();
					_position += Convert.ToInt32( offset );
					break;
				}
				case SeekOrigin.End:
				{
					if ( _length + offset < 0 )
						throw new IOException();
					_position = Convert.ToInt32( _length + offset );
					break;
				}
				default:
					throw new ArgumentException();
			}

			Debug.Assert( _position >= 0, "_position >= 0" );
			return _position;
		}

		public override Int32 Read( byte[] byteBuffer, Int32 offset, Int32 count )
		{
			EnsureChunksAllocated( _length );

			Int32 bytesToRead = Convert.ToInt32( _length - _position );
			if ( bytesToRead > count )
				bytesToRead = count;
			if ( bytesToRead <= 0 )
				return 0;

			Debug.Assert( _position + bytesToRead >= 0, "_position + bytesToRead >= 0" );

			Int32 dataReadSoFar = 0;
			while ( dataReadSoFar < bytesToRead )
			{
				Int32 chunkNum = _position / _chunkSize;
				Int32 chunkPosition = _position % _chunkSize;
				Int32 chunkCapacity = Int32.Min( _length - (chunkNum * _chunkSize), _chunkSize );
				Int32 readableDataThisChunk = Int32.Min( count - dataReadSoFar, chunkCapacity - chunkPosition );

				Buffer.BlockCopy( _chunks[chunkNum], chunkPosition, byteBuffer, dataReadSoFar, readableDataThisChunk );

				dataReadSoFar += readableDataThisChunk;
				_position += readableDataThisChunk;
			}

			Debug.Assert( dataReadSoFar <= count, "dataReadSoFar <= count" );

			return dataReadSoFar;
		}

		public override void Write( byte[] byteBuffer, Int32 offset, Int32 count )
		{
			if ( _length < _position )
				_length = _position;

			_length += count;
			EnsureChunksAllocated( _length );

			Int32 dataWrittenSoFar = 0;
			while ( dataWrittenSoFar < count )
			{
				Int32 chunkNum = _position / _chunkSize;
				Int32 chunkPosition = _position % _chunkSize;
				Int32 writableDataThisChunk = Int32.Min( count - dataWrittenSoFar, _chunkSize - chunkPosition );

				Buffer.BlockCopy( byteBuffer, dataWrittenSoFar, _chunks[chunkNum], chunkPosition, writableDataThisChunk );

				dataWrittenSoFar += writableDataThisChunk;
				_position += writableDataThisChunk;
			}

			Debug.Assert( dataWrittenSoFar <= count, "dataWrittenSoFar <= count" );
		}

		public override void Flush() { }

		public override void SetLength( Int64 newLength )
		{
			if ( newLength < 0 )
				throw new ArgumentOutOfRangeException( nameof( newLength ) );

			_length = Convert.ToInt32( newLength );
			EnsureChunksAllocated( _length );
		}

		public Int32 GetChunksCount() => _chunks.Count;

		private void EnsureChunksAllocated( Int32 size )
		{
			// NOTE: this does not remove redundant chunks!
			while ( size > _chunks.Count * _chunkSize )
				_chunks.Add( new byte[_chunkSize] );
		}
	}
}
