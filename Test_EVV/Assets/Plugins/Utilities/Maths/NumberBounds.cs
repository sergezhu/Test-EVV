namespace Utilities.Maths
{
	using System;
	using System.Globalization;
	using Sirenix.OdinInspector;
	using UnityEngine;

	public interface INumberBounds<T> where T : struct
	{
		public T Min { get; }
		public T Max { get; }
		public T Average { get; }

		public bool InBounds( T value, bool includeMin = true, bool includeMax = true );
		public void Swap();
	}
	
	[Serializable]
	public struct IntBounds : INumberBounds<int>
	{
		[HorizontalGroup( "G1" ), LabelWidth( 30 )]
		[SerializeField] private int _min;
		[HorizontalGroup( "G1" ), LabelWidth( 30 )]
		[SerializeField] private int _max;

		public IntBounds( int min, int max )
		{
			_min = max > min ? min : max;
			_max = max > min ? max : min;
		}
		public int Min => _min;
		public int Max => _max;
		public int Average => Mathf.RoundToInt( 0.5f * (_min + _max) );
		
		public bool InBounds( int value, bool includeMin = true, bool includeMax = true )
		{
			bool isInside = value > Min && value < Max;
			isInside |= includeMin && value == Min;
			isInside |= includeMax && value == Max;
			
			return isInside;
		}

		public void Swap()
		{
			var min = Min;
			var max = Max;
			_min = max;
			_max = min;
		}

		public int GetRandom(bool includeMax)
		{
			var d = includeMax ? 1 : 0;
			return UnityEngine.Random.Range( _min, _max + d );
		}
	}

	[Serializable]
	public struct LongBounds : INumberBounds<long>
	{
		[SerializeField] private long _min;
		[SerializeField] private long _max;

		public LongBounds( long min, long max )
		{
			_min = max > min ? min : max;
			_max = max > min ? max : min;
		}

		public long Min => _min;
		public long Max => _max;
		public long Size => Max - Min;
		public long Average => Lerp( 0.5f );
		public long Lerp( float interpolationFactor ) => _min + (long)(interpolationFactor * Size);


		public bool InBounds( long value, bool includeMin = true, bool includeMax = true )
		{
			bool isInside = value > Min && value < Max;
			isInside |= includeMin && value == Min;
			isInside |= includeMax && value == Max;

			return isInside;
		}

		public void Swap()
		{
			var min = Min;
			var max = Max;
			_min = max;
			_max = min;
		}

		public long GetRandom()
		{
			return Lerp( UnityEngine.Random.Range( 0f, 1f ) );
		}

		public override string ToString()
		{
			return $"[ {_min} , {_max} ]";
		}
	}
	
	[Serializable]
	public struct FloatBounds : INumberBounds<float>
	{
		[SerializeField] private float _min;
		[SerializeField] private float _max;

		public FloatBounds( float min, float max )
		{
			_min = max > min ? min : max;
			_max = max > min ? max : min;
		}
		public float Min => _min;
		public float Max => _max;
		public float Size => Max - Min;
		public float Average => Lerp( 0.5f );
		public bool IsValid => Size > float.Epsilon;

		public float Lerp( float interpolationFactor ) => _min + interpolationFactor * Size;
		public float InverseLerp( float value ) => MathCore.UnclampedInverseLerp( _min, _max, value );

		public bool InBounds( float value, bool includeMin = true, bool includeMax = true )
		{
			var tolerance = float.Epsilon;
			bool isInside = value > Min && value < Max;
			isInside |= includeMin && Math.Abs( value - Min ) < tolerance;
			isInside |= includeMax && Math.Abs( value - Max ) < tolerance;

			return isInside;
		}

		public void Swap()
		{
			var min = Min;
			var max = Max;
			_min = max;
			_max = min;
		}

		public bool HasOverflow( FloatBounds constraintBounds, out float overflowBottom, out float overflowTop )
		{
			return HasOverflow( Min, Max, constraintBounds, out overflowBottom, out overflowTop );
		}

		
		
		public void ScaleWithConstraint( float scaleFactor, FloatBounds totalBoundsY, float relativeAnchor, bool overflowCompensate, float relativeMinSize = 0.1f, float relativeMaxSize = 10f )
		{
			var hasOverflowBefore = HasOverflow( _min, _max, totalBoundsY, out var overflowBottomBefore, out var overflowTopBefore );
			
			relativeAnchor = Mathf.Clamp( relativeAnchor, 0f, 1f );
			relativeMinSize = Mathf.Max( relativeMinSize, 0f );
			relativeMaxSize = Mathf.Max( relativeMaxSize, 0f );

			if ( relativeMaxSize < relativeMinSize ) 
				(relativeMinSize, relativeMaxSize) = (relativeMaxSize, relativeMinSize);

			var minSize = totalBoundsY.Size * relativeMinSize;
			var maxSize = totalBoundsY.Size * relativeMaxSize;
			var newSizeRaw = Size * scaleFactor;
			var desiredSize = Mathf.Clamp( newSizeRaw, minSize, maxSize );
			
			var trueScaleFactor = desiredSize / Size;
			var selfScalePivot = Lerp( relativeAnchor );
			var totalBoundsPivot = totalBoundsY.Lerp( relativeAnchor );
			var newMin = totalBoundsPivot + (Min - totalBoundsPivot) * trueScaleFactor;
			var newMax = totalBoundsPivot + (Max - totalBoundsPivot) * trueScaleFactor;
			var newSize = newMax - newMin;
			var hasOverflowAfter = HasOverflow( newMin, newMax, totalBoundsY, out var overflowBottomAfter, out var overflowTopAfter );

			if ( newSize > maxSize )
			{
				Debug.LogWarning( $"Size {Size}, Desired {desiredSize}, Result size {newSize} - must be lesser than {maxSize}" );
				return;
			}

			if ( newSize < minSize )
			{
				Debug.LogWarning( $"Size {Size}, Desired {desiredSize}, Result size {newSize} must be greater than {minSize}" );
				return;
			}

			if ( overflowCompensate && hasOverflowAfter )
			{
				var compensateOffset = overflowBottomAfter - overflowTopAfter;

				if ( overflowBottomAfter > 0 && overflowTopAfter > 0 )
					compensateOffset = 0;
				
				newMin -= compensateOffset;
				newMax -= compensateOffset;
			}
			
			
			
			/*if ( hasOverflowAfter && !hasOverflowBefore && overflowCompensateWhenGrow )
			{
				var compensateOffset = overflowBottomAfter - overflowTopAfter;
				newMin -= compensateOffset;
				newMax -= compensateOffset;
				
				/*if ( overflowCompensateWhenShrink )
				{
					var compensateOffset = overflowBottomAfter - overflowTopAfter;
					newMin -= compensateOffset;
					newMax -= compensateOffset;
				}
				else
				{
					var clampedNewMin = Mathf.Clamp( Min, totalBoundsY.Min, totalBoundsY.Max );
					var clampedNewMax = Mathf.Clamp( Max, totalBoundsY.Min, totalBoundsY.Max );
					newMin = clampedNewMin;
					newMax = clampedNewMax;
				}#1#
			}*/

			/*if ( !hasOverflowAfter && hasOverflowBefore && overflowCompensateWhenShrink )
			{
				var compensateOffset = overflowBottomAfter - overflowTopAfter;
				newMin -= compensateOffset;
				newMax -= compensateOffset;

				/*if ( overflowCompensateWhenShrink )
				{
					var compensateOffset = overflowBottomAfter - overflowTopAfter;
					newMin -= compensateOffset;
					newMax -= compensateOffset;
				}
				else
				{
					var clampedNewMin = Mathf.Clamp( Min, totalBoundsY.Min, totalBoundsY.Max );
					var clampedNewMax = Mathf.Clamp( Max, totalBoundsY.Min, totalBoundsY.Max );
					newMin = clampedNewMin;
					newMax = clampedNewMax;
				}#1#
			}*/

			_min = newMin;
			_max = newMax;
		}

		public void TryAlignWithConstraint( FloatBounds totalBounds, float relativeAnchor )
		{
			var hasOverflow = HasOverflow( _min, _max, totalBounds, out var overflowBottom, out var overflowTop );

			if(hasOverflow == false)
				return;

			var deltaSize = Size - totalBounds.Size; 
			
			var bottomOffset = deltaSize > 0
				? Mathf.Lerp( 0, deltaSize, 1- relativeAnchor )  
				: Mathf.Lerp( deltaSize, 0, 1 - relativeAnchor ); 
			
			var topOffset = deltaSize > 0
				? Mathf.Lerp( 0, deltaSize, relativeAnchor ) 
				: Mathf.Lerp( deltaSize, 0, relativeAnchor );

			var newMin = totalBounds.Min - bottomOffset; 
			var newMax = totalBounds.Max + topOffset;    

			_min = newMin;
			_max = newMax;
		}
		
		public float GetRandom()
		{
			return UnityEngine.Random.Range( _min, _max );
		}

		public override string ToString()
		{
			return $"[ {_min:F3} , {_max:F3} ]";
		}

		private bool HasOverflow( float min, float max, FloatBounds totalBounds, out float overflowBottom, out float overflowTop )
		{
			overflowTop = Mathf.Max( 0, totalBounds.Max - max );
			overflowBottom = Mathf.Max( 0, min - totalBounds.Min );
			var hasOverflow = overflowTop > 0 || overflowBottom > 0;

			return hasOverflow;
		}
	}

	[Serializable]
	public struct DatetimeBounds : INumberBounds<DateTime>
	{
		private DateTime _min;
		private DateTime _max;

		public DatetimeBounds( DateTime min, DateTime max )
		{
			_min = max > min ? min : max;
			_max = max > min ? max : min;
		}

		public DateTime Min => _min;
		public DateTime Max => _max;
		public TimeSpan Size => Max - Min;
		public DateTime Average => Lerp( 0.5f );
		public bool IsValid => Size.Ticks > 0;

		public DateTime Lerp( float interpolationFactor ) => _min + TimeSpan.FromTicks( (long)(interpolationFactor * Size.Ticks) );
		public float InverseLerp( DateTime value ) => MathCore.UnclampedInverseLerp( _min, _max, value );

		public bool InBounds( DateTime value, bool includeMin = true, bool includeMax = true )
		{
			var tolerance = float.Epsilon;
			bool isInside = value > Min && value < Max;
			isInside |= includeMin && Math.Abs( value.Ticks - Min.Ticks ) < tolerance;
			isInside |= includeMax && Math.Abs( value.Ticks - Max.Ticks ) < tolerance;

			return isInside;
		}

		public void Swap()
		{
			var min = Min;
			var max = Max;
			_min = max;
			_max = min;
		}

		public DateTime GetRandom()
		{
			var rnd = UnityEngine.Random.Range( 0, 1f );
			var deltaInTicks = Max.Ticks - Min.Ticks;
			var offsetInTicks = (long)(rnd * deltaInTicks);

			return Min + TimeSpan.FromTicks( offsetInTicks );
		}

		public override string ToString()
		{
			return $"[ {_min.ToString( CultureInfo.InvariantCulture )} , {_max.ToString( CultureInfo.InvariantCulture )} ]";
		}
	}
}
