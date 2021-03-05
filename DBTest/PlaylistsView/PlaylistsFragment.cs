using Android.Widget;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	public class PlaylistsFragment: PagedFragment<object>, ExpandableListAdapter< object >.IGroupContentsProvider< object >, 
		PlaylistsController.IPlaylistsReporter
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public PlaylistsFragment()
		{
		}

		/// <summary>
		/// Get all the entries associated with a specified Playlist or Tag.
		/// Playlist items are now read at startup. So this is no longer required for them
		/// Tag contents are also obtained on startup, but the Songs in the TaggedAlbums are not and they MAY be required.
		/// There is currently no way to hook into child selection and get the Songs when required. Check for any visual
		/// problems with doing it here
		/// </summary>
		/// <param name="selectedGroup_"></param>
		public async Task ProvideGroupContentsAsync( object selectedGroup )
		{
			if ( selectedGroup is Tag tag )
			{
				foreach ( TaggedAlbum taggedAlbum in tag.TaggedAlbums )
				{
					if ( taggedAlbum.Album.Songs == null )
					{
						await AlbumsController.GetAlbumContentsAsync( taggedAlbum.Album );
					}
				}
			}
		}

		/// <summary>
		/// Called when the PlaylistsController has obtained or updated the playlists held in the model is received
		/// Display the data held in the Playlists view model
		/// </summary>
		/// <param name="message"></param>
		public void DataAvailable()
		{
			Adapter.SetData( PlaylistsViewModel.CombinedList.ToList(), SortSelector.SortType.alphabetic );

			// Display or hide the genres in Tag playlists
			DisplayGenreChanged();
		}

		/// <summary>
		/// Called when a specific playlist has been updated
		/// Pass on the changes to the adpater
		/// </summary>
		/// <param name="message"></param>
		public void PlaylistUpdated( object playlist ) => ( ( PlaylistsAdapter )Adapter ).PlaylistUpdated( playlist );

		/// <summary>
		/// Called when the DisplayGenre flag has been toggled
		/// </summary>
		public void DisplayGenreChanged() => ( ( PlaylistsAdapter )Adapter ).ShowGenre( PlaylistsViewModel.DisplayGenre );

		/// <summary>
		/// Called when the number of selected items has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		protected override void SelectedItemsChanged( GroupedSelection selectedObjects )
		{
			if ( ( selectedObjects.PlaylistItems.Count == 0 ) && ( selectedObjects.Playlists.Count == 0 ) && 
				( selectedObjects.Tags.Count == 0 ) && ( selectedObjects.TaggedAlbums.Count == 0 ) )
			{
				ActionModeTitle = NoItemsSelectedText;
			}
			else
			{
				int playlistCount = selectedObjects.Playlists.Count + selectedObjects.Tags.Count;
				int songCount = selectedObjects.PlaylistItems.Count + selectedObjects.TaggedAlbums.SelectMany( list => list.Album.Songs ).Count();

				string playlistText = ( playlistCount > 0 ) ? string.Format( "{0} playlist{1} ", playlistCount, ( playlistCount == 1 ) ? "" : "s" ) : "";
				string songsText = ( songCount > 0 ) ? string.Format( "{0} song{1} ", songCount, ( songCount == 1 ) ? "" : "s" ) : "";

				ActionModeTitle = string.Format( ItemsSelectedText, playlistText, songsText );
			}
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new PlaylistsAdapter( Context, listView, this, this );

		/// <summary>
		/// Action to be performed after the main view has been created
		/// Initialise the PlaylistsController
		/// </summary>
		protected override void PostViewCreateAction() => PlaylistsController.DataReporter = this;

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources() => PlaylistsController.DataReporter = null;

		/// <summary>
		/// The Layout resource used to create the main view for this fragment
		/// </summary>
		protected override int Layout { get; } = Resource.Layout.playlists_fragment;

		/// <summary>
		/// The resource used to create the ExpandedListView for this fragment
		/// </summary>
		protected override int ListViewLayout { get; } = Resource.Id.playlistsList;

		/// <summary>
		/// The menu resource for this fragment
		/// </summary>
		protected override int Menu { get; } = Resource.Menu.menu_playlists;

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs or playlist";
		private const string ItemsSelectedText = "{0}{1}selected";
	}
}