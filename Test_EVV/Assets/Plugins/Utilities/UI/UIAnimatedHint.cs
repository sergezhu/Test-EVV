namespace Utilities.UI
{
	using System;
	using DG.Tweening;
	using global::Utilities.Extensions;
	using Sirenix.OdinInspector;
	using UnityEngine;

	public interface IUIHintedElement
	{
		public void ShowHint();
		public void HideHint();
	}

	public interface IUIAnimatedHintedElement : IUIHintedElement
	{
		public void ShowHint( Action onComplete );
		public void HideHint( Action onComplete );
	}
	
	public class UIAnimatedHint : MonoBehaviour,IUIAnimatedHintedElement
	{
		[SerializeField] private CanvasGroup _hintCG;
		[SerializeField] private float _fadeInDuration;
		[SerializeField] private float _fadeOutDuration;
		[SerializeField] private float _yOffset;
		[SerializeField] private float _offsetDuration;
		[SerializeField] private float _showDuration;

		Sequence _sequence;
		
		private RectTransform _rt;
		private Vector3 _startPosY;

		private void Awake()
		{
			_rt = GetComponent<RectTransform>();
			_startPosY = _rt.localPosition;
		}

		public void ShowHint()
		{
			_hintCG.Show();
		}

		public void HideHint()
		{
			_hintCG.Hide();
		}

		public void ShowHint( Action onComplete )
		{
			_sequence?.Kill();

			var startLocalScaleFactor = 0.5f;
			
			_hintCG.alpha = 0;
			_rt.localPosition = _startPosY;
			_rt.localScale = startLocalScaleFactor * Vector3.one;
			
			_hintCG.Show();

			_sequence = DOTween.Sequence();
			

			_sequence.Insert( 0, DOVirtual.Float( 0f, 1f, _fadeInDuration, v =>
			{
				_hintCG.alpha = v;
				_rt.localScale = Vector3.Lerp( startLocalScaleFactor * Vector3.one, Vector3.one, v );
			} ) );
			
			_sequence.Insert( _fadeInDuration, DOVirtual.Float( 0f, 1f, _offsetDuration, v =>
								  {
									  var pos = Vector3.Lerp( _startPosY, _startPosY + _yOffset * Vector3.up, v );
									  _rt.localPosition = pos;
								  } )
								  .SetEase( Ease.InOutSine )
								  .SetLoops( 10, LoopType.Yoyo ) );
			
			_sequence.Insert( _fadeInDuration + _showDuration, DOVirtual.Float( 1f, 0f, _fadeOutDuration, v => _hintCG.alpha = v ) );

			_sequence.OnComplete( () =>
			{
				_sequence = null;
				onComplete?.Invoke();
			} );
		}

		public void HideHint( Action onComplete )
		{
			_sequence?.Kill();

			_hintCG.alpha = 1;

			_sequence.Insert( 0, DOVirtual.Float( 0f, 1f, _fadeInDuration, v => _hintCG.alpha = v ) );

			_sequence.OnComplete( () =>
			{
				_sequence = null;
				_hintCG.Hide();
				
				onComplete?.Invoke();
			} );
		}

		[Button]
		private void ShowHintDebug()
		{
			ShowHint(null);
		}
	}
}