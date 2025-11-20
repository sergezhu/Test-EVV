namespace Code.MergeSystem
{
	using System;
	using Code.Database;
	using UnityEngine;
	using Object = UnityEngine.Object;

	public class MergeItem : IDisposable
	{
		private readonly IDatabaseItem dbItem;
		private readonly MergeConfig mergeConfig;
		private readonly MergeItemView view;

		public MergeItem(IDatabaseItem dbItem, MergeItemView view, int mergeLevel)
		{
			this.dbItem = dbItem;
			this.view = view;
			MergeLevel = mergeLevel;

			this.view.SetMergeLevelText(MergeLevel);
		}

		public int MergeLevel { get; }

		public bool TouchStartFlag => view.TouchStartFlag;
		public bool TouchEndFlag => view.TouchEndFlag;
		public ItemDbInfo DbInfo => new ItemDbInfo(dbItem.ID, dbItem.Name);

		public void Dispose()
		{
		}


		public void SetPosition(Vector3 worldPosition)
		{
			if (view == null)
				throw new NullReferenceException("(MergeItemView) _view is null");

			view.SetPosition(worldPosition);
		}

		public void Rollback()
		{
			view.ResetPosition();
			view.ResetTouchFlags();
		}

		public void DestroyView()
		{
			Object.Destroy(view.gameObject);
		}

		public void PlayShowAnimation(Action onComplete)
		{
			view.DoScaleInAnimation(onComplete);
		}

		public void PlayHideAnimation(Action onComplete)
		{
			view.DoScaleOutAnimation(onComplete);
		}


		public void SetDraggedItemLayer()
		{
			view.SetDraggedItemLayer();
		}

		public void SetDefaultItemLayer()
		{
			view.SetDefaultItemLayer();
		}

		public void PlaySpawnFX()
		{
			view.PlaySpawnFX();
		}

		public void PlayMergeFX()
		{
			view.PlayMergeFX();
		}
	}
}