using System;
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					DbAccess.UpdateAsync( this );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				}
			}
		}
		private string path = "";
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
