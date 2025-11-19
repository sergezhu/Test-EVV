namespace Code.Cameras
{
	using UnityEngine;
	using Utilities.Extensions;

	public class CameraController : MonoBehaviour
	{
		[SerializeField] private Camera _activeCamera;


		public Vector3 ActiveCameraPosition()
		{
			return _activeCamera.transform.position;
		}

		public Vector3 GetPointAtHeight( float screenRelX, float screenRelY, float touchWorldHeight )
		{
			return _activeCamera.GetPointAtHeightFromRelative( screenRelX, screenRelY, touchWorldHeight );
		}
	}
}