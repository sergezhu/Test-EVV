namespace Utilities.RxUI
{
	using UnityEngine;

	public class RectTransformOwner: MonoBehaviour
	{
		protected RectTransform _rectTransform;

		public void Initialize()
		{
			TryGetComponent( out _rectTransform );
		}

		public RectTransform RectTransform => _rectTransform;
		public Vector2 SizeDelta => _rectTransform.sizeDelta;
		public Vector2 Size => _rectTransform.rect.size;
		public Vector3 Center() => _rectTransform.TransformPoint( _rectTransform.rect.center );
		public Vector2 Position => _rectTransform.position;
		public Vector2 LocalScale => _rectTransform.localScale;
	}
}