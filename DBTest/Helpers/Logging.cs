﻿namespace DBTest
{
	public static class Logger
	{
		/// <summary>
		/// Log a message via the ILogger interface
		/// </summary>
		/// <param name="message"></param>
		public static void Log( string message )
		{
			Reporter?.Log( message );
		}

		/// <summary>
		/// Report an event via the ILogger interface
		/// </summary>
		/// <param name="message"></param>
		public static void Event( string message )
		{
			Reporter?.Event( message );
		}

		/// <summary>
		/// Report an error via the ILogger interface
		/// </summary>
		/// <param name="message"></param>
		public static void Error( string message )
		{
			Reporter?.Error( message );
		}

		/// <summary>
		/// The object to log through
		/// </summary>
		public static ILogger Reporter { get; set; }

		/// <summary>
		/// Interface defining logging methods
		/// </summary>
		public interface ILogger
		{
			void Log( string message );
			void Event( string message );
			void Error( string message );
		}
	}
}