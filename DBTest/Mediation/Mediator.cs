using System;
using System.Collections.Generic;

namespace DBTest
{
	public static class Mediator
	{
		/// <summary>
		/// Registers interest in a specific message
		/// The registration will be added to the temporary collection that can be cleared when required
		/// </summary>
		/// <param name="callback">The callback to use when the message it received</param>
		/// <param name="message">The message to register</param>
		public static void Register( Action<Object> callback, Type message )
		{
			internalList.AddValue( message, callback );
		}

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
			internalList.RemoveValue( message, callback );
			permanentList.RemoveValue( message, callback );
		}

		/// <summary>
		/// Notify all consumers that have registered interest in the specific message
		/// </summary>
		/// <param name="message">The message by</param>
		public static void SendMessage( object message )
		{
			Type messageType = message.GetType();

			// Temporary registrations first
			if ( internalList.ContainsKey( messageType ) == true )
			{
				// Make a copy of the list of actions in case the callback modifies it
				List < Action<object> > internalListCopy = new List<Action<object>> ( internalList[ messageType ] );

				// Forward the message to all registered listeners
				foreach ( Action<object> callback in internalListCopy )
				{
					callback( message );
				}
			}

			// Now permanent
			if ( permanentList.ContainsKey( messageType ) == true )
			{
				// Make a copy of the list of actions in case the callback modifies it
				List<Action<object>> permanentListCopy = new List<Action<object>>( permanentList[ messageType ] );

				// Forward the message to all registered listeners
				foreach ( Action<object> callback in permanentListCopy )
				{
					callback( message );
				}
			}

		}

		/// <summary>
		/// Clear the temporary registrations collection
		/// </summary>
		public static void RemoveTemporaryRegistrations()
		{
			internalList.Clear();
		}

		/// <summary>
		/// Dictionary of message type to listeners - temporary collection
		/// </summary>
		private static MultiDictionary< Type , Action<Object>> internalList = new MultiDictionary<Type, Action<object>>();

		/// <summary>
		/// Dictionary of message type to listeners - permanent collection
		/// </summary>
		private static MultiDictionary< Type , Action<Object>> permanentList = new MultiDictionary<Type, Action<object>>();
	}
}
