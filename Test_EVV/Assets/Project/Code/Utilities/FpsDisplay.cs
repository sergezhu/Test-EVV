namespace Utilities
{
	using TMPro;
	using UnityEngine;

	public class FpsDisplay : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _displayText;
		[SerializeField] [Range( 0f, 1f )] private float _expSmoothingFactor = 0.9f;
		[SerializeField] private float _refreshFrequency = 0.4f;

		private float _timeSinceUpdate = 0f;
		private float _averageFps = 1f;
		private bool _hasFocus;
		private bool _hasPause;

		private void Update()
		{
			if(_hasFocus == false || _hasPause)
				return;
			
			// Exponentially weighted moving average (EWMA)
			_averageFps = _expSmoothingFactor * _averageFps + (1f - _expSmoothingFactor) * 1f / Time.unscaledDeltaTime;

			if ( _timeSinceUpdate < _refreshFrequency )
			{
				_timeSinceUpdate += Time.deltaTime;
				return;
			}

			int fps = Mathf.RoundToInt( _averageFps );
			_displayText.text = $"FPS: {fps}";

			_timeSinceUpdate = 0f;
		}

		private void OnApplicationFocus( bool hasFocus )
		{
			_hasFocus = hasFocus;
		}

		private void OnApplicationPause( bool hasPause )
		{
			_hasPause = hasPause;
		}
	}
}