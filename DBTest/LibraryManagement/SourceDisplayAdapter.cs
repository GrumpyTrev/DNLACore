using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class SourceDisplayAdapter : BaseAdapter, AdapterView.IOnItemClickListener
	{
		public SourceDisplayAdapter( Context context, List< Source > sources, ListView parent, IReporter reporter )
		{
			inflator = ( LayoutInflater )context.GetSystemService( Context.LayoutInflaterService );
			this.sources = sources;
			this.parent = parent;

			Reporter = reporter;

			parent.OnItemClickListener = this;
		}

		/// <summary>
		/// The following are required by BaseAdapter
		/// </summary>
		/// <returns></returns>
		public override Java.Lang.Object GetItem( int position ) => position;
		public override long GetItemId( int position ) => position;

		public override View GetView( int position, View convertView, ViewGroup parent )
		{
			if ( convertView == null )
			{
				convertView = inflator.Inflate( Resource.Layout.source_item_layout, null );
			}

			convertView.FindViewById<TextView>( Resource.Id.sourceName ).Text = sources[ position ].Name;
			convertView.FindViewById<TextView>( Resource.Id.sourceType ).Text = sources[ position ].AccessMethod.ToString();
			convertView.FindViewById<TextView>( Resource.Id.sourceFolder ).Text = sources[ position ].FolderName;

			return convertView;
		}

		/// <summary>
		/// Called when a source item has been selected.
		/// Report this back to the IReporter
		/// </summary>
		/// <param name="v"></param>
		public void OnItemClick( AdapterView parent, View view, int position, long id )
		{
			int actualPosition = position - this.parent.HeaderViewsCount;
			if ( actualPosition >= 0 )
			{
				Reporter?.OnSourceSelected( sources[ position - this.parent.HeaderViewsCount ] );
			}
		}

		/// <summary>
		/// The number of items in the list
		/// </summary>
		public override int Count => sources.Count;

		/// <summary>
		/// The inflator for the view
		/// </summary>
		private readonly LayoutInflater inflator = null;

		/// <summary>
		/// The sources to display
		/// </summary>
		private readonly List<Source> sources = null;

		/// <summary>
		/// The list view displaying the items
		/// </summary>
		private readonly ListView parent = null;

		/// <summary>
		/// The interface used to report back group status changes
		/// </summary>
		private IReporter Reporter { get; set; } = null;

		/// <summary>
		/// The interface used to report back group status changes
		/// </summary>
		public interface IReporter
		{
			void OnSourceSelected( Source selectedSource );
		}
	}
}
