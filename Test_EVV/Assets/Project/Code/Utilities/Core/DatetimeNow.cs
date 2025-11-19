namespace Utilities.Core
{
	using System;

	public static class DatetimeNow
	{
		public static string Value => $"{DateTime.Now:HH:mm:ss.fff}";
		public static string ValueSSFFF => $"{DateTime.Now:ss.fff}";
		public static string ValueFFF => $"{DateTime.Now:fff}";
	}
}