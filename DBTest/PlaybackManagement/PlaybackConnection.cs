using System;
using Android.Content;
using Android.OS;

namespace DBTest
{
	class PlaybackConnection : Java.Lang.Object, IServiceConnection, BasePlaybackService.IServiceCallbacks
	{
		/// <summary>
		/// PlaybackConnection constructor
		/// Save the supplied context for binding later on
		/// Save the anchorView to position the MediaController
		/// </summary>
		/// <param name="bindContext"></param>
		public PlaybackConnection( Type serviceType, Context bindContext, IConnectionCallbacks callBack )
		{
			contextForBinding = bindContext;
			typeForService = serviceType;
			reporter = callBack;
		}

		/// <summary>
		/// Start the service and initiale the MediaController
		/// </summary>
		public void StartConnection()
		{
			// Start the service
			contextForBinding.StartService( new Intent( contextForBinding, typeForService ) );

			// Bind to the service
			contextForBinding.BindService( new Intent( contextForBinding, typeForService ), this, Bind.None );
		}

		/// <summary>
		/// Called when the owner of this connection is being closed down.
		/// The stop can be permanent or final in which case unbind and therefore stop the service
		/// </summary>
		/// <param name="permanentStop"></param>
		public void StopConnection( bool permanentStop )
		{
			// Only access the service if still bound
			if ( playerService != null )
			{
				playerService.Reporter = null;

				if ( permanentStop == true )
				{
					playerService.Shutdown();
					playerService = null;
				}
			}
		}

		/// <summary>
		/// Called when this controller is selected for playback
		/// </summary>
		public void SelectController()
		{
			if ( playerService != null )
			{
				playerService.PlaybackDevice = PlaybackManagerModel.AvailableDevice;
			}

			treatResumeAsPlay = true;

			Selected = true;
		}

		/// <summary>
		/// Called when this controller is deselected
		/// </summary>
		public void DeselectController()
		{
			playerService?.Stop();
			playerService?.Reset();

			Selected = false;
		}

		/// <summary>
		/// Called when the running service has connected to this manager
		/// Retain a reference to the service for commands and provide this instance as the service's callback interface
		/// </summary>
		/// <param name="name"></param>
		/// <param name="service"></param>
		public void OnServiceConnected( ComponentName name, IBinder service )
		{
			playerService = ( ( BasePlaybackService.PlaybackBinder )service ).Service;
			playerService.Reporter = this;

			// If the current playlist has already been obtained then pass it to the service
			if ( PlaybackManagerModel.NowPlayingPlaylist != null )
			{
				MediaControlDataAvailable();
			}

			// If the service has connected after the connection has been selected then must inform the router 
			if ( Selected == true )
			{
				reporter?.ServiceConnected( this );
			}
		}

		/// <summary>
		/// Called when the service has disconnected
		/// This only happens when something unexpected has happened at the service end
		/// </summary>
		/// <param name="name"></param>
		public void OnServiceDisconnected( ComponentName name )
		{
			playerService = null;
		}

		/// <summary>
		/// Called when the Media Control data has been read
		/// Pass the data on to the service if connected
		/// </summary>
		public void MediaControlDataAvailable()
		{
			if ( playerService != null )
			{
				playerService.Playlist = PlaybackManagerModel.NowPlayingPlaylist;
				playerService.Sources = PlaybackManagerModel.Sources;
				playerService.CurrentSongIndex = PlaybackManagerModel.CurrentSongIndex;
				playerService.PlaybackDevice = PlaybackManagerModel.AvailableDevice;
			}
		}

		/// <summary>
		/// Called when the selected song has been changed
		/// Pass it on to the service
		/// </summary>
		public void SongSelected()
		{
			if ( playerService != null )
			{
				playerService.CurrentSongIndex = PlaybackManagerModel.CurrentSongIndex;
			}
		}


		/// <summary>
		/// Play the currently selected song
		/// </summary>
		public void Play()
		{
			treatResumeAsPlay = false;

			playerService?.Play();
		}

		/// <summary>
		/// Stop playing the song
		/// </summary>
		public void Stop() => playerService?.Stop();

		/// <summary>
		/// The current playback position in milliseconds
		/// </summary>
		public int CurrentPosition => playerService?.Position ?? 0;

		/// <summary>
		/// The total duration of the track in milliseconds
		/// </summary>
		public int Duration => playerService?.Duration ?? 0;

		/// <summary>
		/// Is the track being played
		/// </summary>
		public bool IsPlaying => playerService?.IsPlaying ?? false;

		/// <summary>
		/// Can the service be paused
		/// </summary>
		/// <returns></returns>
		public bool CanPause() => true;

		/// <summary>
		/// Does the service support seeking backward
		/// </summary>
		/// <returns></returns>
		public bool CanSeekBackward() => true;

		/// <summary>
		/// Does the service support seeking forward
		/// </summary>
		/// <returns></returns>
		public bool CanSeekForward() => true;

		/// <summary>
		/// Pause the playback
		/// </summary>
		public void Pause() => playerService?.Pause();

		/// <summary>
		/// Seek to the specified position
		/// </summary>
		/// <param name="position"></param>
		public void SeekTo( int position ) => playerService?.Seek( position );

		/// <summary>
		/// Start playback
		/// </summary>
		public void Start()
		{
			if ( treatResumeAsPlay == true )
			{
				Play();
			}
			else
			{
				playerService?.Resume();
			}
		}

		/// <summary>
		/// Called when the service has changed the song index
		/// Pass this on to the controller
		/// </summary>
		public void SongIndexChanged( int songIndex )
		{
			reporter?.SongIndexChanged( songIndex );
		}

		/// <summary>
		/// Called when playback of a song has started.
		/// Report this back
		/// </summary>
		public void PlayStateChanged()
		{
			reporter?.PlayStateChanged();
		}

		/// <summary>
		/// Play the next track
		/// </summary>
		public void PlayNext() => playerService?.PlayNext();

		/// <summary>
		/// Play the previous track
		/// </summary>
		public void PlayPrevious() => playerService?.PlayPrevious();

		/// <summary>
		/// The service carrying out the playback
		/// </summary>
		private BasePlaybackService playerService = null;

		/// <summary>
		/// The context to use to bind the services
		/// </summary>
		private readonly Context contextForBinding = null;

		/// <summary>
		/// The Type of the service to create
		/// </summary>
		private readonly Type typeForService = null;

		/// <summary>
		/// Keep track of the selection state of this connection
		/// </summary>
		private bool Selected { get; set; } = false;

		/// <summary>
		/// At startup the Resume button should be treated as Play.
		/// </summary>
		private bool treatResumeAsPlay = false;

		/// <summary>
		/// The interface used to report significant playback events
		/// </summary>
		private IConnectionCallbacks reporter = null;

		/// <summary>
		/// The interface defining the calls back to the application
		/// </summary>
		public interface IConnectionCallbacks
		{
			void PlayStateChanged();
			void SongIndexChanged( int songIndex );
			void ServiceConnected( PlaybackConnection connection );
		}
	}
}