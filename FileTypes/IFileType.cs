namespace ORCToolKit.FileTypes
{
	public enum Endianness
	{
		Little,
		Big,
	}

	public interface IFileType
	{
		public void Reset();
		public void LoadFromBuffer( Stream BufferStream );
		public void SaveToBuffer( Stream BufferStream );
	}
}
