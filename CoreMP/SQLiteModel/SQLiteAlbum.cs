using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The SQLiteAlbum class is derived from Album in order to provide SQLLite specific attributes and persistence
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	[Table( "Album" )]
	public class SQLiteAlbum : Album
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public override int Id { get; set; }

		/// <summary>
		/// Persist any changes to the Played property
		/// </summary>
		public override bool Played
		{
			get => base.Played;
			internal set
			{
				base.Played = value;

				if ( StorageController.Loading == false )
				{
					_ = DbAccess.UpdateAsync( this );
				}
			}
		}
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
