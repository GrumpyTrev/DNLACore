using SQLite;
using System.Threading.Tasks;

namespace CoreMP
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				DbAccess.UpdateAsync( PlaybackInstance );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				DbAccess.UpdateAsync( PlaybackInstance );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				DbAccess.UpdateAsync( PlaybackInstance );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				DbAccess.UpdateAsync( PlaybackInstance );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				DbAccess.UpdateAsync( PlaybackInstance );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}

		/// <summary>
		/// The Playback object read from storage
		/// </summary>
		[Ignore]
		private static Playback PlaybackInstance { get; set; } = null;
	}
}
