using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The GroupedSelection class is used to present a set of selected media objects in groups of related object types
	/// </summary>
	public class GroupedSelection
	{
		/// <summary>
		/// Categorise the specified selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		public GroupedSelection( IEnumerable<object> selectedObjects )
		{
			// Save the unprocessed objects.
			SelectedObjects = selectedObjects;

			// Group the objects into sets of Song, PlaylistItem, Playlist, Artist, ArtistAlbum and Album items
			Songs = selectedObjects.OfType<Song>();
			SongsCount = Songs.Count();
			PlaylistItems = selectedObjects.OfType<PlaylistItem>();
			PlaylistItemsCount = PlaylistItems.Count();
			Playlists = selectedObjects.OfType<Playlist>();
			PlaylistsCount = Playlists.Count();
			Artists = selectedObjects.OfType<Artist>();
			ArtistsCount = Artists.Count();
			ArtistAlbums = selectedObjects.OfType<ArtistAlbum>();
			ArtistAlbumsCount = ArtistAlbums.Count();
			Albums = selectedObjects.OfType<Album>();
			AlbumsCount = Albums.Count();

			// Determine if there is a parent playlist
			ParentPlaylist = ( PlaylistItemsCount == 0 ) ? null : DBTest.Playlists.GetPlaylist( PlaylistItems.First().PlaylistId );
		}

		/// <summary>
		/// The set of songs in the selected objects
		/// </summary>
		public IEnumerable<Song> Songs { get; private set; } = null;

		/// <summary>
		/// The number of songs in the selected objects
		/// </summary>
		public int SongsCount { get; private set; } = 0;

		/// <summary>
		/// The set of playlistitems in the selected objects
		/// </summary>
		public IEnumerable<PlaylistItem> PlaylistItems { get; private set; } = null;

		/// <summary>
		/// The number of playlistitems in the selected objects
		/// </summary>
		public int PlaylistItemsCount { get; private set; } = 0;

		/// <summary>
		/// The set of playlists in the selected objects
		/// </summary>
		public IEnumerable<Playlist> Playlists { get; private set; } = null;

		/// <summary>
		/// The number of playlists in the selected objects
		/// </summary>
		public int PlaylistsCount { get; private set; } = 0;

		/// <summary>
		/// The set of artists in the selected objects
		/// </summary>
		public IEnumerable<Artist> Artists { get; private set; } = null;

		/// <summary>
		/// The number of artists in the selected objects
		/// </summary>
		public int ArtistsCount { get; private set; } = 0;

		/// <summary>
		/// The set of artistalbums in the selected objects
		/// </summary>
		public IEnumerable<ArtistAlbum> ArtistAlbums { get; private set; } = null;

		/// <summary>
		/// The number of artistalbums in the selected objects
		/// </summary>
		public int ArtistAlbumsCount { get; private set; } = 0;

		/// <summary>
		/// The set of albums in the selected objects
		/// </summary>
		public IEnumerable<Album> Albums { get; private set; } = null;

		/// <summary>
		/// The number of albums in the selected objects
		/// </summary>
		public int AlbumsCount { get; private set; } = 0;

		/// <summary>
		/// The pareent playlist of selceted playlistitems, if there is one
		/// </summary>
		public Playlist ParentPlaylist { get; private set; } = null;

		/// <summary>
		/// The unprocessed set of selected objects
		/// </summary>
		public IEnumerable<object> SelectedObjects { get; private set; } = null;
	}
}