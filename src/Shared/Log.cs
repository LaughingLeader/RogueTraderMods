using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leader
{
	public struct LogCallbacks
	{
		public Action<string> Info;
		public Action<Exception> Exception;
		public Action<string, Exception> Exception2;
		public Action<string> Error;
		public Action<string> Critical;
		public Action<string> Warning;
		public Action<string> NativeLog;
	}

	public static class Log
	{
		private static LogCallbacks _callbacks;

		public static void Configure(LogCallbacks callbacks) => _callbacks = callbacks;

		public static void Info(string message) => _callbacks.Info(message);
		public static void Error(string message) => _callbacks.Error(message);
		public static void Critical(string message) => _callbacks.Critical(message);
		public static void Warning(string message) => _callbacks.Warning(message);
		public static void NativeLog(string message) => _callbacks.NativeLog(message);
		public static void Exception(Exception ex) => _callbacks.Exception(ex);
		public static void Exception(string message, Exception ex) => _callbacks.Exception2(message, ex);
	}
}