using SQLite;

namespace CoreMP
{
#pragma warning disable CS0618 // Type or member is obsolete
	[Table( "Song" )]
	public class SQLiteSong : Song
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public override int Id { get; set; }

		[Ignore]
		public override Artist Artist { get; set; } = null;

		/// <summary>
		/// The path associated with the song
		/// </summary>
		public override string Path
		{
			get => path;
			set
			{
				path = value;

				if ( StorageController.Loading == false )
				{
					_ = DbAccess.UpdateAsync( this );
				}
			}
		}
		private string path = "";
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
