using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The AutoplayController class is the controller for the AutoplayManagement
	/// </summary>
	static class AutoplayController
	{
		/// <summary>
		/// Get the Autoplay data associated with the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		public static void GetAutoplays( int libraryId )
		{
			// Check if the Artist details for the library have already been obtained
			if ( AutoplayModel.LibraryId != libraryId )
			{
				// New data is required
				AutoplayModel.LibraryId = libraryId;

				// All Artists are read as part of the storage data. So wait until that is available and then carry out the rest of the 
				// initialisation
				StorageController.RegisterInterestInDataAvailable( StorageDataAvailable );
			}
		}

		/// <summary>
		/// Called during startup, or library change, when the storage data is available
		/// </summary>
		/// <param name="message"></param>
		private static void StorageDataAvailable( object message ) =>
			AutoplayModel.CurrentAutoplay = Autoplays.AutoplayCollection.Where( auto => auto.LibraryId == AutoplayModel.LibraryId ).FirstOrDefault();
	}
}