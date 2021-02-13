using System;

namespace DBTest
{
	/// <summary>
	/// The DataReporter class is used by controllers to register interest in the storage availability message and to report the availability back
	/// </summary>
	class DataReporter
	{
		/// <summary>
		/// Public constructor. Save the controller method to call when data is available
		/// </summary>
		/// <param name="dataAvailableCallback"></param>
		public DataReporter( Action dataAvailableCallback ) => availabilityAction = dataAvailableCallback;

		/// <summary>
		/// A request to obtain the data associated with the controller.
		/// Register interest in the storage data being available
		/// </summary>
		public void GetData()
		{
			// Make sure that this data is not returned until all of it is available
			dataAvailable = false;

			// Wait until all relevant data has been read
			StorageController.RegisterInterestInDataAvailable( StorageDataAvailable );
		}

		/// <summary>
		/// Called when the storage data is available.
		/// Let the controller perform its own processing and then report the event
		/// </summary>
		/// <param name="_"></param>
		private void StorageDataAvailable( object _ = null )
		{
			// The data is now valid
			dataAvailable = true;

			// Call the delegate provided by the controller
			availabilityAction?.Invoke();
		}

		/// <summary>
		/// Flag used to ensure that data is only reported when it is available
		/// </summary>
		protected bool dataAvailable = false;

		/// <summary>
		/// The interface instance used to report back controller results
		/// </summary>
		public IReporter Reporter
		{
			get => reporter;
			set
			{
				// Save the interface and report back the data if available
				reporter = value;
				if ( dataAvailable == true )
				{
					Reporter?.DataAvailable();
				}
			}
		}

		/// <summary>
		/// The controller method to call then the data is available
		/// </summary>
		private readonly Action availabilityAction = null;

		/// <summary>
		/// The interface instance
		/// </summary>
		private IReporter reporter = null;

		/// <summary>
		/// The interface used to report back controller results.
		/// Controllers can derive their reporter interfaces from this to extend the reporting capabilities
		/// </summary>
		public interface IReporter
		{
			void DataAvailable();
		}
	}
}