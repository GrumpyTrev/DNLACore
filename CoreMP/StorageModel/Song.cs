using System;
using SQLite;

namespace CoreMP
{
	[Table( "Song" )]
	public class Song
    {
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Title { get; set; }
		public int Track { get; set; }

		[Column( "Path" )]
		public string DBPath { get; set; }

		/// <summary>
		/// The path associated with the song
		/// </summary>
		[Ignore]
		public string Path
		{
			get => DBPath;
			set
			{
				DBPath = value;

				// No need to wait for the storage to complete
				DbAccess.UpdateAsync( this );
			}
		}

		public DateTime ModifiedTime { get; set; }
		public int Length { get; set; }

		public int AlbumId { get; set; }

		public int SourceId { get; set; }

		public int ArtistAlbumId { get; set; }

		[Ignore]
        public ScanActionType ScanAction { get; set; }

        /// <summary>
        /// This entry is not in the database but is set for songs that are being played
        /// </summary>
        [Ignore]
        public Artist Artist { get; set; } = null;

		/// <summary>
		/// The Album that this song is on.
		/// This value is only obtained on demand
		/// </summary>
		private Album album = null;
		[Ignore]
		public Album Album
		{
			get
			{
				if ( album == null )
				{
					album = Albums.GetAlbumById( AlbumId );
				}

				return album;
			}
		}

		public enum ScanActionType { NotMatched, Matched, Differ, New };
    }
}
