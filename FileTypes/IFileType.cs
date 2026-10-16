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
		public void LoadFromBuffer( Stream bufferStream );
		public void SaveToBuffer( Stream bufferStream );
	}
}
