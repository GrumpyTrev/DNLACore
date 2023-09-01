using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The Album class contains a named set of songs associated with one or more artists
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	[Table( "Album" )]
	public class SQLiteAlbum : Album
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public override int Id { get; set; }

		public override bool Played
		{
			get => base.Played;
			set
			{
				base.Played = value;

				if ( StorageController.Loading == false )
				{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					DbAccess.UpdateAsync( this );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				}
			}
		}
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
