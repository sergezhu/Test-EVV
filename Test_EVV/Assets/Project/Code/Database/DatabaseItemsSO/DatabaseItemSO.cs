namespace Code.Database
{
	using System;
	using Code.MergeSystem;
	using Sirenix.Utilities;
	using UnityEngine;

	[Serializable]
	public abstract class DatabaseItemSO : ScriptableObject
	{
		[SerializeField] private uint _id;
		[SerializeField] private string _name;
		[SerializeField] private Sprite _icon;
		[SerializeField] private MergeItemView _boardItemPrefab;


		public uint ID => _id;
		public string Name => _name;
		public Sprite Icon => _icon;
		public MergeItemView BoardItemPrefab => _boardItemPrefab;

		public abstract DatabaseItem GetItem();

		

		private void OnValidate()
		{
			#if UNITY_EDITOR
			
			if ( _name.IsNullOrWhitespace() == false )
			{
				var trimName = _name.Trim();
				var fileName = $"[{_id:0000}] {trimName}";

				string assetPath = UnityEditor.AssetDatabase.GetAssetPath( GetInstanceID() );
				UnityEditor.AssetDatabase.RenameAsset( assetPath, fileName );
				UnityEditor.AssetDatabase.SaveAssets();
			}
			
			#endif
		}
	}
}