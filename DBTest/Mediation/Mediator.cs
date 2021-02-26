using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;

namespace DBTest
{
	public static class Mediator
	{
		/// <summary>
		/// Registers interest in a specific message
		/// The registration will be added to the permanent collection that is retained for the entire life of
		/// the containing process
		/// </summary>
		/// <param name="callback">The callback to use when the message it received</param>
		/// <param name="message">The message to register</param>
		public static void RegisterPermanent( Action<Object> callback, Type message )
		{
			permanentList.AddValue( message, callback );
		}

		/// <summary>
		/// Remove the specified registration from both the temporary and permanent lists
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="message"></param>
		public static void Deregister( Action<Object> callback, Type message )
		{
			permanentList.RemoveValue( message, callback );
		}

		/// <summary>
		/// Notify all consumers that have registered interest in the specific message
		/// </summary>
		/// <param name="message">The message by</param>
		public static void SendMessage( object message )
		{
			Type messageType = message.GetType();

			if ( permanentList.ContainsKey( messageType ) == true )
			{
				// Make a copy of the list of actions in case the callback modifies it
				List<Action<object>> permanentListCopy = new List<Action<object>>( permanentList[ messageType ] );

				UiSwitchingHandler.Post( () =>
				{
					// Forward the message to all registered listeners
					foreach ( Action<object> callback in permanentListCopy )
					{
						callback( message );
					}
				} );
			}
		}

		/// <summary>
		/// Context to use for switching to the UI thread
		/// </summary>
		private static Handler UiSwitchingHandler { get; } = new Handler( Looper.MainLooper );

		/// <summary>
		/// Dictionary of message type to listeners - permanent collection
		/// </summary>
		private static MultiDictionary< Type , Action<Object>> permanentList = new MultiDictionary<Type, Action<object>>();
	}
}
