namespace Code.Database
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class DatabaseUtilityEditor
	{
		private static ItemsLibrary itemsLibrary;

		public static IEnumerable<string> GetDefinitionsNames(string[] categories = null)
		{
			#if UNITY_EDITOR
			itemsLibrary = GetItemsLibraryEditMode();
			#endif

			if (itemsLibrary == null)
				throw new InvalidOperationException("GetDefinitionsNames : ItemsLibrary is null");

			var items = itemsLibrary.GetAllItems();
			var names = items.Select(item => item.Name).ToList();

			return names;
		}
		
		#if UNITY_EDITOR
		private static ItemsLibrary GetItemsLibraryEditMode()
		{
			var path = "Assets/Project/Configs/ItemsLibrary.asset";
			itemsLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemsLibrary>(path);

			return itemsLibrary;
		}
		#endif
	}
}