﻿using System;
using Android.App;
using Android.Views;
using static Android.Widget.MediaController;

namespace DBTest
{
	/// <summary>
	/// The PlaybackRouter is responsible for routing playback instruction to a particular playback device according to the 
	/// current selection
	/// </summary>
	class PlaybackRouter: Java.Lang.Object, PlaybackManagementController.IReporter, IMediaPlayerControl, PlaybackConnection.IConnectionCallbacks
	{
		/// <summary>
		/// PlaybackRouter constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public PlaybackRouter( Activity bindContext, View anchorView )
		{
			contextForBinding = bindContext;
			anchor = anchorView;
		}

		/// <summary>
		/// Initialise the Router by creating the local and remote connections and accessing the playback data 
		/// </summary>
		public void StartRouter()
		{
			localConnection = new PlaybackConnection( typeof( LocalPlaybackService ), contextForBinding, this );
			localConnection.StartConnection();

			remoteConnection = new PlaybackConnection( typeof( RemotePlaybackService ), contextForBinding, this );
			remoteConnection.StartConnection();

			// Initialise the PlaybackManagementController and request the playlist and current song details
			PlaybackManagementController.Reporter = this;
			PlaybackManagementController.GetMediaControlDataAsync( ConnectionDetailsModel.LibraryId );
		}

		/// <summary>
		/// Called when the owner of this router is being closed down.
		/// Pass this request on to the connections
		/// </summary>
		/// <param name="permanentStop"></param>
		public void StopRouter( bool permanentStop )
		{
			localConnection.StopConnection( permanentStop );
			remoteConnection.StopConnection( permanentStop );

			// As this instance is being destroyed don't leave any references hanging around
			PlaybackManagementController.Reporter = null;
		}

		/// <summary>
		/// Called when the media data has been received or updated
		/// If the playlist songs have been replaced then treat this as a request to play the first of the songs
		/// </summary>
		/// <param name="songsReplaced"></param>
		public void MediaControlDataAvailable( bool songsReplaced )
		{
			if ( ( songsReplaced == true ) && ( PlaybackManagerModel.NowPlayingPlaylist.PlaylistItems.Count > 0 ) )
			{
				PlaybackManagementController.SetSelectedSong( 0 );
			}

			// Pass on the media data to all connections
			localConnection.MediaControlDataAvailable();
			remoteConnection.MediaControlDataAvailable();

			// If a playback device has already been selected in the model but not in this instance then select it now
			if ( ( PlaybackManagerModel.AvailableDevice != null ) && ( selectedConnection == null ) )
			{
				SelectPlaybackDevice( null );
			}

			// If songs have been replaced then tell the selected connection to start playing the current song
			if ( ( songsReplaced == true ) && ( PlaybackManagerModel.NowPlayingPlaylist.PlaylistItems.Count > 0 ) )
			{
				selectedConnection?.Play();
			}
		}

		/// <summary>
		/// Called when the Now Playing playlist is cleared.
		/// Stop the selected controller playing
		/// </summary>
		public void SongsCleared() => selectedConnection?.Stop();

		/// <summary>
		/// Called when the selected song index has been changed via the UI
		/// </summary>
		/// <param name="oldIndex"></param>
		public void SongSelected( int oldIndex )
		{
			// Make sure the connections pass the index on to thier services
			localConnection.SongSelected();
			remoteConnection.SongSelected();

			// Has the index changed
			if ( oldIndex != PlaybackManagerModel.CurrentSongIndex )
			{
				// If the new index is not set (-1) then tell the selected connection
				if ( PlaybackManagerModel.CurrentSongIndex == -1 )
				{
					selectedConnection?.Stop();
				}
				else
				{
					// If the old index was valid then this is a change and should be treated as a request to start playing
					if ( oldIndex != -1 )
					{
						selectedConnection?.Stop();
						selectedConnection?.Play();
					}
				}
			}
		}

		/// <summary>
		/// Called when the selected device is available
		/// Use the device details to switch connections
		/// </summary>
		/// <param name="oldSelectedDevice"></param>
		public void SelectPlaybackDevice( Device oldSelectedDevice )
		{
			if ( oldSelectedDevice != null )
			{
				// Deselect the old connection
				selectedConnection?.DeselectController();
			}

			if ( PlaybackManagerModel.AvailableDevice.IsLocal == true )
			{
				selectedConnection = localConnection;
			}
			else
			{
				selectedConnection = remoteConnection;
			}

			selectedConnection.SelectController();

			if ( mediaController == null )
			{
				SetController();
			}

			// Only show the Media Controller if it has not been previously hidden
			if ( PlaybackManagerModel.MediaControllerVisible == true )
			{
				mediaController.Show();
			}
			else
			{
				mediaController.Visibility = ViewStates.Gone;
			}
		}

		/// <summary>
		/// Called when the Selected connection's service has connected
		/// </summary>
		/// <param name="connection"></param>
		public void ServiceConnected( PlaybackConnection connection )
		{
			if ( connection == selectedConnection )
			{
				if ( PlaybackManagerModel.MediaControllerVisible == true )
				{
					mediaController.Show();
				}
			}
		}

