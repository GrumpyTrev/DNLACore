using SQLite;

namespace CoreMP
{
    public partial class Song
    {
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
