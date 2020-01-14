using System;

namespace DBTest
{
	/// <summary>
	/// The sort selector class associates an array of resource ids with an enum type and allows the resource id to be selected by enum value
	/// </summary>
	class AlbumSortSelector
	{
		/// <summary>
		/// The enum used to select a sort order
		/// </summary>
		public enum AlbumSortOrder { alphaAscending = 0, alphaDescending = 1, idAscending = 2, idDescending = 3 };

		/// <summary>
		/// Select the next sort order
		/// </summary>
		public void SelectNext() => CurrentSortOrder = ( AlbumSortOrder )NextSortOrder;

		/// <summary>
		/// Get the resource associated with the NEXT sort order in the sequence
		/// </summary>
		public int SelectedResource => resources[ NextSortOrder ];

		/// <summary>
		/// The current sort order
		/// </summary>
		public AlbumSortOrder CurrentSortOrder { get; set; } = AlbumSortOrder.alphaAscending;

		/// <summary>
		/// The next sort order index
		/// </summary>
		private int NextSortOrder => ( ( int )CurrentSortOrder + 1 ) == Enum.GetValues( typeof( AlbumSortOrder ) ).Length ? 0 : ( int )CurrentSortOrder + 1;

		/// <summary>
		/// The resource ids representing icons to be displayed 
		/// </summary>
		private readonly int [] resources = new int [] { Resource.Drawable.sort_by_alpha_ascending, Resource.Drawable.sort_by_alpha_descending,
			Resource.Drawable.sort_by_id_ascending, Resource.Drawable.sort_by_id_descending };
	}
}