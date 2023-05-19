using SQLite;

namespace CoreMP
{
	[Table( "AlbumPlayListItem" )]
	public class AlbumPlaylistItem : PlaylistItem
	{
		public int AlbumId { get; set; }

		[Ignore]
		public Album Album { get; set; }
	}
}
