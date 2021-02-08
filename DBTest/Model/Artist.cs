using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Artist class represents a named artist and associated albums
	/// </summary>
	public partial class Artist
	{
		/// <summary>
		/// Get the contents for the Artist
		/// The ArtistAlbum entries have already been obtained so just get the Songs for them
		/// </summary>
		/// <param name="theArtist"></param>
		public async Task GetSongsAsync()
		{
			if ( songsRead == false )
			{
				foreach ( ArtistAlbum artistAlbum in ArtistAlbums )
				{
					artistAlbum.Songs = await DbAccess.GetArtistAlbumSongsAsync( artistAlbum.Id );
					artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
				}
			}

			songsRead = true;
		}

		[Ignore]
		public List<ArtistAlbum> ArtistAlbums { get; set; } = new List<ArtistAlbum>();

		/// <summary>
		/// Indicates when all the details for the Artist have been read
		/// </summary>
		private bool songsRead = false;
	}
}