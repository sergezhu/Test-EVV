namespace Code.MergeSystem
{
	using System;
	using Code.FX;
	using DG.Tweening;
	using TMPro;
	using UnityEngine;

	public class MergeItemView : MonoBehaviour
	{
		[SerializeField] private BoxCollider raycastCollider;
		[SerializeField] private TextMeshProUGUI mergeLevelText;
		[SerializeField] private FXWrapper spawnFX;
		[SerializeField] private FXWrapper mergeFX;


		private Vector3 defaultPos;
		private MergeConfig mergeConfig;

		public Transform Transform => transform;
		public string Name => gameObject.name;

		public bool TouchStartFlag { get; private set; }
		public bool TouchEndFlag { get; private set; }


		private void OnDisable()
		{
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(transform.position + raycastCollider.center, raycastCollider.size);
		}

		private void OnValidate()
		{
			raycastCollider = GetComponent<BoxCollider>();
		}

		public void Construct(MergeConfig mergeConfig)
		{
			this.mergeConfig = mergeConfig;
		}

		public void SetInitialGeometry(Vector3 worldPos, Transform parent)
		{
			defaultPos = worldPos;
			transform.position = worldPos;
			transform.SetParent(parent);
		}

		public void SetPosition(Vector3 worldPos)
		{
			transform.position = worldPos;
		}

		public void ResetPosition()
		{
			transform.position = defaultPos;
		}

		public void PlaySpawnFX()
		{
			//Debug.Log($"play spawn FX");
			spawnFX.Play();
		}

		public void PlayMergeFX()
		{
			//Debug.Log($"play merge FX");
			mergeFX.Play();
		}

		public void DoScaleInAnimation(Action completeCallback = null)
		{
		}

		public void DoScaleOutAnimation(Action completeCallback = null)
		{
		}

		public void AnimateScale(Vector3 from, Vector3 to, float duration, Ease ease, Action completeCallback = null)
		{
			Transform.localScale = from;

			Transform
				.DOScale(to, duration)
				.SetEase(ease)
				.OnComplete(() => completeCallback?.Invoke());
		}


		public void OnTouchStartHit()
		{
			TouchStartFlag = true;
		}

		public void OnTouchEndHit()
		{
			TouchEndFlag = true;
		}

		public void ResetTouchFlags()
		{
			TouchStartFlag = false;
			TouchEndFlag = false;
		}

		public void SetDraggedItemLayer()
		{
			gameObject.layer = mergeConfig.DraggedMergeItemLayer;
		}

		public void SetDefaultItemLayer()
		{
			gameObject.layer = mergeConfig.MergeItemLayer;
		}

		public void SetMergeLevelText(int mergeLevel)
		{
			mergeLevelText.text = $"{mergeLevel + 1}";
		}
	}
}