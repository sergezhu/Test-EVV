namespace Code.Input.Touch
{
	using UnityEngine;

	public struct TouchData
	{
		public Vector2 ScreenPosition;

		public Vector3 MainCameraPosition;
		public Vector3 WorldProjectionFromMainCamera;
		public Vector3 WorldDirectionFromMainCamera;
	}
}