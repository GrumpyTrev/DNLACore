using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class TagDataAdapter: BaseAdapter, IListAdapter, AdapterView.IOnItemClickListener
	{

		private Context context;

		public List<AppliedTag> TagData { get; private set; } = new List<AppliedTag>();

		public TagDataAdapter( Context context, ListView parentView )
		{
			this.context = context;
			parentView.OnItemClickListener = this;
		}

		public void SetData( List<AppliedTag> appliedTags )
		{
			TagData = appliedTags;

			NotifyDataSetChanged();
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
				view = ( ( LayoutInflater )context.GetSystemService( Context.LayoutInflaterService ) ).Inflate( Resource.Layout.tag_item_layout, null );
			}

			if ( view != null )
			{
				view.FindViewById<TextView>( Resource.Id.TagName ).Text = TagData[ position ].TagName;

				CheckBox selectionBox = view.FindViewById<CheckBox>( Resource.Id.checkBox );
				ImageView indetermiate = view.FindViewById<ImageView>( Resource.Id.indeterminateCheck );

				if ( TagData[ position ].Applied == AppliedTag.AppliedType.Some )
				{
					selectionBox.Visibility = ViewStates.Invisible;
					indetermiate.Visibility = ViewStates.Visible;
				}
				else
				{
					selectionBox.Visibility = ViewStates.Visible;
					selectionBox.Checked = ( TagData[ position ].Applied == AppliedTag.AppliedType.All );
					indetermiate.Visibility = ViewStates.Invisible;
				}

				// Trap checkbox clicks
				selectionBox.Click -= SelectionBoxClick;
				selectionBox.Click += SelectionBoxClick;

				indetermiate.Click -= IndeterminateClick;
				indetermiate.Click += IndeterminateClick;

				selectionBox.Tag = position;
				indetermiate.Tag = position;
				view.Tag = position;
			}

			return view;
		}

		private void IndeterminateClick( object sender, EventArgs e )
		{
			TagData[ ( int )( sender as View ).Tag ].Applied  = AppliedTag.AppliedType.All;
			NotifyDataSetChanged();
		}

		/// <summary>
		/// Called when an item's checkbox has been selected
		/// Update the stored state for the item contained in the tag
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectionBoxClick( object sender, EventArgs e )
		{
			AppliedTag appliedTag = TagData[ ( int )( sender as View ).Tag ];
			appliedTag.Applied = ( appliedTag.Applied == AppliedTag.AppliedType.All ) ? AppliedTag.AppliedType.None : AppliedTag.AppliedType.All;

			NotifyDataSetChanged();

		}

		public void OnItemClick( AdapterView parent, View view, int position, long id )
		{
			SelectionBoxClick( view, null );
		}

		public override int Count
		{
			get
			{
				return TagData.Count;
			}
		}

	}
}