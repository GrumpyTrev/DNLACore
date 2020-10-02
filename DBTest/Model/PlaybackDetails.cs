using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The PlaybackDetails class holds the Playback details read from storage
	/// </summary>
	public static class PlaybackDetails
	{
		/// <summary>
		/// Get the Playback object from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( PlaybackInstance == null )
			{
				PlaybackInstance = await PlaybackAccess.GetPlaybackAsync();
			}
		}

		/// <summary>
		/// Access the Playback SongIndex
		/// </summary>
		public static int SongIndex
		{
			get => PlaybackInstance?.SongIndex ?? -1;

			set
			{
				if ( PlaybackInstance != null )
				{
					PlaybackInstance.SongIndex = value;

					// No need to wait for the update to complete
					PlaybackAccess.UpdatePlaybackAsync( PlaybackInstance );
				}
			}
		}

		/// <summary>
		/// Access the Playback PlaybackDeviceName
		/// </summary>
		public static string PlaybackDeviceName
		{
			get => PlaybackInstance?.PlaybackDeviceName ?? "";
			set
			{
				if ( PlaybackInstance != null )
				{
					PlaybackInstance.PlaybackDeviceName = value;

					// No need to wait for the update to complete
					PlaybackAccess.UpdatePlaybackAsync( PlaybackInstance );
				}
			}
		}

		/// <summary>
		/// Access the Playback LibraryId
		/// </summary>
		public static int LibraryId
		{
			get => PlaybackInstance?.LibraryId ?? -1;
			set
			{
				if ( PlaybackInstance != null )
				{
					PlaybackInstance.LibraryId = value;

					// No need to wait for the update to complete
					PlaybackAccess.UpdatePlaybackAsync( PlaybackInstance );
				}
			}
		}

		/// <summary>
		/// The Playback object read from storage
		/// </summary>
		private static Playback PlaybackInstance { get; set; } = null;
	}
}