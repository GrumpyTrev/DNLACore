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
		public SummaryDetailsDisplayController()
		{
			NotificationHandler.Register( typeof( StorageController ), () =>
			{
				UpdateSummaryModel();
				NotificationHandler.Register( typeof( Playback ), "LibraryIdentity", UpdateSummaryModel );
				NotificationHandler.Register( typeof( PlaybackSelectionModel ), UpdateSummaryModel );
			} );
		}

		/// <summary>
		/// Called during startup when the storage data is available and when specific model items have changed
		/// </summary>
		private void UpdateSummaryModel()
		{
			SummaryDisplayViewModel.LibraryName = Libraries.GetLibraryById( Playback.LibraryIdentity ).Name;
			SummaryDisplayViewModel.PlaybackName = PlaybackSelectionModel.SelectedDeviceName;
		}
	}
}
