using System;

namespace CoreMP
{
	/// <summary>
	/// The PlaySongMessage class is used to notify that the selected song should be played
	/// </summary>
	internal class PlaySongMessage : BaseMessage
	{
		/// <summary>
		/// The song to play
		/// </summary>
		public Song SongToPlay { private get; set; } = null;

		/// <summary>
		/// Allow this message to be used to just set the song, without actually playing it
		/// </summary>
		public bool DontPlay { private get; set; } = false;

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<Song, bool> )( SongToPlay, DontPlay );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<Song, bool> action ) => MessageRegistration.Register( action, typeof( PlaySongMessage ) );
	}
}
