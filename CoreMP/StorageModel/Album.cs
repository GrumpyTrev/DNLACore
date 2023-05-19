using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The Album class contains a named set of songs associated with one or more artists
	/// </summary>
	[Table( "Album" )]
	public class Album
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		public int LibraryId { get; set; }

		public string ArtistName { get; set; }

		[Column( "Played" )]
		public bool DBPlayed { get; set; } = false;

		public int Year { get; set; } = 0;

		/// <summary>
		/// The rating is from 0 (bad) to 4 (bril)
		/// </summary>
		public int Rating { get; set; } = 2;

		/// <summary>
		/// The full genre string that could include several genres is included in the database
		/// </summary>
		public string Genre { get; set; } = "";
		/// <summary>
		/// The Album's Played flag
		/// </summary>
		[Ignore]
		public bool Played
		{
			get => DBPlayed;
			set
			{
				DBPlayed = value;

				// No need to wait for the storage to complete
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				DbAccess.UpdateAsync( this );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

				// Report the change
				NotificationHandler.NotifyPropertyChanged( this );
//				new AlbumPlayedStateChangedMessage() { AlbumChanged = this }.Send();
			}
		}

		private List<Song> songs = null;

		[Ignore]
		public List<Song> Songs
		{
			get
			{
				if ( songs == null )
				{
					songs = CoreMP.Songs.GetAlbumSongs( Id );

					// Sort the songs by track number
					songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
				}

				return songs;
			}
		}
	}
}
