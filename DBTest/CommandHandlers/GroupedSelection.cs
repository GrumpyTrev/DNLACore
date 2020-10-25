using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The GroupedSelection class is used to present a set of selected media objects in groups of related object types
	/// </summary>
	class GroupedSelection
	{
		/// <summary>
		/// Categorise the specified selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		public GroupedSelection( IEnumerable<object> selectedObjects )
		{
			SelectedObjects = selectedObjects;
			Songs = selectedObjects.OfType<Song>();
			PlaylistItems = selectedObjects.OfType<PlaylistItem>();
			Playlists = selectedObjects.OfType<Playlist>();
			Artists = selectedObjects.OfType<Artist>();
			ArtistAlbums = selectedObjects.OfType<ArtistAlbum>();
			Albums = selectedObjects.OfType<Album>();
		}

		/// <summary>
		/// The set of songs in the selected objects
		/// </summary>
		public IEnumerable<Song> Songs { get; private set; } = null;

		/// <summary>
		/// The set of playlistitems in the selected objects
		/// </summary>
		public IEnumerable<PlaylistItem> PlaylistItems { get; private set; } = null;

		/// <summary>
		/// The set of playlists in the selected objects
		/// </summary>
		public IEnumerable<Playlist> Playlists { get; private set; } = null;

		/// <summary>
		/// The set of artists in the selected objects
		/// </summary>
		public IEnumerable<Artist> Artists { get; private set; } = null;

		/// <summary>
		/// The set of artistalbums in the selected objects
		/// </summary>
		public IEnumerable<ArtistAlbum> ArtistAlbums { get; private set; } = null;

		/// <summary>
		/// The set of albums in the selected objects
		/// </summary>
		public IEnumerable<Album> Albums { get; private set; } = null;

		/// <summary>
		/// The unprocessed set of selected objects
		/// </summary>
		public IEnumerable<object> SelectedObjects { get; private set; } = null;
	}
}