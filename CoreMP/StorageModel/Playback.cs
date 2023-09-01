using System.Threading.Tasks;

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
		/// The name of the currently selected playback device
		/// </summary>
		public virtual string PlaybackDeviceName { get; set; }

		/// <summary>
		/// Is repeat play on
		/// </summary>
		public virtual bool RepeatPlayOn { get; set; }

		/// <summary>
		/// Is shuffle play on
		/// </summary>
		public virtual bool ShufflePlayOn { get; set; }

		/// <summary>
		/// Is auto play on
		/// </summary>
		public virtual bool AutoPlayOn { get; set; }

		public static void CollectionLoaded() { }

		/// <summary>
		/// Access the Playback PlaybackDeviceName
		/// </summary>
		public static string SingletonPlaybackDeviceName
		{
			get => PlaybackInstance.PlaybackDeviceName;
			set => PlaybackInstance.PlaybackDeviceName = value;
		}

		/// <summary>
		/// Access the Playback LibraryId
		/// </summary>
		public static int SingletonLibraryId
		{
			get => PlaybackInstance.LibraryId;
			set => PlaybackInstance.LibraryId = value;
		}

		/// <summary>
		/// Access the Playback RepeatOn
		/// </summary>
		public static bool SingletonRepeatPlayOn
		{
			get => PlaybackInstance.RepeatPlayOn;
			set => PlaybackInstance.RepeatPlayOn = value;
		}

		/// <summary>
		/// Access the Playback ShufflePlayOn
		/// </summary>
		public static bool SingletonShufflePlayOn
		{
			get => PlaybackInstance.ShufflePlayOn;
			set => PlaybackInstance.ShufflePlayOn = value;
		}

		/// <summary>
		/// Access the Playback AutoPlayOn
		/// </summary>
		public static bool SingletonAutoPlayOn
		{
			get => PlaybackInstance.AutoPlayOn;
			set => PlaybackInstance.AutoPlayOn = value;
		}

		/// <summary>
		/// The Playback object read from storage
		/// </summary>
		public static Playback PlaybackInstance { get; set; } = null;
	}
}
