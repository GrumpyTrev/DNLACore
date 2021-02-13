namespace DBTest
{
	/// <summary>
	/// The DisplayGenreController is used to access and update the DisplayGenre flag
	/// </summary>
	class DisplayGenreController
	{
		/// <summary>
		/// Public constructor to allow permanent message registrations
		/// </summary>
		static DisplayGenreController()
		{
		}

		/// <summary>
		/// Get the display genre flag 
		/// </summary>
		public static void GetControllerData() => dataReporter.GetData();

		/// <summary>
		/// Update the state of the DisplayGenre flag
		/// </summary>
		public static bool DisplayGenre
		{
			set
			{
				Playback.DisplayGenre = value;
				StorageDataAvailable();
			}
		}

		/// <summary>
		/// Called during startup when the storage data is available
		/// </summary>
		private static void StorageDataAvailable() => DisplayGenreViewModel.DisplayGenre = Playback.DisplayGenre;

		/// <summary>
		/// The DataReporter instance used to handle storage availability reporting
		/// </summary>
		private static readonly DataReporter dataReporter = new DataReporter( StorageDataAvailable );
	}
}