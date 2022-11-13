using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The sort selector class associates an array of resource ids with an enum type and allows the resource id to be selected by enum value
	/// </summary>
	public class SortSelector
	{
		/// <summary>
		/// Constructor.
		/// Initialise the sort pairings. 
		/// </summary>
		public SortSelector( SortSelectionDelegate selectionCallback, SortSelectionModel selectionData )
		{
			SortData = selectionData;
			selectionDelegate = selectionCallback;
		}

		/// <summary>
		/// Associate this sort selector with a menu item used to change the order and to display the currently selected order
		/// </summary>
		/// <param name="item"></param>
		public void BindToMenu( IMenuItem item, Context context )
		{
			IMenuItem boundMenuItem = item;

			// Allow a null item to be passed in which case nothing is bound
			if ( boundMenuItem != null )
			{
				popupContext = context;

				boundMenuItem.SetActionView( Resource.Layout.toolbarButton );
				imageButton = boundMenuItem.ActionView.FindViewById<AppCompatImageButton>( Resource.Id.toolbarSpecialButton );
				boundMenuItem.ActionView.SetOnClickListener( new ClickHandler() { OnClickAction = () => SortAction() } );
				boundMenuItem.ActionView.SetOnLongClickListener( new LongClickHandler() { OnClickAction = () => LongSortAction() } );

				DisplaySortIcon();
			}
			else
			{
				imageButton = null;
			}

		}

		/// <summary>
		/// Display the sort icon associated with the current sort order
		/// </summary>
		public void DisplaySortIcon() => imageButton?.SetImageResource( resources[ ( int )SortData.CurrentSortOrder ] );

		/// <summary>
		/// Called when the sort icon has been selected
		/// </summary>
		private void SortAction()
		{
			// Select the next sort order
			SortData.SelectNext();

			selectionDelegate?.Invoke();
		}

		/// <summary>
		/// Called when the sort icon has been long clicked
		/// </summary>
		/// <returns></returns>
		private bool LongSortAction()
		{
			// Create a Popup menu containing the sort options
			PopupMenu playlistsMenu = new( popupContext, imageButton );

			// Need some way of associating the selected menu item with the pairing type
			Dictionary<IMenuItem, SortType> pairingTypeLookup = new();

			foreach ( KeyValuePair< SortType, SortSelectionModel.SortPairing> pair in SortData.SortPairings )
			{
				SortSelectionModel.SortPairing pairing = pair.Value;

				if ( pairing.Available == true )
				{
					IMenuItem item = playlistsMenu.Menu.Add( pairing.PairingName );
					pairingTypeLookup[ item ] = pair.Key;

					// Put a check mark against the currently selected pairing
					if ( pairing == SortData.ActiveSortOrder )
					{
						item.SetCheckable( true );
						item.SetChecked( true );
					}
				}
			}

			// If there is only a single option then don't bother displaying the menu
			if ( playlistsMenu.Menu.Size() > 1 )
			{
				// When a menu item is clicked find out which SortPairing has been selected.
				// If it has changed then make it the ActiveSortOrder and report the change
				playlistsMenu.MenuItemClick += ( sender1, args1 ) =>
				{
					SortType selectedType = pairingTypeLookup[ args1.Item ];
					if ( selectedType != SortData.ActiveSortType )
					{
						SortData.SetActiveSortOrder( selectedType );
						selectionDelegate?.Invoke();
					}
				};

				playlistsMenu.Show();
			}

			return true;
		}

		/// <summary>
		/// The resource ids representing icons to be displayed 
		/// </summary>
		private readonly int[] resources = new int[] { Resource.Drawable.sort_by_alpha_ascending, Resource.Drawable.sort_by_alpha_descending,
			Resource.Drawable.sort_by_id_ascending, Resource.Drawable.sort_by_id_descending, Resource.Drawable.sort_by_year_ascending,
			Resource.Drawable.sort_by_year_descending, Resource.Drawable.sort_by_genre_ascending, Resource.Drawable.sort_by_genre_descending
		};


		/// <summary>
		/// The button (icon) item that this monitor is bound to
		/// </summary>
		private AppCompatImageButton imageButton = null;

		/// <summary>
		/// The Context to use when creating the popup menu
		/// </summary>
		private Context popupContext = null;

		public SortSelectionModel SortData { get; private set; }

		/// <summary>
		/// Delegate used to report when the sort order has been changed
		/// </summary>
		public delegate void SortSelectionDelegate();

		/// <summary>
		/// The FilterSelectionDelegate to be used for this instance
		/// </summary>
		private readonly SortSelectionDelegate selectionDelegate = null;

		/// <summary>
		/// Class required to implement the View.IOnLongClickListener interface
		/// </summary>
		private class LongClickHandler : Java.Lang.Object, View.IOnLongClickListener
		{
			/// <summary>
			/// Called when a click has been detected
			/// </summary>
			/// <param name="v"></param>
			public bool OnLongClick( View v ) => OnClickAction();

			/// <summary>
			/// The Action to be performed when a click has been detected
			/// </summary>
			public Func<bool> OnClickAction;
		}

		/// <summary>
		/// Class required to implement the View.IOnClickListener interface
		/// </summary>
		private class ClickHandler : Java.Lang.Object, View.IOnClickListener
		{
			/// <summary>
			/// Called when a click has been detected
			/// </summary>
			/// <param name="v"></param>
			public void OnClick( View v ) => OnClickAction();

			/// <summary>
			/// The Action to be performed when a click has been detected
			/// </summary>
			public Action OnClickAction;
		}
	}
}
