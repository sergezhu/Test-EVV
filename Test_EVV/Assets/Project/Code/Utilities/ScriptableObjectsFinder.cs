namespace Utilities
{
	#if UNITY_EDITOR
	
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.IO;

	public static class ScriptableObjectFinder
	{
		/// <summary>
		/// Ищет все ScriptableObject указанного типа в папке и её подпапках.
		/// </summary>
		/// <typeparam name="T">Тип ScriptableObject</typeparam>
		/// <param name="folderPath">Папка для поиска (относительно Assets)</param>
		/// <returns>Список найденных объектов</returns>
		public static List<T> FindAllScriptableObjects<T>( string folderPath ) where T : ScriptableObject
		{
			List<T> results = new List<T>();

			// Получаем путь в системе файлов
			string absolutePath = Path.Combine( Application.dataPath, folderPath );

			if ( !Directory.Exists( absolutePath ) )
			{
				Debug.LogWarning( $"Папка '{folderPath}' не найдена!" );
				return results;
			}

			// Получаем все asset-файлы в папке и подпапках
			string[] files = Directory.GetFiles( absolutePath, "*.asset", SearchOption.AllDirectories );

			foreach ( string file in files )
			{
				// Преобразуем путь в относительный для Unity (Assets/...)
				string relativePath = "Assets" + file.Replace( Application.dataPath, "" ).Replace( "\\", "/" );

				// Загружаем объект
				T asset = AssetDatabase.LoadAssetAtPath<T>( relativePath );
				if ( asset != null )
				{
					results.Add( asset );
				}
			}

			return results;
		}
	}
	#endif
}