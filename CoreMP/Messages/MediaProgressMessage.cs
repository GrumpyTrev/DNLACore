using System;

namespace CoreMP
{
	/// <summary>
	/// The MediaProgressMessage class is used to notify the progress of the media being played
	/// </summary>
	public class MediaProgressMessage : BaseMessage
	{
		/// <summary>
		/// The playback position
		/// </summary>
		public int CurrentPosition { private get; set; }

		/// <summary>
		/// The reported duration of the song
		/// </summary>
		public int Duration { private get; set; }

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rathre than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<int, int> )( CurrentPosition, Duration );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<int, int> action ) => MessageRegistration.Register( action, typeof( MediaProgressMessage ) );
	}
}
