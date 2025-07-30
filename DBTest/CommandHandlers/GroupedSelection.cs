using System.Collections.Generic;
using CoreMP;

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

			// Group the objects into sets of Song, PlaylistItem, IPlaylist, Artist, ArtistAlbum, Album and Genre (string) items
			foreach ( object selectedObject in selectedObjects )
			{
				if ( selectedObject is Song song )
				{
					Songs.Add( song );
				}
				else if ( selectedObject is PlaylistItem playlistItem )
				{
					PlaylistItems.Add( playlistItem );
				}
				else if ( selectedObject is Playlist playlist )
				{
					Playlists.Add( playlist );
				}
				else if ( selectedObject is Artist artist )
				{
					Artists.Add( artist );
				}
				else if ( selectedObject is ArtistAlbum artistAlbum )
				{
					ArtistAlbums.Add( artistAlbum );
				}
				else if ( selectedObject is Album album )
				{
					Albums.Add( album );
				}
				else if ( selectedObject is string str )
				{
					Genres.Add( str );
				}
			}

			// Determine if there is a parent playlist
			if ( PlaylistItems.Count > 0 )
			{
				ParentPlaylist = PlaylistItems[ 0 ].GetParentPlaylist();
			}
		}

		/// <summary>
		/// The set of songs in the selected objects
		/// </summary>
		public List<Song> Songs { get; } = [];

		/// <summary>
		/// The set of playlistitems in the selected objects
		/// </summary>
		public List<PlaylistItem> PlaylistItems { get; } = [];

		/// <summary>
		/// The set of playlists in the selected objects
		/// </summary>
		public List<Playlist> Playlists { get; } = [];

		/// <summary>
		/// The set of artists in the selected objects
		/// </summary>
		public List<Artist> Artists { get; } = [];

		/// <summary>
		/// The set of artistalbums in the selected objects
		/// </summary>
		public List<ArtistAlbum> ArtistAlbums { get; } = [];

		/// <summary>
		/// The set of albums in the selected objects
		/// </summary>
		public List<Album> Albums { get; } = [];

		/// <summary>
		/// The set of genres in the selected objects
		/// </summary>
		public List<string> Genres { get; } = [];

		/// <summary>
		/// The parent playlist of selected playlistitems, if there is one
		/// </summary>
		public Playlist ParentPlaylist { get; private set; } = null;

		/// <summary>
		/// The unprocessed set of selected objects
		/// </summary>
		public IEnumerable<object> SelectedObjects { get; private set; } = null;
	}
}
