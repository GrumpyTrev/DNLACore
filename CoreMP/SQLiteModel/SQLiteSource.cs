using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The Source class specifies where a set of somngs can be found on a local or remote device
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	[Table( "Source" )]
	public class SQLiteSource : Source
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public override int Id { get; set; }

		[Ignore]
		public override string ScanSource { get; set; }

		[Ignore]
		public override string LocalAccess { get; set; }

		[Ignore]
		public override string RemoteAccess { get; set; }

		[Ignore]
		public override List<Song> Songs { get; set; }

		/// <summary>
		/// Update the source and save it to storaage
		/// </summary>
		/// <param name="newSource"></param>
		public override void UpdateSource( Source newSource )
		{
			base.UpdateSource( newSource );

			// No need to wait for this to be written to storage
			_ = DbAccess.UpdateAsync( this );
		}
	}
#pragma warning restore CS0618 // Type or member is obsolete
}
