using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The PlaybackMonitor monitors the availability of the currently selected playback device and of the wifi network and
	/// displays a summary as an icon
	/// </summary>
	internal class PlaybackMonitor : BaseBoundControl
	{
		/// <summary>
		/// Bind to the specified menu item.
		/// Replace the standard view associated with the menu item with out own reduced margin version
		/// Store the AppCompatImageButton from the view
		/// </summary>
		/// <param name="menu"></param>
		public override void BindToMenu( IMenu menu, Context context, View activityContent )
		{
			if ( menu != null )
			{
				// Find the playback_info menu item if it exists
				IMenuItem boundMenuItem = menu.FindItem( Resource.Id.select_playback_device );
				if ( boundMenuItem != null )
				{
					_ = boundMenuItem.SetActionView( Resource.Layout.toolbarButton );
					imageButton = boundMenuItem.ActionView.FindViewById<AppCompatImageButton>( Resource.Id.toolbarSpecialButton );

					// Create a Popup for this button and route it's selections to the CommandRouter
					popupMenu = new PopupMenu( context, imageButton );
					popupMenu.Inflate( Resource.Menu.menu_playback_options );
					popupMenu.MenuItemClick += ( _, args ) => CommandRouter.HandleCommand( args.Item.ItemId );

					// Show the popup when the button is selected
					imageButton.Click += ( _, _ ) => popupMenu.Show();

					DisplayMonitorIcon();

					// Register interest in PlaybackSelectionModel changes
					NotificationHandler.Register( typeof( PlaybackSelectionModel ), PlaybackModelChanged );
				}
			}
			else
			{
				imageButton = null;
				popupMenu = null;

				// Deregister interest
				NotificationHandler.Deregister();
			}
		}

		/// <summary>
		/// Called when something in the PlaybackSelectionModel changes
		/// </summary>
		private void PlaybackModelChanged()
		{
			// Determine the playbackState from the PlaybackSelectionModel
			// First, is the selected device local or remote. Assume local if not set yet
			if ( ( PlaybackSelectionModel.SelectedDeviceName.Length == 0 ) || 
				 ( ( PlaybackSelectionModel.SelectedDevice != null ) && ( PlaybackSelectionModel.SelectedDevice.IsLocal == true ) ) )
			{
				// Local device
				playbackState = ( PlaybackSelectionModel.WifiAvailable == true ) ? PlaybackStateEnum.localPlaybackWifi : PlaybackStateEnum.localPlaybackNoWifi;
			}
			else
			{
				// Remote device - is it available
				if ( PlaybackSelectionModel.WifiAvailable == true )
				{
					playbackState = ( PlaybackSelectionModel.SelectedDevice != null ) ? PlaybackStateEnum.remotePlaybackWifi : PlaybackStateEnum.remotePlaybackNotAvailableWifi;
				}
				else
				{
					playbackState = PlaybackStateEnum.remotePlaybackNoWifi;
				}
			}

			DisplayMonitorIcon();
		}

		/// <summary>
		/// Display the icon associated with the current monitored state
		/// </summary>
		private void DisplayMonitorIcon() => imageButton?.SetImageResource( SelectedResource );

		/// <summary>
		/// Get the resource associated with the current monitor state
		/// </summary>
		private int SelectedResource => resources[ ( int )playbackState ];

		/// <summary>
		/// The enum of all playback states
		/// </summary>
		private enum PlaybackStateEnum
		{
			localPlaybackNoWifi = 0, localPlaybackWifi = 1, remotePlaybackNotAvailableWifi = 2, remotePlaybackNoWifi = 3, remotePlaybackWifi = 4
		};

		/// <summary>
		/// The current state of the playback syatem
		/// </summary>
		private PlaybackStateEnum playbackState = PlaybackStateEnum.localPlaybackNoWifi;

		/// <summary>
		/// The resource ids representing icons to be displayed 
		/// </summary>
		private readonly int[] resources = [ Resource.Drawable.local_playback_no_wifi, Resource.Drawable.local_playback_wifi,
			Resource.Drawable.remote_playback_na_wifi, Resource.Drawable.remote_playback_no_wifi, Resource.Drawable.remote_playback_wifi
		];

		/// <summary>
		/// The button (icon) item that this monitor is bound to
		/// </summary>
		private AppCompatImageButton imageButton = null;

		/// <summary>
		/// The PopupMenu used to display the menu options available from the icon
		/// </summary>
		private PopupMenu popupMenu = null;
	}
}