		/// <summary>
		/// Can the selected connection be paused
		/// </summary>
		/// <returns></returns>
		public bool CanPause() => selectedConnection?.CanPause() ?? false;

		/// <summary>
		/// Does the selected connection support seeking forward
		/// </summary>
		/// <returns></returns>
		public bool CanSeekBackward() => selectedConnection?.CanSeekBackward() ?? false;

		/// <summary>
		/// Does the selected connection support seeking backward
		/// </summary>
		/// <returns></returns>
		public bool CanSeekForward() => selectedConnection?.CanSeekForward() ?? false;

		/// <summary>
		/// Pause the selected connection
		/// </summary>
		public void Pause() => selectedConnection?.Pause();

		/// <summary>
		/// Seek to the specified position
		/// </summary>
		/// <param name="pos"></param>
		public void SeekTo( int pos ) => selectedConnection?.SeekTo( pos );

		/// <summary>
		/// Start or resume playback
		/// </summary>
		public void Start() => selectedConnection?.Start();

		/// <summary>
		/// Called when the service has changed the song index
		/// Pass this on to the controller
		/// </summary>
		public void SongIndexChanged( int songIndex )
		{
			contextForBinding.RunOnUiThread( () => { PlaybackManagementController.SetSelectedSong( songIndex ); } );
		}

		/// <summary>
		/// Are the playback controls currently visible
		/// </summary>
		public bool PlaybackControlsVisible
		{
			get
			{
				bool visible = false;

				if ( ( mediaController != null ) && ( mediaController.Visibility == ViewStates.Visible ) )
				{
					visible = true;
				}

				return visible;
			}

			set
			{
				// If this is a change and the controls are being shown then make the controller visible and record this in the model
				if ( PlaybackControlsVisible != value )
				{
					if ( value == true )
					{
						if ( mediaController != null )
						{
							mediaController.Visibility = ViewStates.Visible;
							mediaController.Show( 0 );
							PlaybackManagerModel.MediaControllerVisible = true;
						}
					}
				}
			}
		}

		/// <summary>
		/// Initialise the MediaController component
		/// </summary>
		private void SetController()
		{
			mediaController = new MediaControllerNoHide( contextForBinding );

			mediaController.SetPrevNextListeners(
				new ClickHandler() { OnClickAction = () => { PlayNext(); } },
				new ClickHandler() { OnClickAction = () => { PlayPrevious(); } } );
			mediaController.SetMediaPlayer( this );

			mediaController.SetAnchorView( anchor );

			mediaController.Enabled = true;
		}

		/// <summary>
		/// Play the next track
		/// </summary>
		private void PlayNext()
		{
			selectedConnection?.PlayNext();
		}

		/// <summary>
		/// Play the previous track
		/// </summary>
		private void PlayPrevious()
		{
			selectedConnection?.PlayPrevious();
		}

		/// <summary>
		/// Called when the playback has started
		/// </summary>
		public void PlayStateChanged()
		{
			if ( mediaController != null )
			{
				contextForBinding.RunOnUiThread( () => { mediaController?.Show(); } );
			}
		}

		/// <summary>
		/// Class required to implement the View.IOnClickListener interface
		/// </summary>
		private class ClickHandler: Java.Lang.Object, View.IOnClickListener
		{

			/// <summary>
			/// Called when a click has been detected
			/// </summary>
			/// <param name="v"></param>
			public void OnClick( View v )
			{
				OnClickAction();
			}

			/// <summary>
			/// The Action to be performed when a click has been detected
			/// </summary>
			public Action OnClickAction;
		}

		/// <summary>
		/// Get the audio session id for the connection
		/// </summary>
		public int AudioSessionId => throw new NotImplementedException();

		/// <summary>
		/// Buffer percentage - not used
		/// </summary>
		public int BufferPercentage => 0;

		/// <summary>
		/// The current playback position in milliseconds
		/// </summary>
		public int CurrentPosition => selectedConnection?.CurrentPosition ?? 0;

		/// <summary>
		/// The total duration of the track in milliseconds
		/// </summary>
		public int Duration => selectedConnection?.Duration ?? 0;

		/// <summary>
		/// Is the track being played
		/// </summary>
		public bool IsPlaying => selectedConnection?.IsPlaying ?? false;

		/// <summary>
		/// The context to pass on to the PlaybackConnections to bind their services
		/// </summary>
		private readonly Activity contextForBinding = null;

		/// <summary>
		/// The View to be used to anchor the MediaControllers
		/// </summary>
		private readonly View anchor = null;

		/// <summary>
		/// The connection to the local playback service
		/// </summary>
		private PlaybackConnection localConnection = null;

		/// <summary>
		/// The connection to the remote (DLNA) playback service
		/// </summary>
		private PlaybackConnection remoteConnection = null;

		/// <summary>
		/// The currently selected PlaybackConnection
		/// </summary>
		private PlaybackConnection selectedConnection = null;

		/// <summary>
		/// The MediaController to use to display the UI
		/// </summary>
		private MediaControllerNoHide mediaController = null;
	}
}