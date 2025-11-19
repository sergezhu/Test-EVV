namespace Utilities.Extensions
{
	using System;

	public static class StringExtensions
	{
		public static string Reverse( this string s )
		{
			// https://stackoverflow.com/questions/228038/best-way-to-reverse-a-string

			char[] charArray = s.ToCharArray();
			Array.Reverse( charArray );
			return new string( charArray );
		}
	}
}