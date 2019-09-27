using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class NowPlayingAdapter: BaseAdapter, AdapterView.IOnItemLongClickListener, AdapterView.IOnItemClickListener
	{
		public NowPlayingAdapter( Context context, ListView parent )
		{
			// Save the inflator to use when creating the item views
			inflator = ( LayoutInflater )context.GetSystemService( Context.LayoutInflaterService );

			parentView = parent;

			parentView.OnItemLongClickListener = this;
			parentView.OnItemClickListener = this;

			adapterModel = StateModelProvider.Get( typeof( NowPlayingAdpaterModel ) ) as NowPlayingAdpaterModel;
		}

		public override Java.Lang.Object GetItem( int position )
		{
			return position;
		}

		public override long GetItemId( int position )
		{
			return position;
		}

		public override View GetView( int position, View convertView, ViewGroup parent )
		{
			View view = convertView;

			if ( view == null )
			{
				view = inflator.Inflate( Resource.Layout.playlistitem_layout, null );
			}

			Song songItem = NowPlayingItems[ position ].Song;

			// Display the Title and Duration
			view.FindViewById<TextView>( Resource.Id.Title ).Text = songItem.Title;
			view.FindViewById<TextView>( Resource.Id.Duration ).Text = TimeSpan.FromSeconds( songItem.Length ).ToString( @"mm\:ss" );

			// Tag the view with the item position
			view.Tag = position;

			// Display the checkbox
			RenderCheckbox( view, position );

			return view;
		}

		/// <summary>
		/// The number of PlaylistItems
		/// </summary>
		public override int Count
		{
			get
			{
				return NowPlayingItems.Count;
			}
		}

		/// <summary>
		/// Update the data
		/// </summary>
		/// <param name="newData"></param>
		public void SetData( List<PlaylistItem> newData )
		{
			// If this is the first time data has been set then restore group expansions and the Action Mode.
			// If data is being replaced then clear all state data related to the previous data
			if ( NowPlayingItems.Count == 0 )
			{
				NowPlayingItems = newData;
			}
			else
			{
				NowPlayingItems = newData;
			}

			NotifyDataSetChanged();
		}

		public bool OnItemLongClick( AdapterView parent, View view, int position, long id )
		{
			int tag = ( int )view.Tag;

			// If action mode is not in efect then request it.
			// Otherwise ignore long presses
			if ( ActionMode == false )
			{
				ActionMode = true;
			}

			return true;
		}

		/// <summary>
		/// Called when an item has been clicked. This is a request to stop playing the existing item and
		/// play this item. 
		/// This is just a request to play the song and should be passed back in an event.
		/// As a result of this request the song may be played, See the SongBeingPlayed method. 
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="view"></param>
		/// <param name="position"></param>
		/// <param name="id"></param>
		public void OnItemClick( AdapterView parent, View view, int position, long id )
		{
			// Get the Track and Song associated with the displayed item and raise a SongSelected event
			// The Track number should be enough, but pass the Song as well for now
			PlaySongRequested?.Invoke( this, new PlaySongArgs() { TrackNo = NowPlayingItems[ position ].Track,
				SelectedSong = NowPlayingItems[ position ].Song } );
		}

		/// <summary>
		/// Set or clear Action Mode.
		/// In Action Mode checkboxes appear alongside the items and items can be selected
		/// </summary>
		public bool ActionMode
		{
			get
			{
				return adapterModel.ActionMode;
			}
			set
			{
				// Action mode determines whether or not check boxes are shown so refresh the displayed items
				if ( adapterModel.ActionMode != value )
				{
					adapterModel.ActionMode = value;

					if ( adapterModel.ActionMode == true )
					{
						EnteredActionMode?.Invoke( this, new EventArgs() );
					}
					else
					{
						// Clear all selections when leaving Action Mode
						adapterModel.CheckedObjects.Clear();
					}

					NotifyDataSetChanged();
				}
			}
		}

		/// <summary>
		/// Notification that a particular song is being played.
		/// </summary>
		/// <param name="trackId"></param>
		/// <param name="song"></param>
		public void SongBeingPlayed( int trackId, Song song )
		{
			// Highlight the item
			int position = NowPlayingItems.FindIndex( item => ( item.Track == trackId ) );

			if ( position != -1 )
			{
				parentView.SetSelection( position );
			}
		}

		/// <summary>
		/// Show or hide the check box and sets its state from that held for the item
		/// </summary>
		/// <param name="convertView"></param>
		/// <param name="tag"></param>
		private void RenderCheckbox( View convertView, int tag )
		{
			CheckBox selectionBox = convertView.FindViewById<CheckBox>( Resource.Id.checkBox );

			if ( selectionBox != null )
			{
				// Save the item identifier in the check box for the click event
				selectionBox.Tag = tag;

				// Show or hide the checkbox
				selectionBox.Visibility = ( ActionMode == true ) ? ViewStates.Visible : ViewStates.Gone;

				// Retrieve the cheked state of the item and set the checkbox state accordingly
				selectionBox.Checked = IsItemSelected( tag );

				// Trap checkbox clicks
				selectionBox.Click -= SelectionBoxClick;
				selectionBox.Click += SelectionBoxClick;
			}
		}

		/// <summary>
		/// Called when an item's checkbox has been selected
		/// Update the stored state for the item contained in the tag
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectionBoxClick( object sender, EventArgs e )
		{
			CheckBox selectionBox = sender as CheckBox;
			int position = ( int )( ( CheckBox )sender ).Tag;

			// Toggle the selection
			RecordItemSelection( position, !IsItemSelected( position ) );

			SelectedItemsChanged?.Invoke( this, new SelectedItemsArgs() { SelectedItemsCount = adapterModel.CheckedObjects.Count } );
		}

		/// <summary>
		/// Is the specified item selected
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		private bool IsItemSelected( int tag )
		{
			return adapterModel.CheckedObjects.Contains( tag );
		}

		/// <summary>
		/// Record the selection state of the specified item
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="select"></param>
		private bool RecordItemSelection( int tag, bool select )
		{
			return ( select == true ) ? adapterModel.CheckedObjects.Add( tag ) : adapterModel.CheckedObjects.Remove( tag );
		}

		/// <summary>
		/// The event used to publish changes to the number of items selected
		/// </summary>
		public event EventHandler< SelectedItemsArgs > SelectedItemsChanged;

		/// <summary>
		/// Arguments for the selected items event
		/// </summary>
		public class SelectedItemsArgs: EventArgs
		{
			/// <summary>
			/// The number of items selected
			/// </summary>
			public int SelectedItemsCount { get; set; }
		}

		/// <summary>
		/// The event used to publish a request to play a song
		/// </summary>
		public event EventHandler< PlaySongArgs > PlaySongRequested;

		/// <summary>
		/// Arguments for the play song request 
		/// </summary>
		public class PlaySongArgs: EventArgs
		{
			/// <summary>
			/// The track number
			/// </summary>
			public int TrackNo { get; set; }

			public Song SelectedSong { get; set; }
		}

		/// <summary>
		/// The list of PlaylistItems displayed by the ListView
		/// </summary>
		public List<PlaylistItem> NowPlayingItems { get; set; } = new List<PlaylistItem>();

		/// <summary>
		/// The event used to indicate that Acion Mode has been entered
		/// </summary>
		public event EventHandler EnteredActionMode;

		/// <summary>
		/// Inflator used to create the item view 
		/// </summary>
		private readonly LayoutInflater inflator = null;

		private readonly NowPlayingAdpaterModel adapterModel = null;

		private readonly ListView parentView = null;
	}
}