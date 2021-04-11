using System;

namespace DBTest
{
	/// <summary>
	/// The MediaControlPlayPreviousMessage class is used to notify that the Media Control play previous button has been pressed
	/// </summary>
	class MediaControlPlayPreviousMessage : BaseMessage
	{
		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action action ) => MessageRegistration.Register( action, typeof( MediaControlPlayPreviousMessage ) );
	}
}