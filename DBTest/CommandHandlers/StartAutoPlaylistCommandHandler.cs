﻿using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The StartAutoPlaylistCommandHandler class is used to process a Play or Queue autoplay command
	/// </summary>
	class StartAutoPlaylistCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			List<string> selectedGenres = new List<string>();

			// If an Artist has been selected then the starting point for generation will be the albums associated with the Artist.
			// If an Album has been selected then that album will be the starting point.
			// If a Song has been selected then that song will be the starting point.
			if ( selectedObjects.ArtistsCount > 0 )
			{
				// Get all the genres associated with all the selected artists
				foreach ( Artist selectedArtist in selectedObjects.Artists )
				{
					foreach ( ArtistAlbum artistAlbum in selectedArtist.ArtistAlbums )
					{
						selectedGenres.AddRange( artistAlbum.Album.Genre.Split( ';' ).ToList() );
					}
				}
			}
			else if ( selectedObjects.ArtistAlbumsCount > 0 )
			{
				// Get all the genres associated with the albums from all the selected artistalbums
				foreach ( ArtistAlbum selectedArtistAlbum in selectedObjects.ArtistAlbums )
				{
					selectedGenres.AddRange( selectedArtistAlbum.Album.Genre.Split( ';' ).ToList() );
				}
			}
			else if ( selectedObjects.AlbumsCount > 0 )
			{
				// Get all the genres associated with the albums from all the selected albums
				foreach ( Album selectedAlbum in selectedObjects.Albums )
				{
					selectedGenres.AddRange( selectedAlbum.Genre.Split( ';' ).ToList() );
				}
			}
			else if ( selectedObjects.SongsCount > 0 )
			{
				foreach ( Song selectedSong in selectedObjects.Songs )
				{
					selectedGenres.AddRange( ArtistAlbums.GetArtistAlbumById( selectedSong.ArtistAlbumId ).Album.Genre.Split( ';' ).ToList() );
				}
			}

			// Make genre list unique
			selectedGenres = selectedGenres.Distinct().ToList();

			AutoplayController.StartAutoplayAsync( selectedObjects.Songs, selectedGenres, commandIdentity == Resource.Id.auto_play );

			commandCallback.PerformAction();
		}
		
		/// <summary>
		/// Bind this command to the router. An override is required here as this command can be launched using two identities
		/// </summary>
		public override void BindToRouter()
		{
			CommandRouter.BindHandler( CommandIdentity, this );
			CommandRouter.BindHandler( Resource.Id.auto_queue, this );
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.auto_play;
	}
}