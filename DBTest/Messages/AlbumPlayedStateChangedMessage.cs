using System;

namespace DBTest
{
	/// <summary>
	/// The AlbumPlayedStateChangedMessage class is used to notify that the album's played state has changed
	/// </summary>
	class AlbumPlayedStateChangedMessage : BaseMessage
	{
		/// <summary>
		/// The changed album
		/// </summary>
		public Album AlbumChanged { private get; set; } = null;

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<Album> )( AlbumChanged );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<Album> action ) => MessageRegistration.Register( action, typeof( AlbumPlayedStateChangedMessage ) );
	}
}