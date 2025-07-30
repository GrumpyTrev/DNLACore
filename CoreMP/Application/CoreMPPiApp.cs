using System;
using System.Threading.Tasks;

namespace CoreMP
{
	public class CoreMPPiApp
	{
		/// <summary>
		/// Base constructor
		/// </summary>
		public CoreMPPiApp() { }

		/// <summary>
		/// Save the interface provided by the UI implementation
		/// </summary>
		/// <param name="reporter"></param>
		public void SetInterface( ICoreMP uiInterface )
		{
			coreInterface = uiInterface;
			Logger.Reporter = uiInterface;
		}

		/// <summary>
		/// Initialise the CoreMP library
		/// </summary>
		public async Task Initialise()
		{
			new ConnectionController().InitialiseConnection( coreInterface.StoragePath );
			await StorageController.ReadManagedCollections( new SQLiteStorage() );

			// Trigger the device discovery
			deviceDiscoverer.OnWiFiStateChanged( true );
		}

		/// <summary>
		/// Post an Action onto the UI thread
		/// </summary>
		/// <param name="actionToPost"></param>
		public static void Post( Action actionToPost ) => coreInterface.PostAction( actionToPost );

		/// <summary>
		/// The DeviceDiscovery instance used to monitor the network and scan for DLNA devices
		/// </summary>
		private static readonly DeviceDiscovery deviceDiscoverer = new DeviceDiscovery();

		/// <summary>
		/// The interface used to access the UI system
		/// </summary>
		private static ICoreMP coreInterface = null;
	}
}
