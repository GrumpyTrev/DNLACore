using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	public class PlaylistsFragment: PagedFragment<Playlist>, ExpandableListAdapter< Playlist >.IGroupContentsProvider< Playlist >, 
		PlaylistsController.IReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public PlaylistsFragment()
		{
		}

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			inflater.Inflate( Resource.Menu.menu_playlists, menu );

			base.OnCreateOptionsMenu( menu, inflater );
		}

		/// <summary>
		/// Called when a menu item has been selected
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			bool handled;

			// Check for a new playlist request
			if ( item.ItemId == Resource.Id.new_playlist )
			{
				NewPlaylistNameDialogFragment.ShowFragment( Activity.SupportFragmentManager );
			
				handled = true;
			}
			else
			{
				handled = base.OnOptionsItemSelected( item );
			}

			return handled;
		}

		/// <summary>
		/// Get all the PlaylistItem entries associated with a specified Playlist.
		/// </summary>
		/// <param name="thePlayList"></param>
		public async Task ProvideGroupContentsAsync( Playlist _ )
		{
			// Playlist items are now read at startup. So this is no longer required
		}

		/// <summary>
		/// Called when the PlaylistsController has obtained or updated the playlists held in the model is received
		/// Display the data held in the Playlists view model
		/// </summary>
		/// <param name="message"></param>
		public void PlaylistsDataAvailable() => Adapter.SetData( PlaylistsViewModel.Playlists.ToList(), SortSelector.SortType.alphabetic );

		/// <summary>
		/// Called when a specific playlist has been updated
		/// Pass on the changes to the adpater
		/// </summary>
		/// <param name="message"></param>
		public void PlaylistUpdated( Playlist playlist ) => ( ( PlaylistsAdapter )Adapter ).PlaylistUpdated( playlist );

		/// <summary>
		/// Called when the number of selected items has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		protected override void SelectedItemsChanged( List<object> selectedObjects )
		{
			// Determine the number of songs and playlists selected.
			IEnumerable< PlaylistItem > songsSelected = selectedItems.Values.OfType<PlaylistItem>();
			IEnumerable<Playlist> playlistSelected = selectedItems.Values.OfType<Playlist>();
			int songCount = songsSelected.Count();
			int playlistCount = playlistSelected.Count();

			// Are all the selected songs from a single playlist
			int parentPlaylistId = ( songCount > 0 ) ? songsSelected.First().PlaylistId : -1;
			bool singlePlaylistSongs = ( songCount > 0 ) && ( songsSelected.Any( item => ( item.PlaylistId != parentPlaylistId ) ) == false ) ;

			// The move up / move down is available if all the songs are from a single playlist and that playlist is not selected, i.e. not all
			// of its songs are selected
			moveUpCommand.Visible = false;
			moveDownCommand.Visible = false;

			if ( ( singlePlaylistSongs == true ) && ( playlistSelected.Any( list => ( list.Id == parentPlaylistId ) ) == false ) )
			{
				// So the playlist containing all the songs is not selected.
				// Need to obtain the playlist to determine which command is available
				Playlist parentPlaylist = PlaylistsViewModel.Playlists.Single( list => ( list.Id == parentPlaylistId ) );

				// Move up is available if the first song is not selected
				moveUpCommand.Visible = songsSelected.Any( list => ( list.Id == parentPlaylist.PlaylistItems.First().Id ) ) == false;

				// Move down is available if the last song is not selected
				moveDownCommand.Visible = songsSelected.Any( list => ( list.Id == parentPlaylist.PlaylistItems.Last().Id ) ) == false;
			}

			// The edit command is only available if a single playlist has been selected
			editCommand.Visible = ( playlistCount == 1 );

			// Set the action bar title
			SetActionBarTitle( songCount, playlistCount );
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new PlaylistsAdapter( Context, listView, this, this );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected override void PostViewCreateAction()
		{
			// Initialise the PlaylistsController
			PlaylistsController.Reporter = this;

			// Get the data
			PlaylistsController.GetPlaylists( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources() => PlaylistsController.Reporter = null;

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected override int Layout { get; } = Resource.Layout.playlists_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.playlistsList;

		/// <summary>
		/// Set the title for the Action Bar according to the number of songs and playlists selected
		/// </summary>
		/// <param name="songCount"></param>
		/// <param name="playlitsCount"></param>
		private void SetActionBarTitle( int songCount, int playlistCount )
		{
			if ( ( songCount == 0 ) && ( playlistCount == 0 ) )
			{
				ActionModeTitle = NoItemsSelectedText;
			}
			else
			{
				string playlistText = ( playlistCount > 0 ) ? string.Format( "{0} playlist{1} ", playlistCount, ( playlistCount == 1 ) ? "" : "s" ) : "";
				string songsText = ( songCount > 0 ) ? string.Format( "{0} song{1} ", songCount, ( songCount == 1 ) ? "" : "s" ) : "";

				ActionModeTitle = string.Format( ItemsSelectedText, playlistText, songsText );
			}
		}

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs or playlist";
		private const string ItemsSelectedText = "{0}{1}selected";
	}
}