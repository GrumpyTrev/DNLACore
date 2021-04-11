using System;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The AlbumsDeletedMessage class is used to notify that one or more albums have been removed from the library
	/// </summary>
	class AlbumsDeletedMessage : BaseMessage
	{
		/// <summary>
		/// The removed albums album
		/// </summary>
		public List<int> DeletedAlbumIds { private get; set; } = null;
		
		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<List<int>> )( DeletedAlbumIds );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<List<int>> action ) => MessageRegistration.Register( action, typeof( AlbumsDeletedMessage ) );
	}
}