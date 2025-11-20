namespace Utilities.UI
{
	using UnityEngine;

	public class UITutorialSingleMask : MonoBehaviour
	{
		private RectTransform rectTransform;

		private RectTransform RT => rectTransform ? rectTransform : GetComponent<RectTransform>();

		//public void SetGeometry(Vector3 position, Vector2 size, float horizontalPadding = 0, float verticalPadding = 0 )
		
		public void SetPosition( Vector3 targetPosition )
		{
			var pivot = RT.pivot;
			var centerPivot = new Vector2( 0.5f, 0.5f );
			var pivotDelta = centerPivot - pivot;
			var size = RT.sizeDelta;

			var offsetX = size.x * pivotDelta.x;
			var offsetY = size.y * pivotDelta.y;
			
			//Debug.Log( $"size : {size}, pivotDelta : {pivotDelta}, ox : {offsetX} oy {offsetY}" );

			transform.position = targetPosition + new Vector3(offsetX, offsetY, 0);
		}

		public void SetSize( Vector2 size, float horizontalPadding = 0, float verticalPadding = 0 )
		{
			var sizeX = size.x + 2f * horizontalPadding;
			var sizeY = size.y + 2f * verticalPadding;

			RT.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, sizeX );
			RT.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, sizeY );
		}
	}
}