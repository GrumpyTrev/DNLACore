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
					Logger.Log( string.Format( "GetSongsAsync: Getting Songs for {0}", artistAlbum.Name ) );
					artistAlbum.Songs = await DbAccess.GetArtistAlbumSongsAsync( artistAlbum.Id );
					Logger.Log( string.Format( "GetSongsAsync: Got them, now sorting" ) );
					artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
					Logger.Log( string.Format( "GetSongsAsync: Sorted" ) );
				}
			}

			songsRead = true;
		}

		public async Task GetArtistAlbumSongs( ArtistAlbum artistAlbum )
		{
			if ( artistAlbum.Songs == null )
			{
				Logger.Log( string.Format( "GetArtistAlbumSongs: Getting Songs for {0}", artistAlbum.Name ) );
				artistAlbum.Songs = await DbAccess.GetArtistAlbumSongsAsync( artistAlbum.Id );
				Logger.Log( string.Format( "GetArtistAlbumSongs: Got them, now sorting" ) );
				artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
				Logger.Log( string.Format( "GetArtistAlbumSongs: Sorted" ) );
			}
		}

			
			[Ignore]
		public List<ArtistAlbum> ArtistAlbums { get; set; } = new List<ArtistAlbum>();

		/// <summary>
		/// Indicates when all the details for the Artist have been read
		/// </summary>
		private bool songsRead = false;
	}
}