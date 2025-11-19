namespace Utilities.Extensions
{
	using System.Collections.Generic;
	using System.Linq;
	using Unity.Mathematics;
	using UnityEngine;

	public static class VectorsExtensions
	{
		// Vector2 to Int

		public static Vector2Int RoundToInt( this Vector2 v )
		{
			return new Vector2Int( Mathf.RoundToInt( v.x ), Mathf.RoundToInt( v.y ) );
		}

		public static Vector2Int FloorToInt( this Vector2 v )
		{
			return new Vector2Int( Mathf.FloorToInt( v.x ), Mathf.FloorToInt( v.y ) );
		}

		public static Vector2Int CeilToInt( this Vector2 v )
		{
			return new Vector2Int( Mathf.CeilToInt( v.x ), Mathf.CeilToInt( v.y ) );
		}

		// Vector2Int

		public static Vector2Int Mod( this Vector2Int v, int mod )
		{
			return new Vector2Int( v.x % mod, v.y % mod );
		}

		public static int Sum( this Vector2Int v )
		{
			return v.x + v.y;
		}

		public static int Min( this Vector2Int v )
		{
			return Mathf.Min( v.x, v.y );
		}

		public static int Max( this Vector2Int v )
		{
			return Mathf.Max( v.x, v.y );
		}

		public static int Area( this Vector2Int v )
		{
			return v.x * v.y;
		}


		public static Vector2Int Swap( this Vector2Int v )
		{
			return new Vector2Int( v.y, v.x );
		}

		public static Vector2Int Rotate90( this Vector2Int v, bool clockWise )
		{
			return new Vector2Int( v.y, -v.x ) * (clockWise ? 1 : -1);
		}

		public static Vector2Int RotateRef90( ref this Vector2Int v, bool clockWise )
		{
			return v = v.Rotate90( clockWise );
		}

		public static int Random( this Vector2Int v )
		{
			return UnityEngine.Random.Range( v.x, v.y + 1 );
		}

		public static int Dot( Vector2Int a, Vector2Int b )
		{
			return a.x * b.x + a.y * b.y;
		}

		public static Vector2Int ClampMinusOnePlusOne( this Vector2Int v )
		{
			return new Vector2Int( Mathf.Clamp( v.x, -1, 1 ), Mathf.Clamp( v.y, -1, 1 ) );
		}


		// Vector2

		public static float Sum( this Vector2 v )
		{
			return v.x + v.y;
		}

		public static float Min( this Vector2 v )
		{
			return Mathf.Min( v.x, v.y );
		}

		public static float Max( this Vector2 v )
		{
			return Mathf.Max( v.x, v.y );
		}

		public static float Area( this Vector2 v )
		{
			return v.x * v.y;
		}

		public static Vector2 Swap( this Vector2 v )
		{
			return new Vector2( v.y, v.x );
		}

		public static Vector2 Clamp01( this Vector2 v )
		{
			return new Vector2( Mathf.Clamp01( v.x ), Mathf.Clamp01( v.y ) );
		}

		public static Vector2 RotateRadians( this Vector2 v, float angle )
		{
			var x = v.x * Mathf.Cos( angle ) - v.y * Mathf.Sin( angle );
			var y = v.x * Mathf.Sin( angle ) + v.y * Mathf.Cos( angle );
			
			return new Vector2( x, y );
		}

		public static Vector2 RotateDegrees( this Vector2 v, float angle )
		{
			return RotateRadians( v, Mathf.Rad2Deg * angle );
		}
		
		public static Vector2 Rotate90( this Vector2 v, bool clockWise = true )
		{
			return new Vector2( v.y, -v.x ) * (clockWise ? 1 : -1);
		}

		public static Vector2 RotateRef90( ref this Vector2 v, bool clockWise )
		{
			return v = v.Rotate90( clockWise );
		}

		public static float Random( this Vector2 v )
		{
			return UnityEngine.Random.Range( v.x, v.y );
		}

		public static bool InTriangle( this Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2 )
		{
			var a = .5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
			var sign = a < 0 ? -1 : 1;
			var s = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y) * sign;
			var t = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y) * sign;

			return s > 0 && t > 0 && s + t < 2 * a * sign;
		}

		// Vector3

		public static bool InBounds( this Vector3 v, Vector3 lowerBounds, Vector3 upperBounds )
		{
			var inX = v.x >= lowerBounds.x && v.x <= upperBounds.x;
			var inY = v.y >= lowerBounds.y && v.y <= upperBounds.y;
			var inZ = v.z >= lowerBounds.z && v.z <= upperBounds.z;

			return inX && inY && inZ;
		}

		public static bool InBounds( this double[] v, Vector3 lowerBounds, Vector3 upperBounds )
		{
			var inX = v[0] >= lowerBounds.x && v[0] <= upperBounds.x;
			var inY = v[1] >= lowerBounds.y && v[1] <= upperBounds.y;
			var inZ = v[2] >= lowerBounds.z && v[2] <= upperBounds.z;

			return inX && inY && inZ;
		}

		public static Vector3 Clamp( this Vector3 v, Vector3 from, Vector3 to )
		{
			var x = Mathf.Clamp( v.x, from.x, to.x );
			var y = Mathf.Clamp( v.y, from.y, to.y );
			var z = Mathf.Clamp( v.z, from.z, to.z );

			return new Vector3( x, y, z );
		}

		public static Vector3 Clamp( this Vector3 v, float from, float to )
		{
			var x = Mathf.Clamp( v.x, from, to );
			var y = Mathf.Clamp( v.y, from, to );
			var z = Mathf.Clamp( v.z, from, to );

			return new Vector3( x, y, z );
		}

		/*public static Vector3 Double3ToVector3( this double[] value )
		{
			if ( value == null )
				throw new NullReferenceException();

			return new Vector3( (float) value[0], (float) value[1], (float) value[2] );
		}

		public static double[] Vector3ToDouble3( this Vector3 vector )
		{
			return new double[] {vector.x, vector.y, vector.z};
		}*/

		public static Bounds GetBounds( this IEnumerable<Vector3> points )
		{
			var pointsArray = points.ToArray();

			var p0 = pointsArray[0];

			var minX = p0.x;
			var maxX = p0.x;
			var minY = p0.y;
			var maxY = p0.y;
			var minZ = p0.z;
			var maxZ = p0.z;

			pointsArray.ForEach( p =>
			{
				minX = p.x < minX ? p.x : minX;
				maxX = p.x > maxX ? p.x : maxX;

				minY = p.y < minY ? p.y : minY;
				maxY = p.y > maxY ? p.y : maxY;

				minZ = p.z < minZ ? p.z : minZ;
				maxZ = p.z > maxZ ? p.z : maxZ;
			} );

			var center = new Vector3( 0.5f * (minX + maxX), 0.5f * (minY + maxY), 0.5f * (minZ + maxZ) );
			var size = new Vector3( maxX - minX, maxY - minY, maxZ - minZ );

			return new Bounds( center, size );
		}

		public static Rect GetBounds( this IEnumerable<Vector2> points )
		{
			var pointsArray = points.ToArray();

			var p0 = pointsArray[0];

			var minX = p0.x;
			var maxX = p0.x;
			var minY = p0.y;
			var maxY = p0.y;

			pointsArray.ForEach( p =>
			{
				minX = p.x < minX ? p.x : minX;
				maxX = p.x > maxX ? p.x : maxX;

				minY = p.y < minY ? p.y : minY;
				maxY = p.y > maxY ? p.y : maxY;
			} );

			var pos = new Vector2( minX, minY );
			var size = new Vector2( maxX - minX, maxY - minY );

			return new Rect( pos, size );
		}

		// float3

		public static float3 Clamp( this float3 v, float3 from, float3 to )
		{
			var x = math.clamp( v.x, from.x, to.x );
			var y = math.clamp( v.y, from.y, to.y );
			var z = math.clamp( v.z, from.z, to.z );

			return new float3( x, y, z );
		}

		public static float3 Clamp( this float3 v, float3 from, float3 to, float epsilon )
		{
			var x = math.clamp( v.x, from.x - epsilon, to.x + epsilon );
			var y = math.clamp( v.y, from.y - epsilon, to.y + epsilon );
			var z = math.clamp( v.z, from.z - epsilon, to.z + epsilon );

			return new float3( x, y, z );
		}

		public static float3 Lerp( this float3 a, float3 b, float t )
		{
			var lerp = math.lerp( a, b, t );
			return lerp.xxx;
		}

		// Vectors Int substitutions

		public static Vector3Int x1y( this Vector2Int v )
		{
			return new Vector3Int( v.x, 1, v.y );
		}

		public static Vector3Int x0y( this Vector2Int v )
		{
			return new Vector3Int( v.x, 0, v.y );
		}

		public static Vector2Int xz( this Vector3Int v )
		{
			return new Vector2Int( v.x, v.z );
		}

		public static Vector3Int WithXZ( this Vector3Int v, int x, int z )
		{
			v.x = x;
			v.z = z;

			return v;
		}

		public static Vector3Int WithXZ( this Vector3Int v, Vector2Int xz )
		{
			return v.WithXZ( xz.x, xz.y );
		}

		public static Vector2Int WithX( this Vector2Int v, int x )
		{
			v.x = x;
			return v;
		}

		public static Vector2Int WithY( this Vector2Int v, int y )
		{
			v.y = y;
			return v;
		}

		public static Vector3Int WithX( this Vector3Int v, int x )
		{
			v.x = x;
			return v;
		}

		public static Vector3Int WithY( this Vector3Int v, int y )
		{
			v.y = y;
			return v;
		}

		public static Vector3Int WithZ( this Vector3Int v, int z )
		{
			v.y = z;
			return v;
		}


		// Vectors Float substitutions

		public static Vector3 x1y( this Vector2 v )
		{
			return new Vector3( v.x, 1, v.y );
		}

		public static Vector3 x0y( this Vector2 v )
		{
			return new Vector3( v.x, 0, v.y );
		}

		public static Vector2 xz( this Vector3 v )
		{
			return new Vector2( v.x, v.z );
		}

		public static Vector4 xyz1( this Vector3 v )
		{
			return new Vector4( v.x, v.y, v.z, 1 );
		}

		public static Vector3 WithXY( this Vector3 v, float x, float y )
		{
			v.x = x;
			v.y = y;

			return v;
		}

		public static Vector3 WithXY( this Vector3 v, Vector2 xy )
		{
			return v.WithXY( xy.x, xy.y );
		}

		public static Vector3 WithXZ( this Vector3 v, float x, float z )
		{
			v.x = x;
			v.z = z;

			return v;
		}

		public static Vector3 WithXZ( this Vector3 v, Vector2 xz )
		{
			return v.WithXZ( xz.x, xz.y );
		}


		public static Vector2 WithX( this Vector2 v, float x )
		{
			v.x = x;
			return v;
		}

		public static Vector2 WithY( this Vector2 v, float y )
		{
			v.y = y;
			return v;
		}

		public static Vector3 WithX( this Vector3 v, float x )
		{
			v.x = x;
			return v;
		}

		public static Vector3 WithY( this Vector3 v, float y )
		{
			v.y = y;
			return v;
		}

		public static Vector3 WithZ( this Vector3 v, float z )
		{
			v.z = z;
			return v;
		}
	}
}