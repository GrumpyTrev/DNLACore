using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	class StartAutoPlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			Song selectedSong = null;
			List<string> selectedGenres = new List<string>();

			// If an Artist has been selected then the starting point for generation will be the albums associated with the Artist.
			// If an Album has been selected then that album will be the starting point.
			// If a Song has been selected then that song will be the starting point.
			if ( selectedObjects.ArtistsCount > 0 )
			{
				Artist selectedArtist = selectedObjects.Artists.First();

				// Get all the genres associated with this artist
				foreach ( ArtistAlbum artistAlbum in selectedArtist.ArtistAlbums )
				{
					selectedGenres.AddRange( artistAlbum.Album.Genre.Split( ';' ).ToList() );
				}

				// Make genre list unique
				selectedGenres = selectedGenres.Distinct().ToList();
			}
			else if ( selectedObjects.ArtistAlbumsCount > 0 )
			{
				// Use this albums genres
				selectedGenres.AddRange( selectedObjects.ArtistAlbums.First().Album.Genre.Split( ';' ).ToList() );

				// Make genre list unique
				selectedGenres = selectedGenres.Distinct().ToList();
			}
			else if ( selectedObjects.SongsCount > 0 )
			{
				selectedSong = selectedObjects.Songs.First();

				selectedGenres.AddRange( ArtistAlbums.GetArtistAlbumById( selectedSong.ArtistAlbumId ).Album.Genre.Split( ';' ).ToList() );

				// Make genre list unique
				selectedGenres = selectedGenres.Distinct().ToList();
			}

			AutoplayController.StartAutoplayAsync( selectedSong, selectedGenres );

			commandCallback.PerformAction();
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) =>
			( selectedObjects.ArtistsCount == 1 ) || ( selectedObjects.ArtistAlbumsCount == 1 ) || 
			( selectedObjects.SongsCount == 1 ) || ( selectedObjects.AlbumsCount == 1 );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.auto_gen;
	}
}