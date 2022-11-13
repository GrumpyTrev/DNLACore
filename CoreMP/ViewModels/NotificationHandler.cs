using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoreMP
{
	public static class NotificationHandler
	{
		/// <summary>
		/// Registers interest in notifications from the specified class
		/// </summary>
		/// <param name="callback">The callback to use when a notification is made</param>
		/// <param name="classType">The class to register</param>
		public static void Register( Type classType, NotificationDelegate callback, [CallerFilePath] string filePath = "" )
		{
			// Get the file name (class name ) from the filePath
			string callerClassName = GetFileNameWithoutExtension( filePath );

			registrations.AddValue( classType.Name, callback );

			// Keep a record of registrations made by this class so they can be removed
			whoMadeRegistration.AddValue( callerClassName, new Tuple<string, NotificationDelegate>( classType.Name, callback ) );
		}

		public static void Deregister( [CallerFilePath] string filePath = "" )
		{
			// Get the file name (class name ) from the filePath
			string callerClassName = GetFileNameWithoutExtension( filePath );

			// Remove the registrations recorded against this class
			foreach( Tuple<string, NotificationDelegate> registration in whoMadeRegistration[ callerClassName ] )
			{
				registrations.RemoveValue( registration.Item1, registration.Item2 );
			}

			// Remove the record
			whoMadeRegistration.Remove( callerClassName );
		}

		public static void NotifyPropertyChanged( object sender, [CallerFilePath] string filePath = "", [CallerMemberName] string propertyName = "" )
		{
			// Get the file name (class name ) from the filePath
			string callerClassName = GetFileNameWithoutExtension( filePath );

			if ( registrations.ContainsKey( callerClassName ) == true )
			{
				// Make a copy of the list of delegates in case the callback modifies it
				List<Delegate> messageRegistrations = new List<Delegate>( registrations[ callerClassName ] );

				CoreMPApp.Post( () =>
				{
					// Forward the message to all registered listeners
					foreach ( NotificationDelegate callback in messageRegistrations )
					{
						callback.Invoke( sender, propertyName );
					}
				} );
			}
		}

		/// <summary>
		/// The delegate type used to report back property change notifications
		/// </summary>
		/// <param name="message"></param>
		public delegate void NotificationDelegate( object sender, string message );

		/// <summary>
		/// Get the file name without leading directories and extenstion
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static string GetFileNameWithoutExtension( string path )
		{
			int index = path.LastIndexOf( '\\' );
			if ( index != -1 )
			{
				path = path.Substring( index + 1, path.Length - index - 1 );

				index = path.LastIndexOf( '.' );
				if ( index != -1 )
				{
					path = path.Substring( 0, index );
				}
			}

			return path;
		}

		/// <summary>
		/// Dictionary of message type to listeners
		/// </summary>
		private static readonly MultiDictionary<string, NotificationDelegate> registrations = new MultiDictionary<string, NotificationDelegate>();

		/// <summary>
		/// Dictionary of class making registration to "monitored class"/"delegate" tuple. Used to remove registrations
		/// </summary>
		private static readonly MultiDictionary<string, Tuple<string, NotificationDelegate>> whoMadeRegistration = 
			new MultiDictionary<string, Tuple<string, NotificationDelegate>>();
	}
}
