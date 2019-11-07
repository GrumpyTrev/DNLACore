using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Linq;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace DBTest
{
	public class ArtistsFragment: PagedFragment<Artist>, ExpandableListAdapter<Artist>.IGroupContentsProvider<Artist>, ArtistsController.IReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public ArtistsFragment()
		{
		}

		/// <summary>
		/// Called to create the UI components to display playlists
		/// </summary>
		/// <param name="inflater"></param>
		/// <param name="container"></param>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		protected override View OnSpecialisedCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			// Create the view
			View view = inflater.Inflate( Resource.Layout.artists_fragment, container, false );

			// Get the ExpandableListView and link to an ArtistsAdapter
			listView = view.FindViewById<ExpandableListView>( Resource.Id.libraryLayout );

			adapter = new ArtistsAdapter( Context, listView, this, this );
			base.Adapter = adapter;

			listView.SetAdapter( adapter );

			// Initialise the ArtistsController
			ArtistsController.Reporter = this;

			// Request the Artists data from the library - via a Post so that any response comes back after the UI has been created
			view.Post( () => {
				ArtistsController.GetArtistsAsync( ConnectionDetailsModel.LibraryId );
			} );

			return view;
		}

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			inflater.Inflate( Resource.Menu.menu_artists, menu );

			base.OnCreateOptionsMenu( menu, inflater );
		}

		/// <summary>
		/// Get all the ArtistAlbum entries associated with a specified Artist.
		/// </summary>
		/// <param name="theArtist"></param>
		public void ProvideGroupContents( Artist theArtist )
		{
			if ( theArtist.ArtistAlbums == null )
			{
				ArtistsController.GetArtistContents( theArtist );
			}
		}

		/// <summary>
		/// Called when a menu item on the Contextual Action Bar has been selected
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnActionItemClicked( ActionMode mode, IMenuItem item )
		{
			return false;
		}

		/// <summary>
		/// Called when the Controller has obtained the Artist data
		/// Pass it on to the adapter
		/// </summary>
		public void ArtistsDataAvailable()
		{
			adapter.SetData( ArtistsViewModel.Artists, ArtistsViewModel.AlphaIndex );

			if ( ArtistsViewModel.ListViewState != null )
			{
				listView.OnRestoreInstanceState( ArtistsViewModel.ListViewState );
				ArtistsViewModel.ListViewState = null;
			}
		}

		/// <summary>
		/// Called when a bottom toolbar button has been clicked
		/// </summary>
		/// <param name="button"></param>
		public void ToolbarButtonClicked( ImageButton button )
		{
			if ( button.Id == Resource.Id.add_songs_to_playlist )
			{
				// Create a Popup menu containing the play list names and show it
				PopupMenu playlistsMenu = new PopupMenu( Context, button );

				// TO DO This is a bit iffy as the PlaylistsViewModel.PlaylistNames may not have been populated yet
				// and this 'view' should not be accessing someone else's model data
				foreach ( string name in PlaylistsViewModel.PlaylistNames )
				{
					playlistsMenu.Menu.Add( 0, Menu.None, 0, name );
				}

				// When a menu item is clicked get the songs from the adapter and the playlist name from the selected item
				// and pass them both to the ArtistsController
				playlistsMenu.MenuItemClick += ( sender1, args1 ) => {

					List<Song> selectedSongs = adapter.GetSelectedItems().Cast<Song>().ToList();

					// Determine which Playlist has been selected and add the selected songs to the playlist
					ArtistsController.AddSongsToPlaylist( selectedSongs, args1.Item.TitleFormatted.ToString() );

					LeaveActionMode();
				};

				playlistsMenu.Show();
			}
			else
			{
				// Form a list of Songs from the selected objects
				List<Song> selectedSongs = adapter.GetSelectedItems().Cast<Song>().ToList();

				if ( button.Id == Resource.Id.action_add_queue )
				{
					// Get the sorted list of selected songs from the adapter and add them to the Now Playing playlist
					ArtistsController.AddSongsToNowPlayingList( selectedSongs, false );
					LeaveActionMode();
				}
				else if ( button.Id == Resource.Id.action_playnow )
				{
					// Get the sorted list of selected songs from the adapter and replace the Now Playing playlist with them
					ArtistsController.AddSongsToNowPlayingList( selectedSongs, true );
					LeaveActionMode();
				}
			}
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources()
		{
			// Remove this object from the controller
			ArtistsController.Reporter = null;

			// Save the scroll position 
			ArtistsViewModel.ListViewState = listView.OnSaveInstanceState();
		}

		/// <summary>
		/// Called to allow the specialised fragment to initialise the bottom toolbar
		/// </summary>
		/// <param name="bottomToolbar"></param>
		protected override void InitialiseBottomToolbar( Toolbar bottomToolbar )
		{
			new DefinedSourceImageButton( bottomToolbar, Resource.Id.add_songs_to_playlist, Resource.Drawable.add_to_playlist, ToolbarButtonClicked );
			new DefinedSourceImageButton( bottomToolbar, Resource.Id.action_add_queue, Resource.Drawable.add_to_queue, ToolbarButtonClicked );
			new DefinedSourceImageButton( bottomToolbar, Resource.Id.action_playnow, Resource.Drawable.play_now, ToolbarButtonClicked );
		}

		/// <summary>
		/// The ArtistsAdapter used to hold the Artist data and display it in the ExpandableListView
		/// </summary>
		private ArtistsAdapter adapter = null;

		/// <summary>
		/// The actual list view used to display the data
		/// </summary>
		private ExpandableListView listView = null;
	}
}