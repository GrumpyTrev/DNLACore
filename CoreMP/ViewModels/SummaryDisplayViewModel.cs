namespace CoreMP
{
	/// <summary>
	/// The SummaryDisplayViewModel holds summary details to be displayed to the user
	/// </summary>
	public class SummaryDisplayViewModel
	{
		/// <summary>
		/// The name of the currently selected library
		/// </summary>
		private static string libraryName = "";
		public static string LibraryName
		{
			get => libraryName;
			internal set
			{
				libraryName = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

		/// <summary>
		/// The name of the currently selected playback device
		/// </summary>
		private static string playbackName = "";
		public static string PlaybackName
		{
			get => playbackName;
			internal set
			{
				playbackName = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}
	}
}
