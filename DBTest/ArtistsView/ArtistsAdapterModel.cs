namespace DBTest
{
	static class ArtistsAdapterModel
	{
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