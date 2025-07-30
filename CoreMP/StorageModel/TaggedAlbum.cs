using SQLite;

namespace CoreMP
{
	[Table( "TaggedAlbum" )]
	public class TaggedAlbum
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public int TagIndex { get; set; }

		public int AlbumId { get; set; }

		[Ignore]
		public Album Album { get; set; }

		public int TagId { get; set; }

		public override bool Equals( object obj ) => ( obj != null ) && ( ( ( TaggedAlbum )obj ).AlbumId == AlbumId );

		public override int GetHashCode() => AlbumId.GetHashCode();
	}
}
