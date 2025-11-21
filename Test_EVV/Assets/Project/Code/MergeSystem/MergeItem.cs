namespace Code.MergeSystem
{
	using System;
	using Code.Database;
	using UnityEngine;
	using Utilities.Extensions;
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
		public Vector3 Position => view.Transform.position;
		public Transform Parent => view.Transform.parent;

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
			view.DoShowAnimation(onComplete);
		}

		public void PlayHideAnimation(Action onComplete)
		{
			view.DoHideAnimation(onComplete);
		}


		public void SetDraggedItemLayer()
		{
			view.SetAsDragged();
			
			CanvasOrderSwitcher[] canvasOrderSwitchers = view.GetComponentsInChildren<CanvasOrderSwitcher>();
			canvasOrderSwitchers.ForEach(s => s.SetDraggedOrder());
		}

		public void SetDefaultItemLayer()
		{
			view.SetAsDefault();

			CanvasOrderSwitcher[] canvasOrderSwitchers = view.GetComponentsInChildren<CanvasOrderSwitcher>();
			canvasOrderSwitchers.ForEach(s => s.SetDefaultOrder());
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