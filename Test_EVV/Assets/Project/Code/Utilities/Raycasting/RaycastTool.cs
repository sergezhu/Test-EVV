namespace Utilities.Raycasting
{
	using System.Linq;
	using global::Utilities.Extensions;
	using UnityEngine;

	public struct TouchData
	{
		public Vector2 ScreenPosition;

		public Vector3 CameraPosition;
		public Vector3 WorldProjectionFromCamera;
		public Vector3 WorldDirectionFromCamera;
	}

	// mask for example : var mask = (1 << Layers.MergeItem);
	
	public class RaycastTool
	{
		private readonly RaycastHit[] _hits;

		public RaycastTool( int maxHits )
		{
			_hits = new RaycastHit[maxHits];
		}

		/*Vector2Int ScreenSize => new Vector2Int( Screen.width, Screen.height );

		public TComponent ThrowRaycastAtMousePosition<TComponent>( Vector2 mousePosition, Camera camera, float distance, int mask ) where TComponent : Component
		{
			var touchData = CreateTouchData( mousePosition, camera );
			return ThrowRaycastFromCameraPosition<TComponent>( touchData.CameraPosition, touchData.WorldDirectionFromCamera, distance, mask );
		}
		
		
		public TComponent ThrowRaycastAtMousePosition<TComponent>( Vector2 mousePosition, Camera camera, float distance ) where TComponent : Component
		{
			var touchData = CreateTouchData( mousePosition, camera );
			return ThrowRaycastFromCameraPosition<TComponent>( touchData.CameraPosition, touchData.WorldDirectionFromCamera, distance );
		}*/
		
		
		
		public TComponent ThrowRaycastFromCameraPosition<TComponent>( Vector3 camPosition, Vector3 dirFromCam, float distance, int mask, out Vector3? hitPoint ) where TComponent : Component
		{
			for ( var i = 0; i < _hits.Length; i++ )
			{
				_hits[i] = default;
			};
			
			Physics.RaycastNonAlloc( camPosition, dirFromCam, _hits, distance, mask );

			var components = _hits
				.Where( hit => hit.collider != null )
				.Select( hit => hit.collider.gameObject.GetComponent<TComponent>() )
				.ToList();

			var hitPositions = _hits
				.Where( hit => hit.collider != null )
				.Select( hit => hit.point )
				.ToList();

			TComponent firstComponent = null;
			hitPoint = null;

			if ( components.Count > 0 )
			{
				firstComponent = components[0];
				hitPoint = hitPositions[0];

			}

			return firstComponent;
		}

		public TComponent ThrowRaycastFromCameraPosition<TComponent>( Vector3 camPosition, Vector3 dirFromCam, float distance, out Vector3? hitPoint ) where TComponent : Component
		{
			Physics.RaycastNonAlloc( camPosition, dirFromCam, _hits, distance );

			var components = _hits
				.Where( hit => hit.collider != null )
				.Select( hit => hit.collider.gameObject.GetComponent<TComponent>() )
				.ToList();
			
			var hitPositions = _hits
				.Where( hit => hit.collider != null )
				.Select( hit => hit.point )
				.ToList();

			TComponent firstComponent = null;
			hitPoint = null;

			if ( components.Count > 0 )
			{
				firstComponent = components[0];
				hitPoint = hitPositions[0];
			}

			return firstComponent;
		}

		
	}
}