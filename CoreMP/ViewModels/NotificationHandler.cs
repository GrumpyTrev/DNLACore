﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoreMP
{
	public static class NotificationHandler
	{
		/// <summary>
		/// Registers interest in notifications from the specified class with parameters
		/// </summary>
		/// <param name="callback">The callback to use when a notification is made</param>
		/// <param name="classType">The class to register</param>
		public static void Register( Type classType, NotificationDelegate callback, [CallerFilePath] string filePath = "" ) =>
			Register( classType, new DelegateContainer( callback ), filePath );

		/// <summary>
		/// Registers interest in notifications from the specified class with no parameters
		/// </summary>
		/// <param name="callback">The callback to use when a notification is made</param>
		/// <param name="classType">The class to register</param>
		public static void Register( Type classType, NotificationDelegateNoParams callback, [CallerFilePath] string filePath = "" ) =>
			Register( classType, new DelegateContainer( callback ), filePath );

		/// <summary>
		/// Registers interest in notifications from the specified class with parameters
		/// </summary>
		/// <param name="callback">The callback to use when a notification is made</param>
		/// <param name="classType">The class to register</param>
		public static void Register( Type classType, string propertyName, NotificationDelegate callback, [CallerFilePath] string filePath = "" ) =>
			Register( classType, new DelegateContainer( callback, propertyName ), filePath );

		/// <summary>
		/// Registers interest in notifications from the specified class with no parameters
		/// </summary>
		/// <param name="callback">The callback to use when a notification is made</param>
		/// <param name="classType">The class to register</param>
		public static void Register( Type classType, string propertyName, NotificationDelegateNoParams callback, [CallerFilePath] string filePath = "" ) =>
			Register( classType, new DelegateContainer( callback, propertyName ), filePath );

		/// <summary>
		/// Deregister all notifications for the calling class
		/// </summary>
		/// <param name="filePath"></param>
		public static void Deregister( [CallerFilePath] string filePath = "" )
		{
			// Get the file name (class name ) from the filePath
			string callerClassName = GetFileNameWithoutExtension( filePath );

			// Remove the registrations recorded against this class
			foreach( Tuple<string, DelegateContainer> registration in whoMadeRegistration[ callerClassName ] )
			{
				registrations.RemoveValue( registration.Item1, registration.Item2 );
			}

			// Remove the record
			whoMadeRegistration.Remove( callerClassName );
		}

		/// <summary>
		/// Called when a monitorable property has been changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="filePath"></param>
		/// <param name="propertyName"></param>
		public static void NotifyPropertyChanged( object sender, [CallerFilePath] string filePath = "", [CallerMemberName] string propertyName = "" )
		{
			// Get the file name (class name ) from the filePath
			string callerClassName = GetFileNameWithoutExtension( filePath );

			if ( registrations.ContainsKey( callerClassName ) == true )
			{
				// Make a copy of the list of delegates in case the callback modifies it
				List<DelegateContainer> messageRegistrations = new List<DelegateContainer>( registrations[ callerClassName ] );

				CoreMPApp.Post( () =>
				{
					// Forward the message to all registered listeners
					foreach ( DelegateContainer callback in messageRegistrations )
					{
						if ( ( callback.PropertyName == null ) || ( callback.PropertyName == propertyName ) )
						{
							callback.Invoke( sender, propertyName );
						}
					}
				} );
			}
		}

		/// <summary>
		/// Called when the value of a monitorable property has been changed
		/// Save the details of this change so that it can be reported when it is first registered
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="filePath"></param>
		/// <param name="propertyName"></param>
		public static void NotifyPropertyChangedPersistent( object sender, [CallerFilePath] string filePath = "", [CallerMemberName] string propertyName = "" )
		{
			NotifyPropertyChanged( sender, filePath, propertyName );
			savedNotifications[ GetFileNameWithoutExtension( filePath ) ] = new Tuple<object, string>( sender, propertyName );
		}

		/// <summary>
		/// The delegate type used to report back property change notifications
		/// </summary>
		/// <param name="message"></param>
		public delegate void NotificationDelegate( object sender, string message );

		/// <summary>
		/// The delegate type used to report back property change notifications with no parameters
		/// </summary>
		public delegate void NotificationDelegateNoParams();

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
		/// Registers interest in notifications from the specified class
		/// </summary>
		/// <param name="callback">The callback to use when a notification is made</param>
		/// <param name="classType">The class to register</param>
		private static void Register( Type classType, DelegateContainer container, string filePath )
		{
			// Get the file name (class name ) from the filePath
			string callerClassName = GetFileNameWithoutExtension( filePath );

			registrations.AddValue( classType.Name, container );

			// Keep a record of registrations made by this class so they can be removed
			whoMadeRegistration.AddValue( callerClassName, new Tuple<string, DelegateContainer>( classType.Name, container ) );

			// If a notification for the class have already been stored then report it now
			if ( savedNotifications.ContainsKey( classType.Name ) == true )
			{
				Tuple<object, string> notification = savedNotifications[ classType.Name ];

				if ( ( container.PropertyName == null ) || ( container.PropertyName == notification.Item2 ) )
				{
					CoreMPApp.Post( () => container.Invoke( notification.Item1, notification.Item2 ) );
				}
			}
		}

		/// <summary>
		/// Dictionary of message type to listeners
		/// </summary>
		private static readonly MultiDictionary<string, DelegateContainer> registrations = new MultiDictionary<string, DelegateContainer>();

		/// <summary>
		/// Dictionary of class making registration to "monitored class"/"delegate" tuple. Used to remove registrations
		/// </summary>
		private static readonly MultiDictionary<string, Tuple<string, DelegateContainer>> whoMadeRegistration =
			new MultiDictionary<string, Tuple<string, DelegateContainer>>();

		private static readonly Dictionary<string, Tuple<object, string>> savedNotifications = new Dictionary<string, Tuple<object, string>>();

		/// <summary>
		/// The DelegateContainer class is used to contain delegate with differnt signatures
		/// </summary>
		private class DelegateContainer
		{
			public DelegateContainer( NotificationDelegate notification, string property = null )
			{
				notificationDelegate = notification;
				PropertyName = property;
			}

			public DelegateContainer( NotificationDelegateNoParams notification, string property = null )
			{
				notificationDelegateNoParams = notification;
				PropertyName = property;
			}

			public void Invoke( object sender, string message )
			{
				if ( notificationDelegate != null )
				{
					notificationDelegate.Invoke( sender, message );
				}
				else
				{
					notificationDelegateNoParams.Invoke();
				}
			}

			public string PropertyName { get; private set; } = null;

			private readonly NotificationDelegate notificationDelegate = null;
			private readonly NotificationDelegateNoParams notificationDelegateNoParams = null;
		}
	}
}
