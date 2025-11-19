namespace Utilities.Maths
{
	using System;
	using UnityEngine;

	public static class MathCore
	{
		public static float Epsilon => float.Epsilon;
		public static float Abs( float v ) => Mathf.Abs( v );
		public static int Abs( int v ) => Mathf.Abs( v );
		public static float Sign( float v ) => Abs( v ) < Epsilon ? 0 : v > 0 ? 1f : -1f;
		public static int Sign( int v ) => v == 0 ? 0 : v > 0 ? 1 : -1;

		public static float UnclampedInverseLerp( float a, float b, float value )
		{
			return Math.Abs( a - b ) > float.Epsilon 
				? (value - a) / (b - a) 
				: 0.0f;
		}

		public static float UnclampedInverseLerp( DateTime a, DateTime b, DateTime value )
		{
			var aTicks = a.Ticks;
			var bTicks = b.Ticks;
			var valueTicks = value.Ticks;
			
			return aTicks != bTicks
				? (float)(valueTicks - aTicks) / (bTicks - aTicks)
				: 0.0f;
		}

		public static T Clamp<T>( T value, T minValue, T maxValue ) where T : struct, IComparable<T>
		{
			if ( minValue.CompareTo( maxValue ) > 0 )
			{
				throw new ArgumentException( "minValue must be less than or equal to maxValue" );
			}

			if ( value.CompareTo( minValue ) < 0 )
			{
				return minValue;
			}

			if ( value.CompareTo( maxValue ) > 0 )
			{
				return maxValue;
			}

			return value;
		}
	}
}