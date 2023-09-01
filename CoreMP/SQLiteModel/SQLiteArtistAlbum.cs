using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
#pragma warning disable CS0618 // Type or member is obsolete
	[Table( "ArtistAlbum" )]
	public class SQLiteArtistAlbum : ArtistAlbum
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public override int Id { get; set; }

		[Ignore]
		public override Album Album { get; set; }

		[Ignore]
		public override List<Song> Songs { get; set; }

		[Ignore]
		public override Artist Artist { get; set; }
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
