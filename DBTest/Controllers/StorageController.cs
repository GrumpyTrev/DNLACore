using System;

namespace DBTest
{
	/// <summary>
	/// The StorageController class is responsible for coordinating the reading from storage collections of data that are not library specific
	/// and are not therefore re-read whenever the library changes. Other controllers can make use of library specific subsets of this data
	/// </summary>
	static class StorageController
	{
		/// <summary>
		/// Called to register interest in the availability of the managed storage collections
		/// </summary>
		/// <param name="callback"></param>
		public static void RegisterInterestInDataAvailable( Action<Object> callback )
		{
			// If the data is available then call the callback
			if ( DataAvailable == true )
			{
				callback( null );
			}
			else
			{
				// Data is not currently available. Register the callback with the StorageDataAvailableMessage
				Mediator.RegisterPermanent( callback, typeof( StorageDataAvailableMessage ) );

				// If the data is not being read then start the read process
				if ( DataBeingRead == false )
				{
					DataBeingRead = true;
					ReadManagedCollections();
				}
			}
		}

		/// <summary>
		/// Read all the managed collections and then tell any registered listeners
		/// </summary>
		private static async void ReadManagedCollections()
		{
			await Genres.GetDataAsync();
			await Albums.GetDataAsync();
			await Sources.GetDataAsync();
			DataAvailable = true;
			new StorageDataAvailableMessage().Send();
		}

		/// <summary>
		/// Is the managed storage available
		/// </summary>
		private static bool DataAvailable { get; set; } = false;

		/// <summary>
		/// If the managed storage currently being read
		/// </summary>
		private static bool DataBeingRead { get; set; } = false;
	}
}