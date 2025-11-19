namespace Utilities
{
	using UnityEngine;

	public class TimeScaleToggle : MonoBehaviour
	{
		private readonly float[] timeScales = { 0f, 0.1f, 0.33f, 1f };
		private int currentIndex = 0;

		void Update()
		{
			if ( Input.GetKeyDown( KeyCode.Space ) )
			{
				currentIndex = (currentIndex + 1) % timeScales.Length;
				Time.timeScale = timeScales[currentIndex];
				
				Debug.Log( $"Time.timeScale set to {Time.timeScale}" );
			}
		}
	}
}