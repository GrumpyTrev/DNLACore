using System;

namespace DBTest
{
	/// <summary>
	/// The SongStartedMessage class is used to notify that a song is being played
	/// </summary>
	class SongStartedMessage: BaseMessage
	{
		/// <summary>
		/// The song being played
		/// </summary>
		public Song SongPlayed { private get; set; } = null;

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<Song> )( SongPlayed );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<Song> action ) => MessageRegistration.Register( action, typeof( SongStartedMessage ) );
	}
}