namespace Utilities.RxUI
{
	#if UNIRX
	
	using UnityEngine;
	using UnityEngine.UI;

	public class UIProgressButton : UIMultiImageButton
	{
		[Space]
		[SerializeField] private Image _progressImage;
		[field:SerializeField] public bool IsProgressChanging { get; set; }

		public void SetProgress( float value )
		{
			value = Mathf.Clamp( value, 0, 1f );
			_progressImage.fillAmount = value;
		}
	}

	#endif
}