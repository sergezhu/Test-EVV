namespace Utilities.RxUI
{
	using System;
	using System.Linq;
	using Sirenix.OdinInspector;
	using UniRx;
	using UnityEngine;
	using UnityEngine.UI;
	using Utilities.Extensions;

	public class UIImageButton : UIBaseButton
	{
		[SerializeField, Space] private Image _defaultIconImage;
		[SerializeField] private Image[] _customIconImages;
		
		[SerializeField, Space] private RectTransform _lockLayer;
		[SerializeField, ReadOnly] private bool _isLocked;

        private float[] _defaultOpacities;

		protected override void CustomInitialize()
		{
			base.CustomInitialize();

            _defaultOpacities = _customIconImages.Length > 0
                ? _customIconImages.Select( img => img.color.a ).ToArray()
                : Array.Empty<float>();
			
			ClearIcon();
			SetLockState( false, true );

           // IsEnabled.Subscribe(v => _isEnabled = v ).AddTo(this);
            //Click.Subscribe(v => Debug.Log( "UIImageButton CLICK" ) ).AddTo(this);
		}

		public void SetIcon( Sprite icon )
		{
			foreach ( Image image in _customIconImages )
            {
                image.sprite = icon;
                image.Show();
            }

            if(_defaultIconImage != null)
				_defaultIconImage.Hide(); 
		}

		public void ClearIcon()
		{
            foreach (Image image in _customIconImages)
            {
                image.Hide();
            }

            SetIconOpacity( _defaultOpacities );

			if(_defaultIconImage != null)
				_defaultIconImage.Show();
		}

		public Sprite GetIcon()
		{
			return _customIconImages.First().sprite;
		}

        public void SetIconOpacity( float[] opacities )
        {
            for (int i = 0; i < opacities.Length; i++)
            {
                var image = _customIconImages[i];
                var opacity = Mathf.Clamp( opacities[i], 0f, 1f );
                var color = image.color;
                color.a = opacity;
                image.color = color;
            }
        }

		public void SetIconOpacity( float opacity )
		{
			opacity = Mathf.Clamp( opacity, 0f, 1f );
			

            foreach (Image image in _customIconImages)
            {
                var color = image.color;
                color.a = opacity;
                image.color = color;
            }
        }

		public void SetLockState( bool isLocked, bool clearIfLocked )
		{
			_isLocked = isLocked;
			
			if ( _lockLayer == null )
				return;

			if ( isLocked )
			{
				if( clearIfLocked )
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