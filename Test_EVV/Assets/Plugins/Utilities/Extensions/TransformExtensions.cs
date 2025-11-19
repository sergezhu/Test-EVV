namespace Utilities.Extensions
{
	using System.Linq;
	using UnityEngine;

	public static class TransformExtensions
	{
		public static Vector2[] ToCornersXZ( this Transform t )
		{
			Vector2 pos = t.localPosition.xz();
			Quaternion rot = t.localRotation;
			Vector2 size = t.localScale.xz();

			var corners = new Vector3[]
			{
				new Vector3( -0.5f * size.x, 0, -0.5f * size.y ),
				new Vector3( +0.5f * size.x, 0, -0.5f * size.y ),
				new Vector3( +0.5f * size.x, 0, +0.5f * size.y ),
				new Vector3( -0.5f * size.x, 0, +0.5f * size.y ),
			};

			return corners.Select( c => (rot * c).xz() + pos ).ToArray();
		}

		public static Vector2[] ToCornersXZ( this Transform t, Vector2 size )
		{
			Vector2 pos = t.localPosition.xz();
			Quaternion rot = t.localRotation;

			var corners = new Vector3[]
			{
				new Vector3(  - 0.5f * size.x, 0,  - 0.5f * size.y ),
				new Vector3(  + 0.5f * size.x, 0,  - 0.5f * size.y ),
				new Vector3(  + 0.5f * size.x, 0,  + 0.5f * size.y ),
				new Vector3(  - 0.5f * size.x, 0,  + 0.5f * size.y ),
			};

			return corners.Select( c => (rot * c).xz() + pos ).ToArray();
		}
	}
}