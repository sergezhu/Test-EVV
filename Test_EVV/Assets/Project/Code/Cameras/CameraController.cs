namespace Code.Cameras
{
	using UnityEngine;
	using Utilities.Extensions;

	public class CameraController : MonoBehaviour
	{
		[SerializeField] private Camera activeCamera;


		public Vector3 ActiveCameraPosition()
		{
			return activeCamera.transform.position;
		}

		public Vector3 GetPointAtHeight(float screenRelX, float screenRelY, float touchWorldHeight)
		{
			return activeCamera.GetPointAtHeightFromRelative(screenRelX, screenRelY, touchWorldHeight);
		}
	}
}