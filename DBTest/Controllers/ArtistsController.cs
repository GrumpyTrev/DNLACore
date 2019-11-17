using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The ArtistsController is the Controller for the ArtistsView. It responds to ArtistsView commands and maintains Artists data in the
	/// ArtistsViewModel
	/// </summary>
	static class ArtistsController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static ArtistsController()
		{
			Mediator.RegisterPermanent( PlaylistAddedOrDeleted, typeof( PlaylistDeletedMessage ) );
			Mediator.RegisterPermanent( PlaylistAddedOrDeleted, typeof( PlaylistAddedMessage ) );
		}

		/// <summary>
		/// Get the Artist data associated with the specified library
		/// If the data has already been obtained then notify view immediately.
		/// Otherwise get the data from the database asynchronously
		/// </summary>
		/// <param name="libraryId"></param>
		public static async void GetArtistsAsync( int libraryId )
		{
			// Check if the Artist details for the library have already been obtained
			if ( ( ArtistsViewModel.Artists == null ) || ( ArtistsViewModel.LibraryId != libraryId ) )
			{
				// New data is required
				ArtistsViewModel.LibraryId = libraryId;
				ArtistsViewModel.Artists = await ArtistAccess.GetArtistDetailsAsync( ArtistsViewModel.LibraryId );

				// Sort the list of artists by name
				ArtistsViewModel.Artists.Sort( ( a, b ) => {
					// Do a normal comparison, except remove a leading 'The ' before comparing
					string artistA = ( a.Name.ToUpper().StartsWith( "THE " ) == true ) ? a.Name.Substring( 4 ) : a.Name;
					string artistB = ( b.Name.ToUpper().StartsWith( "THE " ) == true ) ? b.Name.Substring( 4 ) : b.Name;

					return artistA.CompareTo( artistB );
				} );

				// Work out the section indexes for the sorted data
				ArtistsViewModel.AlphaIndex = new Dictionary<string, int>();
				int index = 0;
				foreach ( Artist artist in ArtistsViewModel.Artists )
				{
					string key = artist.Name[ 0 ].ToString();
					if ( ArtistsViewModel.AlphaIndex.ContainsKey( key ) == false )
					{
						ArtistsViewModel.AlphaIndex[ key ] = index;
					}
					index++;
				}

				// Get the list of current playlists
				List< Playlist > playlists = await PlaylistAccess.GetPlaylistDetailsAsync( PlaylistsViewModel.LibraryId );

				// Extract just the names as well
				ArtistsViewModel.PlaylistNames = playlists.Select( i => i.Name ).ToList();
			}

			// Publish the data
			Reporter?.ArtistsDataAvailable();
		}

		/// <summary>
		/// Get the contents for the specified Artist
		/// </summary>
		/// <param name="theArtist"></param>
		public static void GetArtistContents( Artist theArtist )
		{
			ArtistAccess.GetArtistContents( theArtist );

			// Sort the albums alphabetically
			theArtist.ArtistAlbums.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

			// Sort the songs by track number
			foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
			{
				artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
			}

			// Now all the ArtistAlbum and Song entries have been read form a single list from them
			theArtist.EnumerateContents();
		}

		/// <summary>
		/// Add a list of Songs to a specified playlist
		/// </summary>
		/// <param name="songsToAdd"></param>
		/// <param name="clearFirst"></param>
		public static void AddSongsToPlaylist( List<Song> songsToAdd, string playlistName )
		{
			// Carry out the common processing to add songs to a playlist
			PlaylistAccess.AddSongsToPlaylist( songsToAdd, playlistName, ArtistsViewModel.LibraryId );

			// Publish this event
			new PlaylistSongsAddedMessage() { PlaylistName = playlistName }.Send();
		}

		/// <summary>
		/// Called when a PlaylistDeletedMessage or PlaylistAddedMessage message has been received
		/// Update the list of playlists held by the model
		/// </summary>
		/// <param name="message"></param>
		private static async void PlaylistAddedOrDeleted( object message )
		{
			// Get the list of current playlists
			List<Playlist> playlists = await PlaylistAccess.GetPlaylistDetailsAsync( PlaylistsViewModel.LibraryId );

			ArtistsViewModel.PlaylistNames = playlists.Select( i => i.Name ).ToList();
		}

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public static IReporter Reporter { get; set; } = null;

		/// <summary>
		/// The interface used to report back controller results
		/// </summary>
		public interface IReporter
		{
			void ArtistsDataAvailable();
		}
	}
}