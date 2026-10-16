using System.Numerics;

namespace ORCToolKit.Extensions
{
	static public class MathExtensions
	{
		static public bool IsPowerOfTwo<T>( T num ) where T : INumber<T>, IBitwiseOperators<T, T, T>
		{
			return !T.IsZero( num ) && (num & num - T.One) == T.Zero;
		}

		static public T AlignValue<T>( T value, T alignTo ) where T : INumber<T>
		{
			if ( alignTo < T.One )
				return value;
			return value + (alignTo - value % alignTo) % alignTo;
		}
		static public T RoundUpToPowerOf2<T>( T value ) where T : INumber<T>
		{
			// more portable than bit shifting... kinda sucks though!
			T number = T.One;
			while ( number < value )
				number += number;
			return number;
		}
	}
}
