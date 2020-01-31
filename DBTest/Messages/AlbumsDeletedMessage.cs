using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The AlbumsDeletedMessage class is used to notify that one or more albums have been removed from the library
	/// </summary>
	class AlbumsDeletedMessage: BaseMessage
	{
		/// <summary>
		/// The removed albums album
		/// </summary>
		public List<int> DeletedAlbumIds { get; set; } = null;
	}
}