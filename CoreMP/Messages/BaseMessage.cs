using System;

namespace CoreMP
{
	/// <summary>
	/// All other messages are based on this message
	/// </summary>
	public class BaseMessage
	{
		/// <summary>
		/// Send this message to all registered receivers
		/// </summary>
		public void Send() => MessageRegistration.SendMessage( this );

		/// <summary>
		/// The base class Dispatch call the callback with no parameters
		/// </summary>
		/// <param name="callBack"></param>
		public virtual void Dispatch( Delegate callBack ) => ( callBack as Action)();
	}
}
