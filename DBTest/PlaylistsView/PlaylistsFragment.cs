using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.App;
using Android.Content;
using System;
using Android.Views.InputMethods;
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
			bool handled = false;

			// Check for a new playlist request
			if ( item.ItemId == Resource.Id.new_playlist )
			{
				// Show a dialogue asking for a new playlist name. Don't install handlers for Ok/Cancel yet.
				// This prevents the default Dismiss action after the buttons are clicked
				EditText playListName = new EditText( Context ) { Hint = "Enter new playlist name" };

				AlertDialog alert = new AlertDialog.Builder( Context )
					.SetTitle( "New playlist" )
					.SetView( playListName )
					.SetPositiveButton( "Ok", ( EventHandler<DialogClickEventArgs> )null )
					.SetNegativeButton( "Cancel", ( EventHandler<DialogClickEventArgs> )null )
					.Create();

				alert.Show();

				// Install a handler for the Ok button that performs the validation and playlist creation
				alert.GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) => 
				{
					string alertText = "";

					if ( playListName.Text.Length == 0 )
					{
						alertText = "An empty name is not valid.";
					}
					else if ( PlaylistsViewModel.PlaylistNames.Contains( playListName.Text ) == true )
					{
						alertText = "A playlist with that name already exists.";
					}
					else
					{
						PlaylistsController.AddPlaylistAsync( playListName.Text );

						// If the media playback control is displayed the keyboard will remain visible, so explicitly get rid of it
						InputMethodManager imm = ( InputMethodManager )Context.GetSystemService( Context.InputMethodService );
						imm.HideSoftInputFromWindow( playListName.WindowToken, 0 );

						alert.Dismiss();
					}

					// Display an error message if the playlist name is not valid. Do not dismiss the dialog
					if ( alertText.Length > 0 )
					{
						new AlertDialog.Builder( Context ).SetTitle( alertText ).SetPositiveButton( "Ok", delegate { } ).Show();
					}
				};

				// Install a handler for the cancel button so that the keyboard can be explicitly hidden
				alert.GetButton( ( int )DialogButtonType.Negative ).Click += ( sender, args ) => 
				{
					// If the media playback control is displayed the keyboard will remain visible, so explicitly get rid of it
					InputMethodManager imm = ( InputMethodManager )Context.GetSystemService( Context.InputMethodService );
					imm.HideSoftInputFromWindow( playListName.WindowToken, 0 );

					alert.Dismiss();
				};
				
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
		public async Task ProvideGroupContentsAsync( Playlist thePlayList )
		{
			if ( thePlayList.PlaylistItems == null )
			{
				await PlaylistsController.GetPlaylistContentsAsync( thePlayList );
			}
		}

		/// <summary>
		/// Called when the PlaylistsDataAvailableMessage is received
		/// Display the data held in the Playlists view model
		/// </summary>
		/// <param name="message"></param>
		public void PlaylistsDataAvailable() => Adapter.SetData( PlaylistsViewModel.Playlists.ToList() );

		/// <summary>
		/// Called when the PlaylistSongsAddedMessage is received
		/// Pass on the changes to the adpater
		/// </summary>
		/// <param name="message"></param>
		public void PlaylistUpdated( string playlistName ) => ( ( PlaylistsAdapter )Adapter ).PlaylistUpdated( playlistName );

		/// <summary>
		/// Called when the number of selected items has changed.
		/// Update the text to be shown in the Action Mode title
		/// </summary>
		public override void SelectedItemsChanged( SortedDictionary<int, object> selectedItems )
		{
			// Determine the number of songs and playlists selected.
			IEnumerable< PlaylistItem > songsSelected = selectedItems.Values.OfType<PlaylistItem>();
			IEnumerable<Playlist> playlistSelected = selectedItems.Values.OfType<Playlist>();
			int songCount = songsSelected.Count();
			int playlistCount = playlistSelected.Count();

			// Determine which commands are available
			// The Play Now and Add To Queue options are available if any songs are selected
			playNowCommand.Visible = ( songCount > 0 );
			addToQueueCommand.Visible = ( songCount > 0 );

			// Are all the selected songs from a single playlist
			int parentPlaylistId = ( songCount > 0 ) ? songsSelected.First().PlaylistId : -1;
			bool singlePlaylistSongs = ( songCount > 0 ) && ( songsSelected.Any( item => ( item.PlaylistId != parentPlaylistId ) ) == false ) ;

			// The Delete command is only available if the selected songs are from a single playlist and only one playlist is selected.
			// Or if just a single empty playlist is selected
			// Remember the playlist could be empty
			deleteCommand.Visible = ( ( singlePlaylistSongs == true ) && ( playlistCount < 2 ) ) || ( ( songCount == 0 ) && ( playlistCount == 1 ) );

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
			renameCommand.Visible = ( playlistCount == 1 );

			// Set the action bar title
			SetActionBarTitle( songCount, playlistCount );

			// Show the command toolbar if one or more commands are enabled
			CommandBar.Visibility = ShowCommandBar();
		}

		/// <summary>
		/// Create the Data Adapter required by this fragment
		/// </summary>
		protected override void CreateAdapter( ExpandableListView listView ) => Adapter = new PlaylistsAdapter( Context, listView, this, this );

		/// <summary>
		/// Called when a command bar command has been invoked
		/// </summary>
		/// <param name="button"></param>
		protected override void HandleCommand( int commandId )
		{
			IEnumerable<PlaylistItem> songsSelected = Adapter.SelectedItems.Values.OfType<PlaylistItem>();
			IEnumerable<Playlist> playlistSelected = Adapter.SelectedItems.Values.OfType<Playlist>();
			int songCount = songsSelected.Count();
			int playlistCount = playlistSelected.Count();

			if ( ( commandId == Resource.Id.add_to_queue ) || ( commandId == Resource.Id.play_now ) )
			{
				BaseController.AddSongsToNowPlayingListAsync( songsSelected.Select( song => song.Song ).ToList(), ( commandId == Resource.Id.play_now ),
					PlaylistsViewModel.LibraryId );
				LeaveActionMode();
			}
			else if ( commandId == Resource.Id.delete )
			{
				// If a playlist as well as songs are selected then prompt the user to check if the playlist entry should be deleted as well
				if ( ( songCount > 0 ) && ( playlistCount > 0 ) )
				{
					new AlertDialog.Builder( Context ).SetTitle( "Do you want to delete the playlist" )
						.SetPositiveButton( "Yes", delegate {
							// Delete the single selected playlist and all of its contents
							PlaylistsController.DeletePlaylistAsync( playlistSelected.First() );
						} )
						.SetNegativeButton( "No", delegate {
							// Just delete the songs. They will all be in the selected playlist
							PlaylistsController.DeletePlaylistItemsAsync( playlistSelected.First(), songsSelected.ToList() );
						} )
						.Show();
				}
				else if ( songCount > 0 )
				{
					// All of the songs will be associated with the same playlist. Need to access the playlist
					Playlist parentPlaylist = PlaylistsViewModel.Playlists.Single( list => ( list.Id == songsSelected.First().PlaylistId ) );

					PlaylistsController.DeletePlaylistItemsAsync( parentPlaylist, songsSelected.ToList() );
				}
				else
				{
					// Deletion of a playlist with no songs
					PlaylistsController.DeletePlaylistAsync( playlistSelected.First() );
				}

				LeaveActionMode();
			}
			else if ( commandId == Resource.Id.move_down )
			{
				// All of the songs will be associated with the same playlist. Need to access the playlist
				Playlist parentPlaylist = PlaylistsViewModel.Playlists.Single( list => ( list.Id == songsSelected.First().PlaylistId ) );

				PlaylistsController.MoveItemsDown( parentPlaylist, songsSelected.ToList() );
			}
			else if ( commandId == Resource.Id.move_up )
			{
				// All of the songs will be associated with the same playlist. Need to access the playlist
				Playlist parentPlaylist = PlaylistsViewModel.Playlists.Single( list => ( list.Id == songsSelected.First().PlaylistId ) );

				PlaylistsController.MoveItemsUp( parentPlaylist, songsSelected.ToList() );
			}

		}

		/// <summary>
		/// Action to be performed after the main view has been created
		/// </summary>
		protected override void PostViewCreateAction()
		{
			// Initialise the PlaylistsController
			PlaylistsController.Reporter = this;

			// Get the data
			PlaylistsController.GetPlaylistsAsync( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Called to release any resources held by the fragment
		/// </summary>
		protected override void ReleaseResources() => PlaylistsController.Reporter = null;

		/// <summary>
		/// Called to allow derived classes to bind to the command bar commands
		/// </summary>
		protected override void BindCommands( CommandBar commandBar )
		{
			addToQueueCommand = commandBar.BindCommand( Resource.Id.add_to_queue );
			playNowCommand = commandBar.BindCommand( Resource.Id.play_now );
			deleteCommand = commandBar.BindCommand( Resource.Id.delete );
			renameCommand = commandBar.BindCommand( Resource.Id.rename );
			moveUpCommand = commandBar.BindCommand( Resource.Id.move_up );
			moveDownCommand = commandBar.BindCommand( Resource.Id.move_down );
		}

		/// <summary>
		/// Let derived classes determine whether or not the command bar should be shown
		/// </summary>
		/// <returns></returns>
		protected override bool ShowCommandBar() => playNowCommand.Visible || addToQueueCommand.Visible || deleteCommand.Visible || moveUpCommand.Visible ||
				moveDownCommand.Visible || renameCommand.Visible;

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
		/// Determine if all of the selected songs are from the same playlist
		/// </summary>
		/// <param name="songsSelected"></param>
		/// <returns></returns>
		private bool SongsInSinglePlaylist( IEnumerable<PlaylistItem> songsSelected, out int playlistId )
		{
			// Traverse the selected songs and check that all of them are from the same playlist
			bool samePlaylist = true;
			playlistId = -1;
			IEnumerator<PlaylistItem> songsToCheck = songsSelected.GetEnumerator();

			while ( ( samePlaylist == true ) && ( songsToCheck.MoveNext() == true ) )
			{
				if ( playlistId == -1 )
				{
					playlistId = songsToCheck.Current.PlaylistId;
				}
				else
				{
					samePlaylist = ( playlistId == songsToCheck.Current.PlaylistId );
				}
			}

			return samePlaylist;
		}

		/// <summary>
		/// Constant strings for the Action Mode bar text
		/// </summary>
		private const string NoItemsSelectedText = "Select songs or playlist";
		private const string ItemsSelectedText = "{0}{1}selected";

		/// <summary>
		/// Command handlers
		/// </summary>
		private CommandBinder addToQueueCommand = null;
		private CommandBinder playNowCommand = null;
		private CommandBinder deleteCommand = null;
		private CommandBinder renameCommand = null;
		private CommandBinder moveUpCommand = null;
		private CommandBinder moveDownCommand = null;
	}
}