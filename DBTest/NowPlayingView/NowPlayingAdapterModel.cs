namespace DBTest
{
	static class NowPlayingAdapterModel
	{
		/// <summary>
		/// The index of the song currently being played
		/// </summary>
		public static int SongPlayingIndex { get; set; } = -1;

		public static ExpandableListAdapterModel BaseModel { get; } = new ExpandableListAdapterModel();

		/// <summary>
		/// Clear the states held by this model
		/// </summary>
		public static void OnClear()
		{
			BaseModel.OnClear();

			SongPlayingIndex = -1;
		}
	}
}