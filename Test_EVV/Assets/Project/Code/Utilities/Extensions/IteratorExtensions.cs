namespace Utilities.Extensions
{
	using System;
	using System.Collections.Generic;
	using JetBrains.Annotations;

	public static class IteratorExtensions
	{
		public static void ForEach<T>( this IEnumerable<T> collection, [NotNull] Action<T> action )
		{
			foreach ( var element in collection ) 
				action( element );
		}

		public static void ForEach<T>( this IEnumerable<T> collection, [NotNull] Action<T, int> action )
		{
			int index = 0;

			foreach ( var element in collection )
			{
				action( element, index );
				index++;
			}
		}
	}
}