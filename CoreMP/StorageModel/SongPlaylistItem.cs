using SQLite;

namespace CoreMP
{
	[Table( "PlayListItem" )]
	public class SongPlaylistItem : PlaylistItem
	{
		public int SongId { get; set; }

		[Ignore]
		public Song Song { get; set; }

		/// <summary>
		/// This entry is not in the database but needs to be accessed via the Song's ArtistAlbum and its Artist id
		/// </summary>
		[Ignore]
		public Artist Artist { get; set; }
	}
}
