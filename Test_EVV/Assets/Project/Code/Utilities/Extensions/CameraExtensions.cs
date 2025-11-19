namespace Utilities.Extensions
{
	using UnityEngine;

	public static class CameraExtensions
	{
		public static Vector2Int ScreenSize => new Vector2Int( Screen.width, Screen.height );
		
		public static Vector3 GetPointAtHeightFromRelative( this Camera camera, float screenX, float screenY, float height )
		{
			Ray ray = camera.ViewportPointToRay( new Vector3( screenX, screenY, 0 ) );
			Vector3 atHeight = ray.origin + (ray.origin.y - height) / -ray.direction.y * ray.direction;

			return atHeight;
		}
		
		public static Vector3 GetPointAtHeightFromAbsolute( this Camera camera, float screenX, float screenY, float height )
		{
			Vector2 screenSize = ScreenSize;
			Vector2 relativePos = new Vector2( screenX / screenSize.x, screenY / screenSize.y );

			return camera.GetPointAtHeightFromRelative( relativePos.x, relativePos.y, height );
		}
	}
}