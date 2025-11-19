namespace Utilities.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class TypeHelper
	{
		/// <summary>
		/// Возвращает список всех наследников указанного типа
		/// </summary>
		public static List<Type> GetAllSubclasses( Type baseType )
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany( assembly => assembly.GetTypes() )
				.Where( type => type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom( type ) )
				.ToList();
		}
	}
}