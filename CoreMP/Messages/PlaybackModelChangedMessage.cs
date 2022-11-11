using System;

namespace CoreMP
{
	/// <summary>
	/// The PlaybackModelChangedMessage class is used to notify that the set of available Playback Devices has changed
	/// </summary>
	public class PlaybackModelChangedMessage : BaseMessage
	{
		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action action ) => MessageRegistration.Register( action, typeof( PlaybackModelChangedMessage ) );
	}
}
