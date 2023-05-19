using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
	[Table( "ArtistAlbum" )]
	public class ArtistAlbum
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		public int AlbumId { get; set; }

		[Ignore]
		public Album Album { get; set; }

		public int ArtistId { get; set; }

		[Ignore]
		public List<Song> Songs { get; set; }

		/// <summary>
		/// This entry is not in the database but needs to be accessed when an ArtistAlbum is selected
		/// </summary>
		[Ignore]
		public Artist Artist { get; set; }
	}
}
