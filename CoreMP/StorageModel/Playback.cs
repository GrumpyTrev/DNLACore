namespace CoreMP
{
	/// <summary>
	/// The Playback class specifies which song in which library is currently being played, and on which device playback is currently routed
	/// It also holds play mode features such as repeat, shuffle and auto. 
	/// </summary>
	public class Playback
	{
		public virtual int LibraryId { get; set; }

		/// <summary>
		/// Is repeat play on
		/// </summary>
		public virtual bool RepeatPlayOn { get; set; }

		/// <summary>
		/// Is shuffle play on
		/// </summary>
		public virtual bool ShufflePlayOn { get; set; }

		public static void CollectionLoaded() { }

		/// <summary>
		/// Access the Playback LibraryId
		/// </summary>
		public static int LibraryIdentity
		{
			get => PlaybackInstance.LibraryId;
			set
			{
				PlaybackInstance.LibraryId = value;
				NotificationHandler.NotifyPropertyChanged( null );
			}
		}

		/// <summary>
		/// Access the Playback RepeatOn
		/// </summary>
		public static bool RepeatOn
		{
			get => PlaybackInstance.RepeatPlayOn;
			set => PlaybackInstance.RepeatPlayOn = value;
		}

		/// <summary>
		/// Access the Playback ShufflePlayOn
		/// </summary>
		public static bool ShuffleOn
		{
			get => PlaybackInstance.ShufflePlayOn;
			set => PlaybackInstance.ShufflePlayOn = value;
		}

		/// <summary>
		/// The Playback object read from storage
		/// </summary>
		public static Playback PlaybackInstance { get; set; } = null;
	}
}
