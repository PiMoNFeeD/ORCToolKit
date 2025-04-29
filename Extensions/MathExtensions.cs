using System.Numerics;

namespace ORCToolKit.Extensions
{
	static public class MathExtensions
	{
		static public bool IsPowerOfTwo<T>( T num ) where T : INumber<T>, IBitwiseOperators<T, T, T>
		{
			return !T.IsZero( num ) && (num & num - T.One) == T.Zero;
		}

		static public T AlignValue<T>( T Value, T AlignTo ) where T : INumber<T>
		{
			return Value + (AlignTo - Value % AlignTo) % AlignTo;
		}
		static public T RoundUpToPowerOf2<T>( T Value ) where T : INumber<T>
		{
			// more portable than bit shifting... kinda sucks though!
			T Number = T.One;
			while ( Number < Value )
				Number += Number;
			return Number;
		}
	}
}
