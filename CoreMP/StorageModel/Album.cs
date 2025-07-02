using System;
using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The Album class contains a named set of songs associated with one or more artists
	/// </summary>
	public class Album
	{
		[Obsolete( "Do not create model instances directly", false )]
		public Album() { }

		public virtual int Id { get; set; }

		public string Name { get; set; }

		public int LibraryId { get; set; }

		public string ArtistName { get; set; }

		public int Year { get; set; } = 0;

		/// <summary>
		/// The full genre string that could include several genres is included in the database
		/// </summary>
		public string Genre { get; set; } = "";

		/// <summary>
		/// The Album's Played flag
		/// </summary>
		public virtual bool Played
		{
			get => played;
			internal set
			{
				played = value;

				// Report the change
				if ( StorageController.Loading == false )
				{
					NotificationHandler.NotifyPropertyChanged( this );
				}
			}
		}
		private bool played = false;

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
		private List<Song> songs = null;
	}
}
