using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace DBTest
{
	public class ArtistAlbumListViewAdapter: BaseExpandableListAdapter, ISectionIndexer
	{

		public ArtistAlbumListViewAdapter( Context context, ExpandableListView parentView )
		{
			adapterContext = context;
			parentView.OnItemLongClickListener = new OnItemLongClickListener() { Adapter = this };
		}

		public override int GroupCount
		{
			get
			{
				return artists.Count;
			}
		}

		public override bool HasStableIds
		{
			get
			{
				return false;
			}
		}

		public override Java.Lang.Object GetChild( int groupPosition, int childPosition )
		{
			//			return new JavaObjectWrapper<object>() { Obj = artists[ groupPosition ].Contents[ childPosition ] };
			return null; 
		}

		public override long GetChildId( int groupPosition, int childPosition )
		{
			return childPosition;
		}

		public override int GetChildrenCount( int groupPosition )
		{
			return artists[ groupPosition ].Contents.Count;
		}

		public override View GetChildView( int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent )
		{
			LayoutInflater inflator = ( LayoutInflater )adapterContext.GetSystemService( Context.LayoutInflaterService );

			// The child can be either a ArtistAlbum or a Song which use different layouts
			object childObject = artists[ groupPosition ].Contents[ childPosition ];
			if ( ( childObject is ArtistAlbum ) == true )
			{
				// If the supplied view previously contained a Song then dispose of it
				if ( convertView != null )
				{
					if ( convertView.FindViewById<TextView>( Resource.Id.AlbumName ) == null )
					{
						// Not an ArtistAlbum entry so make sure a new view is created
						convertView = null;
					}
				}

				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.album_layout, null );
				}

				TextView albumName = convertView.FindViewById<TextView>( Resource.Id.AlbumName );
				albumName.Text = ( ( ArtistAlbum )childObject ).Name;

				CheckBox selectionBox = convertView.FindViewById<CheckBox>( Resource.Id.checkBox );
				selectionBox.Visibility = ( ActionMode == true ) ? ViewStates.Visible : ViewStates.Gone;
			}
			else
			{
				// If the supplied view previously contained an ArtistAlbum then dispose of it
				if ( convertView != null )
				{
					if ( convertView.FindViewById<TextView>( Resource.Id.Title ) == null )
					{
						// Not a Song entry so make sure a new view is created
						convertView = null;
					}
				}

				if ( convertView == null )
				{
					convertView = inflator.Inflate( Resource.Layout.song_layout, null );
				}

				TextView trackNumber = convertView.FindViewById<TextView>( Resource.Id.Track );
				trackNumber.Text = ( ( Song )childObject ).Track.ToString();

				TextView songTitle = convertView.FindViewById<TextView>( Resource.Id.Title );
				songTitle.Text = ( ( Song )childObject ).Title;

				TextView duration = convertView.FindViewById<TextView>( Resource.Id.Duration );
				duration.Text = TimeSpan.FromSeconds( ( ( Song )childObject ).Length ).ToString( @"mm\:ss" );

				CheckBox selectionBox = convertView.FindViewById<CheckBox>( Resource.Id.checkBox );
				selectionBox.Visibility = ( ActionMode == true ) ? ViewStates.Visible : ViewStates.Gone;
			}

			return convertView;
		}

		public override Java.Lang.Object GetGroup( int groupPosition )
		{
			//return new JavaObjectWrapper<Artist>() { Obj = artists[ groupPosition ] };
			return null;
		}

		public override long GetGroupId( int groupPosition )
		{
			return groupPosition;
		}

		public override View GetGroupView( int groupPosition, bool isExpanded, View convertView, ViewGroup parent )
		{
			if ( convertView == null )
			{
				LayoutInflater inflator = ( LayoutInflater )adapterContext.GetSystemService( Context.LayoutInflaterService );
				convertView = inflator.Inflate( Resource.Layout.artist_layout, null );
			}

			TextView artistName = convertView.FindViewById<TextView>( Resource.Id.ArtistName );
			artistName.Text = artists[ groupPosition ].Name;

			CheckBox selectionBox = convertView.FindViewById<CheckBox>( Resource.Id.checkBox );
			selectionBox.Visibility = ( ActionMode == true ) ? ViewStates.Visible : ViewStates.Gone;

			return convertView;
		}

		public int GetPositionForSection( int sectionIndex )
		{
			return alphaIndexer[ sections[ sectionIndex ] ];
		}

		public int GetSectionForPosition( int position )
		{
			int prevSection = 0;
			int index = 0;
			bool positionFound = false;

			while ( ( positionFound == false ) && ( index < sections.Length ) )
			{
				if ( GetPositionForSection( index ) > position )
				{
					positionFound = true;
				}
				else
				{
					prevSection = index++;
				}
			}

			return prevSection;
		}

		public Java.Lang.Object[] GetSections()
		{
			return new Java.Util.ArrayList( alphaIndexer.Keys ).ToArray();
		}

		public override bool IsChildSelectable( int groupPosition, int childPosition )
		{
			return true;
		}

		public void SetData( List<Artist> newData, Dictionary<string, int> alphaIndex )
		{
			alphaIndexer = alphaIndex;
			sections = alphaIndexer.Keys.ToArray();
			artists = newData;
			NotifyDataSetChanged();
		}

		/// <summary>
		/// Keep track of whether or not action mode is in effect
		/// </summary>
		private bool actionMode = false;

		public bool ActionMode
		{
			get
			{
				return actionMode;
			}
			set
			{
				// Action mode determines whether or not check boxes are shown so refresh the displayed items
				actionMode = value;
				NotifyDataSetChanged();
			}
		}

		private Context adapterContext = null;
		public List< Artist > artists = new List<Artist>();
		private Dictionary<string, int> alphaIndexer = null;
		private string[] sections = null;

		private ItemLongClickListener clickListener = new ItemLongClickListener();

		private class ItemLongClickListener: Java.Lang.Object, ExpandableListView.IOnLongClickListener
		{
			public bool OnLongClick( View v )
			{
				return true;
			}
		}

		private class OnItemLongClickListener: Java.Lang.Object, AdapterView.IOnItemLongClickListener
		{
			public ArtistAlbumListViewAdapter Adapter { get; set; }

			public bool OnItemLongClick( AdapterView parent, View view, int position, long id )
			{
				// If action mode is not in efect then request it.
				// Otherwise ignore long presses
				if ( Adapter.ActionMode == false )
				{
					Adapter.ActionModeRequested?.Invoke( Adapter, new EventArgs() );
				}

				return true;
			}
		}

		/// <summary>
		/// The event used to indicate that entry to action mode has been requested
		/// </summary>
		public event EventHandler ActionModeRequested;
	}

	public class JavaObjectWrapper<T>: Java.Lang.Object { public T Obj { get; set; } }


}