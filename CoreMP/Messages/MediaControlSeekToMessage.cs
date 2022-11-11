using System;

namespace CoreMP
{
	/// <summary>
	/// The MediaControlSeekToMessage class is used to notify that the Media Control seek button has been pressed
	/// </summary>
	internal class MediaControlSeekToMessage : BaseMessage
	{
		/// <summary>
		/// The position to seek to.
		/// </summary>
		public int Position { private get; set; }

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<int> )( Position );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<int> action ) => MessageRegistration.Register( action, typeof( MediaControlSeekToMessage ) );
	}
}
