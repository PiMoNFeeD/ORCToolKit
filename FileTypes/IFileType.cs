namespace ORCToolKit.FileTypes
{
	public interface IFileType
	{
		public void Reset();
		public void LoadFromBuffer( Stream FileStream );
		public void SaveToBuffer( Stream FileStream );
	}
}
