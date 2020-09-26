using Android.Support.V4.App;
using System.Collections.Generic;


namespace DBTest
{
	/// <summary>
	/// The CommandRouter class is used to route commands issued by the user to specific command handlers
	/// </summary>
	static class CommandRouter
	{
		public static void BindHandlers()
		{
			new ScanLibraryCommandHandler().BindToRouter();
			new SelectLibraryCommandHandler().BindToRouter();
			new ClearLibraryCommandHandler().BindToRouter();
			new EditLibraryCommandHandler().BindToRouter();
			new SelectDeviceCommandHandler().BindToRouter();
			new ShuffleNowPlayingCommandHandler().BindToRouter();
		}

		public static void BindHandler( int commandIdentity, CommandHandler commandToBind )
		{
			router[ commandIdentity ] = commandToBind;
		}

		public static bool HandleCommand( int commandIdentity )
		{
			bool commandHandled = false;

			CommandHandler handler = router.GetValueOrDefault( commandIdentity );
			if ( handler != null )
			{
				commandHandled = true;
				handler.HandleCommand( commandIdentity );
			}

			return commandHandled;
		}

		public static FragmentManager Manager { get; set; } = null;
		
		private static Dictionary<int, CommandHandler> router = new Dictionary<int, CommandHandler>();
	}
}