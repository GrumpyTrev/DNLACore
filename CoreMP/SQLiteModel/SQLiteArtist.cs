using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The Artist class represents a named artist and associated albums
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	[Table( "Artist" )]
	public class SQLiteArtist : Artist
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public override int Id { get; set; }

		[Ignore]
		public override List<ArtistAlbum> ArtistAlbums { get; set; } = new List<ArtistAlbum>();
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
