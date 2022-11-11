using System;

namespace CoreMP
{
	/// <summary>
	/// The SongFinishedMessage class is used to notify that a song has finished being played
	/// </summary>
	public class SongFinishedMessage : BaseMessage
	{
		/// <summary>
		/// The song that has just finished being played
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
		public static void Register( Action<Song> action ) => MessageRegistration.Register( action, typeof( SongFinishedMessage ) );
	}
}
