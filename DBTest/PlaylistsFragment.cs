using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using SQLite;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;

namespace DBTest
{
	public class PlaylistsFragment: Fragment, ExpandableListAdapter< Artist >.IGroupContentsProvider< Artist >, ActionMode.ICallback, IPageVisible
	{
		/// <summary>
		/// Default constructor required for system view hierarchy restoration
		/// </summary>
		public PlaylistsFragment()
		{
		}

		/// <summary>
		/// Called when the fragment is created but before any UI elements are available.
		/// If the artists list is not already available then get it now
		/// </summary>
		/// <param name="savedInstanceState"></param>
		public override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			// Get the ConnectionDetailsModel to provide the database path and library identity
			connectionModel = ViewModelProvider.Get( typeof( ConnectionDetailsModel ) ) as ConnectionDetailsModel;

			// Get the PlaylistsModel
			content = ViewModelProvider.Get( typeof( PlaylistsModel ) ) as PlaylistsModel;

			// If this is a new model then populate it
			if ( content.IsNew == true )
			{
				GetArtistDetails();
			}

			// Allow this fragment to add menu items to the activity toolbar
			HasOptionsMenu = true;
		}

		/// <summary>
		/// Called to create the UI components to display playlists
		/// </summary>
		/// <param name="inflater"></param>
		/// <param name="container"></param>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			// Create the view
			View view = inflater.Inflate( Resource.Layout.playlists_fragment, container, false );

			// Get the ExpandableListView and link to a PlaylistsAdapter
			listView = view.FindViewById<ExpandableListView>( Resource.Id.playlistsLayout );

			adapter = new PlaylistsAdapter( Context, listView, this );
			listView.SetAdapter( adapter );

			// Detect when the adapter has entered Action Mode
			adapter.EnteredActionMode += EnteredActionMode;

			// If the Artist data is already available then display it now
			if ( content.IsNew == false )
			{
				adapter.SetData( content.Artists, content.AlphaIndex );
			}

