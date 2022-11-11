namespace CoreMP
{
	/// <summary>
	/// The enum used to select a sort order
	/// </summary>
	public enum SortOrder
	{
		alphaAscending = 0, alphaDescending = 1, idAscending = 2, idDescending = 3, yearAscending = 4, yearDescending = 5,
		genreAscending = 6, genreDescending = 7
	};

	/// <summary>
	/// The enum identifying the types of sorting possible
	/// </summary>
	public enum SortType { alphabetic, identity, year, genre };
}
