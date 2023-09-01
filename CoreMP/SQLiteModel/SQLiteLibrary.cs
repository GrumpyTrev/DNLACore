using SQLite;

namespace CoreMP
{
#pragma warning disable CS0618 // Type or member is obsolete
	[Table( "Library" )]
	public class SQLiteLibrary : Library
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public override int Id { get; set; }
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
