namespace DBTest
{
	/// <summary>
	/// The AlbumsAdapterModel class holds the current state of the AlbumsAdapter class
	/// </summary>
	static class AlbumsAdapterModel
	{
		/// <summary>
		/// The model for the base ExpandableListAdapter
		/// </summary>
		public static ExpandableListAdapterModel BaseModel { get; } = new ExpandableListAdapterModel();

		/// <summary>
		/// Clear the states held by this model
		/// </summary>
		public static void OnClear()
		{
			BaseModel.OnClear();
		}
	}
}