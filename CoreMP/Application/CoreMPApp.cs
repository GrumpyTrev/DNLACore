using System;

namespace CoreMP
{
	public class CoreMPApp
	{
		/// <summary>
		/// Base constructor
		/// Save a pointer to the one and only CoreMPApp
		/// </summary>
		public CoreMPApp() => Instance = this;

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
		public void Initialise()
		{
			new ConnectionController().InitialiseConnection( coreInterface.StoragePath );
			StorageController.ReadManagedCollections( new SQLiteStorage() );
		}

		/// <summary>
		/// Called when the UI has shutdown
		/// </summary>
		public void Shutdown() => CommandInterface.StopRouter();

		/// <summary>
		/// Pass on wifi state changes to the DeviceDiscoverer instance
		/// </summary>
		/// <param name="wifiAvailable"></param>
		public void WifiStateChanged( bool wifiAvailable ) => deviceDiscoverer.OnWiFiStateChanged( wifiAvailable );

		/// <summary>
		/// Commander class used to pass all commands on to
		/// </summary>
		public Commander CommandInterface { get; } = new Commander();

		public void SetLocalPlayer( BasePlayback localPlayer ) => CommandInterface.SetLocalPlayer( localPlayer );

		/// <summary>
		/// Post an Action onto the UI thread
		/// </summary>
		/// <param name="actionToPost"></param>
		public static void Post( Action actionToPost ) => coreInterface.PostAction( actionToPost );

		public static void AquireWakeLock() => coreInterface.AquireWakeLock();

		public static void ReleaseWakeLock() => coreInterface.ReleaseWakeLock();

		/// <summary>
		/// The one and only CoreMPApp
		/// </summary>
		public static CoreMPApp Instance { get; private set; }

		/// <summary>
		/// The one and only Http server used to serve local files to remote devices
		/// </summary>
#pragma warning disable IDE0052 // Remove unread private members
		private static readonly SimpleHTTPServer localServer = new SimpleHTTPServer( "", 8080 );
#pragma warning restore IDE0052 // Remove unread private members

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
