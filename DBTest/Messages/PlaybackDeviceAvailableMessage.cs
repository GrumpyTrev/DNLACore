using System;

namespace DBTest
{
	/// <summary>
	/// The PlaybackDeviceAvailableMessage is used to report that the selected playback device is available
	/// </summary>
	class PlaybackDeviceAvailableMessage : BaseMessage
	{
		public PlaybackDevice SelectedDevice { private get; set; }

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rathre than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<PlaybackDevice> )( SelectedDevice );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<PlaybackDevice> action ) => MessageRegistration.Register( action, typeof( PlaybackDeviceAvailableMessage ) );
	}
}