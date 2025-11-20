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

		Sequence sequence;
		
		private RectTransform rt;
		private Vector3 startPosY;

		private void Awake()
		{
			rt = GetComponent<RectTransform>();
			startPosY = rt.localPosition;
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
			sequence?.Kill();

			var startLocalScaleFactor = 0.5f;
			
			_hintCG.alpha = 0;
			rt.localPosition = startPosY;
			rt.localScale = startLocalScaleFactor * Vector3.one;
			
			_hintCG.Show();

			sequence = DOTween.Sequence();
			

			sequence.Insert( 0, DOVirtual.Float( 0f, 1f, _fadeInDuration, v =>
			{
				_hintCG.alpha = v;
				rt.localScale = Vector3.Lerp( startLocalScaleFactor * Vector3.one, Vector3.one, v );
			} ) );
			
			sequence.Insert( _fadeInDuration, DOVirtual.Float( 0f, 1f, _offsetDuration, v =>
								  {
									  var pos = Vector3.Lerp( startPosY, startPosY + _yOffset * Vector3.up, v );
									  rt.localPosition = pos;
								  } )
								  .SetEase( Ease.InOutSine )
								  .SetLoops( 10, LoopType.Yoyo ) );
			
			sequence.Insert( _fadeInDuration + _showDuration, DOVirtual.Float( 1f, 0f, _fadeOutDuration, v => _hintCG.alpha = v ) );

			sequence.OnComplete( () =>
			{
				sequence = null;
				onComplete?.Invoke();
			} );
		}

		public void HideHint( Action onComplete )
		{
			sequence?.Kill();

			_hintCG.alpha = 1;

			sequence.Insert( 0, DOVirtual.Float( 0f, 1f, _fadeInDuration, v => _hintCG.alpha = v ) );

			sequence.OnComplete( () =>
			{
				sequence = null;
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