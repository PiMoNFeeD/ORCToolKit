using System.Runtime.InteropServices;

namespace ORCToolKit
{
	static public class BinaryWriterExtensions
	{
		static public void WriteType<T>( this BinaryWriter Writer, T Type )
		{
			Int32 SizeOfType = Marshal.SizeOf( Type );
			byte[] TypeBytes = new byte[SizeOfType];

			IntPtr Data = Marshal.AllocHGlobal( SizeOfType );
			Marshal.StructureToPtr( Type, Data, true );
			Marshal.Copy( Data, TypeBytes, 0, SizeOfType );
			Marshal.FreeHGlobal( Data );

			Writer.Write( TypeBytes );
		}
	}
}