			// TESTING
			bottomBar = view.FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.bottomToolbar );

			bottomBar.Visibility = ( expandedGroupCount > 0 ) ? ViewStates.Visible : ViewStates.Gone;

			return view;
		}

		/// <summary>
		/// Add fragment specific menu items to the main toolbar
		/// </summary>
		/// <param name="menu"></param>
		/// <param name="inflater"></param>
		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			inflater.Inflate( Resource.Menu.menu_playlists, menu );

			// Show or hide the collapse 
			collapseItem = menu.FindItem( Resource.Id.action_collapse );
			collapseItem.SetVisible( expandedGroupCount > 0 );
		}

		/// <summary>
		/// Called when a menu item has been selected
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			int id = item.ItemId;

			// Pass on a collapse request to the adapter
			if ( id == Resource.Id.action_collapse )
			{
				adapter.OnCollapseRequest();
			}

			return base.OnOptionsItemSelected( item );
		}

		/// <summary>
		/// Get all the ArtistAlbum entries associated with a specified Artist.
		/// </summary>
		/// <param name="theArtist"></param>
		public void ProvideGroupContents( Artist theArtist )
		{
			if ( theArtist.ArtistAlbums == null )
			{
				using ( SQLiteConnection db = new SQLiteConnection( connectionModel.DatabasePath ) )
				{
					db.GetChildren<Artist>( theArtist );

					// Sort the albums alphabetically
					theArtist.ArtistAlbums.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

					foreach ( ArtistAlbum artistAlbum in theArtist.ArtistAlbums )
					{
						db.GetChildren<ArtistAlbum>( artistAlbum );

						// Sort the songs by track number
						artistAlbum.Songs.Sort( ( a, b ) => a.Track.CompareTo( b.Track ) );
					}
				}

				// Now all the ArtistAlbum and Song entries have been read form a single list from them
				theArtist.EnumerateContents();
			}
		}

		/// <summary>
		/// Called when the count of expanded artist groups has changed
		/// Show or hide associated UI elements
		/// </summary>
		/// <param name="count"></param>
		public void ExpandedGroupCountChanged( int count )
		{
			expandedGroupCount = count;

			if ( collapseItem != null )
			{
				collapseItem.SetVisible( expandedGroupCount > 0 );
			}

			if ( bottomBar != null )
			{
				bottomBar.Visibility = ( expandedGroupCount > 0 ) ? ViewStates.Visible : ViewStates.Gone;
			}
		}

		/// <summary>
		/// Called when a menu item on the Contextual Action Bar has been selected
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool OnActionItemClicked( ActionMode mode, IMenuItem item )
		{
			return false;
		}

		/// <summary>
		/// Called when the Contextual Action Bar is created.
		/// Add any configured menu items
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="menu"></param>
		/// <returns></returns>
		public bool OnCreateActionMode( ActionMode mode, IMenu menu )
		{
			// Keep a record of the ActionMode instance so that it can be destroyed when this fragmeny is hidden
			actionModeInstance = mode;
			MenuInflater inflater = actionModeInstance.MenuInflater;
			inflater.Inflate( Resource.Menu.action_mode, menu );
			return true;
		}

		/// <summary>
		/// Called when the Contextual Action Bar is destroyed.
		/// </summary>
		/// <param name="mode"></param>
		public void OnDestroyActionMode( ActionMode mode )
		{
			// If the Contextual Action Bar is being destroyed by the user then inform the adapter
			if ( retainAdapterActionMode == false )
			{
				adapter.ActionMode = false;
			}
			actionModeInstance = null;
		}

		/// <summary>
		/// Required by the interface
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="menu"></param>
		/// <returns></returns>
		public bool OnPrepareActionMode( ActionMode mode, IMenu menu )
		{
			return false;
		}

		/// <summary>
		/// Called when this fragment is shown or hidden
		/// </summary>
		/// <param name="visible"></param>
		public void PageVisible( bool visible )
		{
			if ( visible == true )
			{
				// If the Contextual Action Bar was being displyed before the fragment was hidden then show it again
				if ( retainAdapterActionMode == true )
				{
					Activity.StartActionMode( this );
					retainAdapterActionMode = false;
				}
			}
			else
			{
				// Record that the Contextual Action Bar was being shown and then destroy it
				if ( actionModeInstance != null )
				{
					retainAdapterActionMode = true;
					actionModeInstance.Finish();
				}
			}
		}

		/// <summary>
		/// Get all the Artists associated with the library identity specified in the ComnnectionDetailsModel
		/// </summary>
		private async void GetArtistDetails( )
		{
			SQLiteAsyncConnection dbAsynch = new SQLiteAsyncConnection( connectionModel.DatabasePath );

			Library songLibrary = await dbAsynch.GetAsync<Library>( connectionModel.LibraryId );
			await dbAsynch.GetChildrenAsync<Library>( songLibrary );

			// Get all of the artist details from the database
			for ( int artistIndex = 0; artistIndex < songLibrary.Artists.Count; ++artistIndex )
			{
				songLibrary.Artists[ artistIndex ] = await dbAsynch.GetAsync<Artist>( songLibrary.Artists[ artistIndex ].Id );
			}

			// Sort the list of artists by name
			songLibrary.Artists.Sort( ( a, b ) => {
				// Do a normal comparison, except remove a leading 'The ' before comparing
				string artistA = a.Name;
				if ( a.Name.ToUpper().StartsWith( "THE " ) == true )
				{
					artistA = a.Name.Substring( 4 );
				}

				string artistB = b.Name;
				if ( b.Name.ToUpper().StartsWith( "THE " ) == true )
				{
					artistB = b.Name.Substring( 4 );
				}

				return artistA.CompareTo( artistB );
			} );

			// Work out the section indexes for the sorted data
			Dictionary<string, int> alphaIndex = new Dictionary<string, int>();
			int index = 0;
			foreach ( Artist artist in songLibrary.Artists )
			{
				string key = artist.Name[ 0 ].ToString();
				if ( alphaIndex.ContainsKey( key ) == false )
				{
					alphaIndex[ key ] = index;
				}
				index++;
			}

			// Now load the adapter with this data
			content.Artists = songLibrary.Artists;
			content.AlphaIndex = alphaIndex;

			adapter.SetData( songLibrary.Artists, alphaIndex );
		}

		/// <summary>
		/// A request to enter action mode has been requested
		/// Display the Contextual Action Bar
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EnteredActionMode( object sender, System.EventArgs e )
		{
			// If this fragment is not being displayed then record that the Contextual Action Bar should be displayed when the fragment 
			// is visible
			if ( IsVisible == true )
			{
				Activity.StartActionMode( this );
			}
			else
			{
				retainAdapterActionMode = true;
			}
		}

		private ExpandableListView listView = null;

		private IMenuItem collapseItem = null;

		private PlaylistsAdapter adapter = null;

		private int expandedGroupCount = 0;

		private Android.Support.V7.Widget.Toolbar bottomBar = null;

		private ActionMode actionModeInstance = null;

		private bool retainAdapterActionMode = false;

		private PlaylistsModel content = null;

		private ConnectionDetailsModel connectionModel = null;
	}
}