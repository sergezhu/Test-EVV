namespace Code.MergeSystem
{
	using System;
	using Code.Database;
	using UnityEngine;
	using Object = UnityEngine.Object;

	public class MergeItem : IDisposable
	{
		private readonly IDatabaseItem _dbItem;
		private readonly MergeItemView _view;
		private readonly int _mergeLevel;
		private readonly MergeConfig _mergeConfig;
		private readonly ItemDbInfo _dbInfo;

		public MergeItem( IDatabaseItem dbItem, MergeItemView view, int mergeLevel )
		{
			_dbItem = dbItem;
			_view = view;
			_mergeLevel = mergeLevel;
			_dbInfo = new ItemDbInfo { ID = dbItem.ID, Name = dbItem.Name };

			_view.SetMergeLevelText( _mergeLevel );
		}

		public int MergeLevel => _mergeLevel;
		public bool TouchStartFlag => _view.TouchStartFlag;
		public bool TouchEndFlag => _view.TouchEndFlag;
		public ItemDbInfo DbInfo => new ItemDbInfo { ID = _dbItem.ID, Name = _dbItem.Name };


		public void SetPosition( Vector3 worldPosition )
		{
			if ( _view == null )
				throw new NullReferenceException("(MergeItemView) _view is null");
			
			_view.SetPosition( worldPosition );
		}

		public void Rollback()
		{
			_view.ResetPosition();
			_view.ResetTouchFlags();
		}

		public void DestroyView()
		{
			Object.Destroy( _view.gameObject );
		}

		public void Dispose()
		{
		}

		public void PlayShowAnimation( Action onComplete )
		{
			_view.DoScaleInAnimation( onComplete );
		}

		public void PlayHideAnimation( Action onComplete )
		{
			_view.DoScaleOutAnimation( onComplete );
		}


		public void SetDraggedItemLayer() => _view.SetDraggedItemLayer();
		public void SetDefaultItemLayer() => _view.SetDefaultItemLayer();

		public void PlaySpawnFX() => _view.PlaySpawnFX();
		public void PlayMergeFX() => _view.PlayMergeFX();
	}
}