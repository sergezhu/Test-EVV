namespace Code.Database
{
	using System;
	using Code.MergeSystem;
	using Sirenix.Utilities;
	using UnityEditor;
	using UnityEngine;

	[Serializable]
	public abstract class DatabaseItemSO : ScriptableObject
	{
		[SerializeField] private uint id;
		[SerializeField] private string itemName;
		[SerializeField] private Sprite icon;
		[SerializeField] private MergeItemView boardItemPrefab;


		public uint ID => id;
		public string Name => itemName;
		public Sprite Icon => icon;
		public MergeItemView BoardItemPrefab => boardItemPrefab;


		private void OnValidate()
		{
			#if UNITY_EDITOR

			if (itemName.IsNullOrWhitespace() == false)
			{
				string trimName = itemName.Trim();
				string fileName = $"[{id:0000}] {trimName}";

				string assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
				AssetDatabase.RenameAsset(assetPath, fileName);
				AssetDatabase.SaveAssets();
			}

			#endif
		}

		public abstract DatabaseItem GetItem();
	}
}