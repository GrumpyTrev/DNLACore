using Android.Support.V7.Widget;
using Android.Views;

namespace DBTest
{
	/// <summary>
	/// The PlaybackMonitor monitors the availability of the currently selected playback device and of the wifi network and
	/// displays a summary as an icon
	/// </summary>
	class PlaybackMonitor
	{
		/// <summary>
		/// Get a notiification whenever the PlaybackSelectionModel changes
		/// </summary>
		public PlaybackMonitor() => Mediator.RegisterPermanent( PlaybackModelChanged, typeof( PlaybackModelChangedMessage ) );

		/// <summary>
		/// Bind to the specified menu item.
		/// Replace the standard view associated with the menu item with out own reduced margin version
		/// Store the AppCompatImageButton from the view
		/// </summary>
		/// <param name="menu"></param>
		public void BindToMenu( IMenu menu )
		{
			if ( menu != null )
			{
				// Find the playback_info menu item if it exists
				IMenuItem boundMenuItem = menu.FindItem( Resource.Id.playback_info );
				if ( boundMenuItem != null )
				{
					boundMenuItem.SetActionView( Resource.Layout.toolbarButton );
					imageButton = boundMenuItem.ActionView.FindViewById<AppCompatImageButton>( Resource.Id.toolbarSpecialButton );
					DisplayMonitorIcon();
				}
			}
			else
			{
				imageButton = null;
			}
		}

		/// <summary>
		/// Called when something in the PlaybackSelectionModel changes
		/// </summary>
		/// <param name="message"></param>
		private void PlaybackModelChanged( object message )
		{
			// Determine the playbackState from the PlaybackSelectionModel
			// First, is the selected device local or remote. Assume local if not set yet
			if ( ( PlaybackSelectionModel.SelectedDeviceName.Length == 0 ) || ( PlaybackSelectionModel.SelectedDeviceName == PlaybackSelectionModel.LocalDeviceName ) )
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
		private readonly int[] resources = new int[] { Resource.Drawable.local_playback_no_wifi, Resource.Drawable.local_playback_wifi,
			Resource.Drawable.remote_playback_na_wifi, Resource.Drawable.remote_playback_no_wifi, Resource.Drawable.remote_playback_wifi
		};

		/// <summary>
		/// The button (icon) item that this monitor is bound to
		/// </summary>
		private AppCompatImageButton imageButton = null;
	}
}