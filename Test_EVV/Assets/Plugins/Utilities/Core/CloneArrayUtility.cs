namespace Utilities.Core
{
	using System;
	using System.Linq;

	public static class CloneArrayUtility
	{
		/*public static T[] CloneArray<T>( this T[] source ) where T : struct
		{
			if ( source == null ) 
				return null;
			
			T[] copy = new T[source.Length];
			Array.Copy( source, copy, source.Length );
			
			return copy;
		}*/
		
		public static T[] CloneSimpleArray<T>( this T[] source ) where T : struct
		{
			if ( source == null ) 
				return null;

			if ( source.Length == 0 )
				return Array.Empty<T>();

			return source.ToArray();
		}
		
		public static T[] CloneCloneableArray<T>( this T[] source ) where T : ICloneable<T>
		{
			if ( source == null ) 
				return null;

			if ( source.Length == 0 )
				return Array.Empty<T>();

			var copy = source.Select( c => c.Clone() ).ToArray();
			
			return copy;
		}
	}
}