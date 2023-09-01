using SQLite;

namespace CoreMP
{
	/// <summary>
	/// The Playback class specifies which song in which library is currently being played, and on which device playback is currently routed
	/// It also holds play mode features such as repeat, shuffle and auto. 
	/// </summary>
	[Table( "Playback" )]
	public class SQLitePlayback : Playback
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public override int LibraryId
		{
			get => libraryId;
			set
			{
				libraryId = value;

				Update();
			}
		}
		private int libraryId = -1;

		/// <summary>
		/// Access the Playback PlaybackDeviceName
		/// </summary>
		public override string PlaybackDeviceName
		{
			get => playbackDeviceName;
			set
			{
				playbackDeviceName = value;

				Update();
			}
		}
		private string playbackDeviceName = "";

		/// <summary>
		/// Access the Playback RepeatOn
		/// </summary>
		public override bool RepeatPlayOn
		{
			get => repeatPlayOn;
			set
			{
				repeatPlayOn = value;

				Update();
			}
		}
		private bool repeatPlayOn = false;

		/// <summary>
		/// Access the Playback ShufflePlayOn
		/// </summary>
		public override bool ShufflePlayOn
		{
			get => shufflePlayOn;
			set
			{
				shufflePlayOn = value;

				Update();
			}
		}
		private bool shufflePlayOn = false;

		/// <summary>
		/// Access the Playback AutoPlayOn
		/// </summary>
		public override bool AutoPlayOn
		{
			get => autoPlayOn;
			set
			{
				autoPlayOn = value;

				Update();
			}
		}
		private bool autoPlayOn = false;

		/// <summary>
		/// Update this instances in the database if loading is not in progress
		/// </summary>
		private void Update()
		{
			if ( StorageController.Loading == false )
			{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				DbAccess.UpdateAsync( this );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
		}
	}
}
