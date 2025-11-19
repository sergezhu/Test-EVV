namespace Utilities.RxUI
{
	using Sirenix.OdinInspector;
	using UnityEngine;
	using UnityEngine.UI;
	using Utilities.Extensions;

	public class UIMultiImageButton : UIBaseButton
	{
		[SerializeField, Space] private Image[] _defaultIconImages;
		[SerializeField] private Image[] _customIconImages;
		
		[SerializeField, Space] private RectTransform _lockLayer;
		[SerializeField, ReadOnly] private bool _isLocked;

		protected override void CustomInitialize()
		{
			base.CustomInitialize();
			
			ClearIcon();
			SetLockState( false );
		}

		public void SetIcon( Sprite icon )
		{
			foreach ( var customIconImage in _customIconImages )
			{
				customIconImage.sprite = icon;
				customIconImage.Show();
			}

			foreach ( var defaultIconImage in _defaultIconImages )
			{
				defaultIconImage.Hide(); 
			}
		}

		public Sprite GetIcon()
		{
			if ( _customIconImages.Length == 0 )
				return null;

			return _customIconImages[0].sprite;
		}

		public void SetIconOpacity( float opacity )
		{
			opacity = Mathf.Clamp( opacity, 0f, 1f );
			var color = Color.white;
			color.a = opacity;

			foreach ( var customIconImage in _customIconImages )
			{
				customIconImage.color = color;
			}
		}
		
		public void ClearIcon()
		{
			foreach ( var customIconImage in _customIconImages )
			{
				customIconImage.Hide();
			}
			
			SetIconOpacity( 1 );

			foreach ( var defaultIconImage in _defaultIconImages )
			{
				defaultIconImage.Show();
			}
		}

		public void SetLockState( bool isLocked )
		{
			_isLocked = isLocked;
			
			if ( _lockLayer == null )
				return;

			if ( isLocked )
			{
				ClearIcon();
				_lockLayer.Show();
			}
			else
			{
				_lockLayer.Hide();
			}
		}
	}
}