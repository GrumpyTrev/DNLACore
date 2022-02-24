using Android.OS;
using System;
using System.Collections.Generic;

namespace DBTest
{
	public static class MessageRegistration
	{
		/// <summary>
		/// Registers interest in a specific message
		/// </summary>
		/// <param name="callback">The callback to use when the message it received</param>
		/// <param name="message">The message to register</param>
		public static void Register( Delegate callback, Type message ) => registrations.AddValue( message, callback );

		/// <summary>
		/// Notify all consumers that have registered interest in the specific message
		/// </summary>
		/// <param name="message">The message by</param>
		public static void SendMessage( BaseMessage message )
		{
			Type messageType = message.GetType();

			if ( registrations.ContainsKey( messageType ) == true )
			{
				// Make a copy of the list of delegates in case the callback modifies it
				List<Delegate> messageRegistrations = new( registrations[ messageType ] );

				UiSwitchingHandler.Post( () =>
				{
					// Forward the message to all registered listeners
					foreach ( Delegate callback in messageRegistrations )
					{
						message.Dispatch( callback );
					}
				} );
			}
		}

		/// <summary>
		/// Context to use for switching to the UI thread
		/// </summary>
		private static Handler UiSwitchingHandler { get; } = new Handler( Looper.MainLooper );

		/// <summary>
		/// Dictionary of message type to listeners
		/// </summary>
		private static readonly MultiDictionary< Type , Delegate> registrations = new();
	}
}
