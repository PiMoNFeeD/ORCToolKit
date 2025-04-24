using System.Numerics;

namespace ORCToolKit
{
	static public class MathExtensions
	{
		static public bool IsPowerOfTwo<T>( T num ) where T : INumber<T>, IBitwiseOperators<T, T, T>
		{
			return !T.IsZero( num ) && (num & (num - T.One)) == T.Zero;
		}
	}
}
