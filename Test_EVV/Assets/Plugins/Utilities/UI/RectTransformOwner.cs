namespace Utilities.RxUI
{
	using UnityEngine;

	public class RectTransformOwner: MonoBehaviour
	{
		protected RectTransform RT;

		public void Initialize()
		{
			TryGetComponent( out RT);
		}

		public RectTransform RectTransform => RT;
		public Vector2 SizeDelta => RT.sizeDelta;
		public Vector2 Size => RT.rect.size;
		public Vector3 Center() => RT.TransformPoint(RT.rect.center );
		public Vector2 Position => RT.position;
		public Vector2 LocalScale => RT.localScale;
	}
}