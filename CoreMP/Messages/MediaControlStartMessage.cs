using System;

namespace CoreMP
{
	/// <summary>
	/// The MediaControlStartMessage class is used to notify that the Media Control start button has been pressed
	/// </summary>
	internal class MediaControlStartMessage : BaseMessage
	{
		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action action ) => MessageRegistration.Register( action, typeof( MediaControlStartMessage ) );
	}
}
