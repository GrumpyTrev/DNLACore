using SQLite;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Playback class holds the Playback details read from storage
	/// </summary>
	public partial class Playback
	{
		/// <summary>
		/// Get the Playback object from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( PlaybackInstance == null )
			{
				PlaybackInstance = ( await DbAccess.LoadAsync<Playback>() )[0];
			}
		}

		/// <summary>
		/// Access the Playback SongIndex
		/// </summary>
		[Ignore]
		public static int SongIndex
		{
			get => PlaybackInstance.DBSongIndex;

			set
			{
				PlaybackInstance.DBSongIndex = value;

				// No need to wait for the update to complete
				DbAccess.UpdateAsync( PlaybackInstance );

				// Inform controllers about this
				new SongSelectedMessage().Send();
			}
		}

		/// <summary>
		/// Access the Playback PlaybackDeviceName
		/// </summary>
		[Ignore]
		public static string PlaybackDeviceName
		{
			get => PlaybackInstance.DBPlaybackDeviceName;
			set
			{
				PlaybackInstance.DBPlaybackDeviceName = value;

				// No need to wait for the update to complete
				DbAccess.UpdateAsync( PlaybackInstance );
			}
		}

		/// <summary>
		/// Access the Playback LibraryId
		/// </summary>
		[Ignore]
		public static int LibraryId
		{
			get => PlaybackInstance.DBLibraryId;
			set
			{
				PlaybackInstance.DBLibraryId = value;

				// No need to wait for the update to complete
				DbAccess.UpdateAsync( PlaybackInstance );
			}
		}

		/// <summary>
		/// Access the Playback RepeatOn
		/// </summary>
		[Ignore]
		public static bool RepeatPlayOn
		{
			get => PlaybackInstance.DBRepeatPlayOn;
			set
			{
				PlaybackInstance.DBRepeatPlayOn = value;

				// No need to wait for the update to complete
				DbAccess.UpdateAsync( PlaybackInstance );
			}
		}

		/// <summary>
		/// Access the Playback ShufflePlayOn
		/// </summary>
		[Ignore]
		public static bool ShufflePlayOn
		{
			get => PlaybackInstance.DBShufflePlayOn;
			set
			{
				PlaybackInstance.DBShufflePlayOn = value;

				// No need to wait for the update to complete
				DbAccess.UpdateAsync( PlaybackInstance );
			}
		}

		/// <summary>
		/// Access the Playback AutoPlayOn
		/// </summary>
		[Ignore]
		public static bool AutoPlayOn
		{
			get => PlaybackInstance.DBAutoPlayOn;
			set
			{
				PlaybackInstance.DBAutoPlayOn = value;

				// No need to wait for the update to complete
				DbAccess.UpdateAsync( PlaybackInstance );
			}
		}

		/// <summary>
		/// The Playback object read from storage
		/// </summary>
		[Ignore]
		private static Playback PlaybackInstance { get; set; } = null;
	}
}