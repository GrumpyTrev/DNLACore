﻿using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The PlaybackModeView class displays the current playback mode and allows the user to change the playback mode
	/// </summary>
	internal class PlaybackModeView : BaseBoundControl
	{
		/// <summary>
		/// Bind to the specified menu item.
		/// Replace the standard view associated with the menu item with our own reduced margin version
		/// Store the AppCompatImageButton from the view so that the icon can be changed.
		/// Add an event handler for the button.
		/// </summary>
		/// <param name="menu"></param>
		public override void BindToMenu( IMenu menu, Context context, View activityContent )
		{
			if ( menu != null )
			{
				// Find the playback_info menu item if it exists
				IMenuItem boundMenuItem = menu.FindItem( Resource.Id.playback_mode );
				if ( boundMenuItem != null )
				{
					boundMenuItem.SetActionView( Resource.Layout.toolbarButton );
					imageButton = boundMenuItem.ActionView.FindViewById<AppCompatImageButton>( Resource.Id.toolbarSpecialButton );

					// Create a Popup for this button and route it's selections to the CommandRouter
					titlePopup = new PopupMenu( context, imageButton );
					titlePopup.Inflate( Resource.Menu.menu_playback );
					titlePopup.MenuItemClick += ( _, args ) => CommandRouter.HandleCommand( args.Item.ItemId );

					// Show the popup when the button is selected
					imageButton.Click += ( _, _ ) =>
					{
						// Set the submenu item text according to the current state of the individual playback attributes
						titlePopup.Menu.FindItem( Resource.Id.repeat_on_off ).SetTitle( PlaybackModeModel.RepeatOn ? "Repeat off" : "Repeat on" );
						titlePopup.Menu.FindItem( Resource.Id.shuffle_on_off ).SetTitle( PlaybackModeModel.ShuffleOn ? "Shuffle off" : "Shuffle on" );
						titlePopup.Menu.FindItem( Resource.Id.auto_on_off ).SetTitle( PlaybackModeModel.AutoOn ? "Auto off" : "Auto on" );
						titlePopup.Show();
					};

					DisplayPlaybackIcon();
				}

				NotificationHandler.Register( typeof( PlaybackModeModel ), () => DisplayPlaybackIcon() );
			}
			else
			{
				imageButton = null;
				titlePopup = null;

				// Remove any model notifications
				NotificationHandler.Deregister();
			}
		}

		/// <summary>
		/// Display the icon associated with the current playback state
		/// </summary>
		private void DisplayPlaybackIcon() => imageButton?.SetImageResource( SelectedResource );

		/// <summary>
		/// Get the resource associated with the current monitor state
		/// </summary>
		private int SelectedResource => resources[ ( int )PlaybackModeModel.ActivePlayMode ];

		/// <summary>
		/// The resource ids representing icons to be displayed. These aree in the same order as the PlayModeType enum
		/// </summary>
		private readonly int[] resources = new int[] { Resource.Drawable.linear_play, Resource.Drawable.repeat, Resource.Drawable.shuffle,
			Resource.Drawable.repeat_shuffle, Resource.Drawable.auto_play };

		/// <summary>
		/// The button (icon) item that this view is bound to
		/// </summary>
		private AppCompatImageButton imageButton = null;

		/// <summary>
		/// The PopupMenu used to display the playback mode change options
		/// </summary>
		private PopupMenu titlePopup = null;
	}
}
