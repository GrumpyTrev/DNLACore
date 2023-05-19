using SQLite;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Playback class specifies which song in which library is currently being played, and on which device playback is currently routed
	/// It also holds play mode features such as repeat, shuffle and auto. 
	/// </summary>
	[Table( "Playback" )]
	public class Playback
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		[Column( "LibraryId" )]
		public int DBLibraryId { get; set; }

		/// <summary>
		/// The name of the currently selected playback device
		/// </summary>
		[Column( "PlaybackDeviceName" )]
		public string DBPlaybackDeviceName { get; set; }

		/// <summary>
		/// Is repeat play on
		/// </summary>
		[Column( "RepeatPlayOn" )]
		public bool DBRepeatPlayOn { get; set; }

		/// <summary>
		/// Is shuffle play on
		/// </summary>
		[Column( "ShufflePlayOn" )]
		public bool DBShufflePlayOn { get; set; }

		/// <summary>
		/// Is auto play on
		/// </summary>
		[Column( "AutoPlayOn" )]
		public bool DBAutoPlayOn { get; set; }
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
