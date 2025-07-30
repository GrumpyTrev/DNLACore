namespace CoreMP
{
	/// <summary>
	/// The SummaryDetailsDisplayController is used to obtain summary data to be displayed to the user
	/// </summary>
	internal class SummaryDetailsDisplayController
	{
		/// <summary>
		/// Register for the generic StorageController data available as well as specific items
		/// </summary>
		public SummaryDetailsDisplayController() => NotificationHandler.Register<StorageController>( nameof( StorageController.IsSet ), () =>
		{
			UpdateSummaryModel();
			NotificationHandler.Register<Playback>( nameof( Playback.LibraryIdentity ), UpdateSummaryModel );
			NotificationHandler.Register<DevicesModel>( nameof( DevicesModel.SelectedDevice ), UpdateSummaryModel );
		} );

		/// <summary>
		/// Called during startup when the storage data is available and when specific model items have changed
		/// </summary>
		private void UpdateSummaryModel()
		{
			SummaryDisplayViewModel.LibraryName = Libraries.GetLibraryById( Playback.LibraryIdentity ).Name;
			SummaryDisplayViewModel.PlaybackName = DevicesModel.SelectedDevice.FriendlyName.Split(' ')[0];
		}
	}
}
