using System;

namespace DBTest
{
	/// <summary>
	/// The MediaControlPauseMessage class is used to notify that the Media Control pause button has been pressed
	/// </summary>
	class MediaControlPauseMessage : BaseMessage
	{
		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action action ) => MessageRegistration.Register( action, typeof( MediaControlPauseMessage ) );
	}
}