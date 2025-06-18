namespace DBTest
{
	internal static class NowPlayingAdapterModel
	{
		/// <summary>
		/// The index of the song currently being played
		/// </summary>
		public static int SongPlayingIndex { get; set; } = -1;

		/// <summary>
		/// Is the selected song currently being played
		/// </summary>
		public static bool IsPlaying { get; set; } = false;

		/// <summary>
		/// The common model components
		/// </summary>
		public static ExpandableListAdapterModel BaseModel { get; } = new ExpandableListAdapterModel();
	}
}
