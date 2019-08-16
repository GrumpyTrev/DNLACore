using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System.Collections.Generic;

namespace DBTest
{
	public class PlaylistsFragment: Fragment, PlaylistsAdapter.IArtistContentsProvider
	{
		public PlaylistsFragment( string databasePath, Library songLibrary )
		{
			databaseName = databasePath;
			songs = songLibrary;
		}
		
		public override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );

			HasOptionsMenu = true;

			dbAsynch = new SQLiteAsyncConnection( databaseName );
			db = new SQLiteConnection( databaseName );

			GetArtistDetails( songs );
		}

		public override View OnCreateView( LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState )
		{
			View view = inflater.Inflate( Resource.Layout.playlists_fragment, container, false );

			listView = view.FindViewById<ExpandableListView>( Resource.Id.playlistsLayout );

			adapter = new PlaylistsAdapter( Context, listView, this );

			listView.SetAdapter( adapter );

			return view;
		}

		public override void OnCreateOptionsMenu( IMenu menu, MenuInflater inflater )
		{
			inflater.Inflate( Resource.Menu.menu_playlists, menu );

			collapseItem = menu.FindItem( Resource.Id.action_collapse );
			collapseItem.SetVisible( false );
		}

		public override bool OnOptionsItemSelected( IMenuItem item )
		{
			int id = item.ItemId;

			if ( id == Resource.Id.action_collapse )
			{
				adapter.OnCollapseRequest();
			}

			return base.OnOptionsItemSelected( item );
		}

		public void ProvideArtistContents( Artist theArtist )
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

			// Now all the ArtistAlbum and Song entries have been read form a single list from them
			theArtist.EnumerateContents();
		}

		public void ExpandedGroupCountChanged( int count )
		{
			collapseItem.SetVisible( count > 0 );
		}

		private async void GetArtistDetails( Library songLibrary )
		{
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
			adapter.SetData( songLibrary.Artists, alphaIndex );
		}


		private string databaseName = "";

		private Library songs = null;

		private ExpandableListView listView = null;

		private IMenuItem collapseItem = null;

		private PlaylistsAdapter adapter = null;

		private SQLiteConnection db = null;
		private SQLiteAsyncConnection dbAsynch = null;
	}
}