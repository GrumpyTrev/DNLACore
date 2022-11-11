using System;

namespace CoreMP
{
	/// <summary>
	/// The MediaPlayingMessage class is used to notify whether or not the media is being played
	/// </summary>
	public class MediaPlayingMessage : BaseMessage
	{
		/// <summary>
		/// The playback position
		/// </summary>
		public bool IsPlaying { private get; set; }

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<bool> )( IsPlaying );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<bool> action ) => MessageRegistration.Register( action, typeof( MediaPlayingMessage ) );
	}
}
