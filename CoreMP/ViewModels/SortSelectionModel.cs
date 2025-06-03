using System.Collections.Generic;
using System.Linq;

namespace CoreMP
{
	public class SortSelectionModel
	{
		public SortSelectionModel()
		{
			SortPairings.Add( SortType.alphabetic, new SortPairing() { PairingName = "Alphabetic", Order1 = SortOrder.alphaAscending, Order2 = SortOrder.alphaDescending } );
			SortPairings.Add( SortType.identity, new SortPairing() { PairingName = "Identity", Order1 = SortOrder.idDescending, Order2 = SortOrder.idAscending } );
			SortPairings.Add( SortType.year, new SortPairing() { PairingName = "Year", Order1 = SortOrder.yearDescending, Order2 = SortOrder.yearAscending } );
			SortPairings.Add( SortType.genre, new SortPairing() { PairingName = "Genre", Order1 = SortOrder.genreAscending, Order2 = SortOrder.genreDescending } );

			SetActiveSortOrder( SortType.alphabetic );
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
		/// Select the next sort order
		/// </summary>
		public void SelectNext() => ActiveSortOrder.SelectNext();

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
		/// The collection of possible SortPairings
		/// </summary>
		public readonly Dictionary<SortType, SortPairing> SortPairings = new Dictionary<SortType, SortPairing>();

		/// <summary>
		/// The current sort order
		/// </summary>
		public SortOrder CurrentSortOrder
		{
			get => ActiveSortOrder.CurrentSortOrder;
			set => ActiveSortOrder.CurrentSortOrder = value;
		}

		/// <summary>
		/// The ActiveSortOrder property.
		/// </summary>
		public SortPairing ActiveSortOrder { get; set; }

		/// <summary>
		/// The currently active sort type
		/// </summary>
		public SortType ActiveSortType { get; set; }

		/// <summary>
		/// The SortPairing class encapsulates a pair of related sort orders that toggled between by the user
		/// </summary>
		public class SortPairing
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
	}
}
