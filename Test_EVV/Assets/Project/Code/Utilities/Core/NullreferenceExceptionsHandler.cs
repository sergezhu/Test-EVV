namespace Utilities.Core
{
	using System;
	using UnityEngine;

	public static class NullreferenceExceptionsHandler
	{
		//[RuntimeInitializeOnLoadMethod]
		static void InitExceptionHandler()
		{
			AppDomain.CurrentDomain.UnhandledException += ( sender, args ) =>
			{
				var exception = args.ExceptionObject as Exception;
				if ( exception is NullReferenceException )
				{
					Debug.LogError( $"🔥 Caught NullReferenceException:\n{exception}\nStackTrace: {exception.StackTrace}" );
				}
			};
		}

		//[RuntimeInitializeOnLoadMethod]
		private static void SetupLogCatcher()
		{
			Application.logMessageReceived += HandleLog;
		}

		private static void HandleLog( string condition, string stackTrace, LogType type )
		{
			if ( type == LogType.Exception && condition.StartsWith( "NullReferenceException" ) )
			{
				Debug.LogError( $"🔥 Caught NRE via log hook: {condition}\n{stackTrace}" );
				// Тут можно сделать дополнительный анализ, лог в файл, уведомление и т.д.
			}
		}
	}

	
}