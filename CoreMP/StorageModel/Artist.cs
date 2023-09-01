using System;
using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The Artist class represents a named artist and associated albums
	/// </summary>
	public class Artist
	{
		[Obsolete( "Do not create model instances directly", false )]
		public Artist() { }

		public virtual int Id { get; set; }

		public string Name { get; set; }

		public int LibraryId { get; set; }

		/// <summary>
		/// Get the contents for the Artist
		/// The ArtistAlbum entries have already been obtained so just get the Songs for them
		/// </summary>
		/// <param name="theArtist"></param>
		public void GetSongs()
		{
			if ( songsRead == false )
			{
				foreach ( ArtistAlbum artistAlbum in ArtistAlbums )
				{
					artistAlbum.Songs = Songs.GetArtistAlbumSongs( artistAlbum.Id );
					artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
				}
			}

			songsRead = true;
		}

		public static void GetArtistAlbumSongs( ArtistAlbum artistAlbum )
		{
			if ( artistAlbum.Songs == null )
			{
				artistAlbum.Songs = Songs.GetArtistAlbumSongs( artistAlbum.Id );
				artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}
		}

		public virtual List<ArtistAlbum> ArtistAlbums { get; set; } = new List<ArtistAlbum>();

		/// <summary>
		/// Indicates when all the details for the Artist have been read
		/// </summary>
		private bool songsRead = false;
	}
}
