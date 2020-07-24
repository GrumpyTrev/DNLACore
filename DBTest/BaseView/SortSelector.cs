using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;

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
		public SortSelector()
		{
			SortPairings.Add( SortType.alphabetic, new SortPairing() { PairingName = "Alphabetic", Order1 = SortOrder.alphaAscending, Order2 = SortOrder.alphaDescending } );
			SortPairings.Add( SortType.identity, new SortPairing() { PairingName = "Identity", Order1 = SortOrder.idDescending, Order2 = SortOrder.idAscending } );
			SortPairings.Add( SortType.year, new SortPairing() { PairingName = "Year", Order1 = SortOrder.yearDescending, Order2 = SortOrder.yearAscending } );
			SetActiveSortOrder( SortType.alphabetic );
		}

		/// <summary>
		/// Associate this sort selector with a menu item used to change the order and to display the currently selected order
		/// </summary>
		/// <param name="item"></param>
		public void BindToMenu( IMenuItem item, Context context, ISortReporter callback )
		{
			boundMenuItem = item;

			// Allow a null item to be passed in which case nothing is bound
			if ( boundMenuItem != null )
			{
				popupContext = context;
				Reporter = callback;

				boundMenuItem.SetActionView( Resource.Layout.toolbarButton );
				boundMenuItem.ActionView.SetOnClickListener( new ClickHandler() { OnClickAction = () => { SortAction(); } } );
				boundMenuItem.ActionView.SetOnLongClickListener( new LongClickHandler() { OnClickAction = () => { return LongSortAction(); } } );

				DisplaySortIcon();
			}
		}

		/// <summary>
		/// Display the sort icon associated with the current sort order
		/// </summary>
		public void DisplaySortIcon() => 
			boundMenuItem?.ActionView.FindViewById<AppCompatImageButton>( Resource.Id.sortSpecial ).SetImageResource( SelectedResource );

		/// <summary>
		/// The enum used to select a sort order
		/// </summary>
		public enum SortOrder { alphaAscending = 0, alphaDescending = 1, idAscending = 2, idDescending = 3, yearAscending = 4, yearDescending = 5 };

		/// <summary>
		/// The enum identifying the types of sorting possible
		/// </summary>
		public enum SortType { alphabetic, identity, year };

		/// <summary>
		/// Select the next sort order
		/// </summary>
		public void SelectNext() => ActiveSortOrder.SelectNext();

		/// <summary>
		/// Get the resource associated with the current sort order
		/// </summary>
		public int SelectedResource => resources[ ( int )CurrentSortOrder ];

		/// <summary>
		/// The current sort order
		/// </summary>
		public SortOrder CurrentSortOrder
		{
			get => ActiveSortOrder.CurrentSortOrder;
			set => ActiveSortOrder.CurrentSortOrder = value;
		}

		/// <summary>
		/// Set the specified Sort Type as active
		/// </summary>
		/// <param name="activeType"></param>
		public void SetActiveSortOrder( SortType activeType )
		{
			ActiveSortOrder = SortPairings[ activeType ];
			ActiveSortOrder.CurrentSortOrder = ActiveSortOrder.Order1;
			ActiveSortType = activeType;
		}

		/// <summary>
		/// Make the specified sort types available for selection by the user
		/// </summary>
		/// <param name="sortType"></param>
		public void MakeAvailable( List<SortType> sortTypes )
		{
			// First make all the sort types unavailable
			SortPairings.Values.ToList().ForEach( item => item.Available = false );

			// Set each of the specified sort types available
			sortTypes.ForEach( item => SortPairings[ item ].Available = true );
		}

		/// <summary>
		/// The currently active sort order
		/// </summary>
		public SortType ActiveSortType { get; set; }

		/// <summary>
		/// Called when the sort icon has been selected
		/// </summary>
		private void SortAction()
		{
			// Select the next sort order
			SelectNext();

			Reporter?.SortOrderChanged();
		}

		/// <summary>
		/// Called when the sort icon has been long clicked
		/// </summary>
		/// <returns></returns>
		private bool LongSortAction()
		{
			// Create a Popup menu containing the sort options
			PopupMenu playlistsMenu = new PopupMenu( popupContext, boundMenuItem.ActionView.FindViewById<AppCompatImageButton>( Resource.Id.sortSpecial ) );

			// Need some way of associating the selected menu item with the pairing type
			Dictionary<IMenuItem, SortType> pairingTypeLookup = new Dictionary<IMenuItem, SortType>();

			foreach ( KeyValuePair< SortType, SortPairing > pair in SortPairings )
			{
				SortPairing pairing = pair.Value;

				if ( pairing.Available == true )
				{
					IMenuItem item = playlistsMenu.Menu.Add( pairing.PairingName );
					pairingTypeLookup[ item ] = pair.Key;

					// Put a check mark against the currently selected pairing
					if ( pairing == ActiveSortOrder )
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
					if ( selectedType != ActiveSortType )
					{
						SetActiveSortOrder( selectedType );
						Reporter?.SortOrderChanged();
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
			Resource.Drawable.sort_by_year_descending
		};

		/// <summary>
		/// The collection of possible SortPairings
		/// </summary>
		private Dictionary<SortType, SortPairing> SortPairings = new Dictionary<SortType, SortPairing>();

		/// <summary>
		/// The ActiveSortOrder property.
		/// </summary>
		private SortPairing ActiveSortOrder { get; set; }

		/// <summary>
		/// The menu item that this selector its bound to
		/// </summary>
		private IMenuItem boundMenuItem = null;

		/// <summary>
		/// The Context to use when creating the popup menu
		/// </summary>
		private Context popupContext = null;

		/// <summary>
		/// The iterface used to report sort order changes
		/// </summary>
		public ISortReporter Reporter { get; set; } = null;

		/// <summary>
		/// Inteface used to report sort order changes
		/// </summary>
		public interface ISortReporter
		{
			void SortOrderChanged();
		}

		/// <summary>
		/// The SortPairing class encapsulates a pair of related sort orders that toggled between by the user
		/// </summary>
		private class SortPairing
		{
			/// <summary>
			/// The two sort orders that can be switched between
			/// </summary>
			public SortOrder Order1 { get; set; }
			public SortOrder Order2 { get; set; }

			/// <summary>
			/// Whether or not this AortPairing is available to the user
			/// </summary>
			public bool Available { get; set; } = false;

			/// <summary>
			/// Which of the two SortOrders are currently selected 
			/// </summary>
			public SortOrder CurrentSortOrder { get; set; }

			/// <summary>
			/// Select the next sort order
			/// </summary>
			public void SelectNext() => CurrentSortOrder = ( CurrentSortOrder == Order1 ) ? Order2 : Order1;

			/// <summary>
			/// The displayable name for this pairing
			/// </summary>
			public string PairingName { get; set; }
		}

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