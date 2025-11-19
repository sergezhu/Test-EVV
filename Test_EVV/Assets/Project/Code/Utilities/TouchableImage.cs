namespace _Game._Scripts.Utilities
{
	using UnityEngine;
	using UnityEngine.UI;

	// http://answers.unity.com/answers/851816/view.html
	// http://answers.unity.com/answers/1165070/view.html

	public class TouchableImage : Graphic
	{
		public override bool Raycast( Vector2 sp, Camera eventCamera )
		{
			// return base.Raycast( sp, eventCamera );
			return true;
		}

 
		protected override void OnPopulateMesh( VertexHelper vertexHelper )
		{
			vertexHelper.Clear();
		}
	}
}