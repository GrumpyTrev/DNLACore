using System;

namespace DBTest
{
	/// <summary>
	/// The PlaylistUpdatedMessage class is used to notify that a playlist has changed in some way
	/// </summary>
	class PlaylistUpdatedMessage : BaseMessage
	{
		/// <summary>
		/// The song being played
		/// </summary>
		public Playlist UpdatedPlaylist { private get; set; } = null;

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<Playlist> )( UpdatedPlaylist );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<Playlist> action ) => MessageRegistration.Register( action, typeof( PlaylistUpdatedMessage ) );
	}
}