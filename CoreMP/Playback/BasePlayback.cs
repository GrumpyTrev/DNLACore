using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CoreMP
{
	/// <summary>
	/// Base functionality for a playback service
	/// </summary>
	public abstract class BasePlayback
	{
		/// <summary>
		/// Start the position check timer. This will only be processed by derived classes if they are playing
		/// </summary>
		public BasePlayback() => positionTimer = new Timer( timer => PositionTimerElapsed(), null, TimerPeriod, TimerPeriod );

		/// <summary>
		/// Called when the playback system is being shutdown.
		/// Allow derived classes to release system resources
		/// </summary>
		public void StopConnection()
		{
			Reporter = null;

			Shutdown();
		}

		/// <summary>
		/// Called when this instance is selected for playback
		/// </summary>
		public void Select()
		{
			PlaybackDevice = PlaybackManagerModel.AvailableDevice;

			treatResumeAsPlay = true;
		}

		/// <summary>
		/// Called when this instance is deselected
		/// </summary>
		public void Deselect()
		{
			Stop();
			Reset();
		}

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
				Resume();
			}
		}

		/// <summary>
		/// Play the currently selected song
		/// </summary>
		public virtual void Play() => treatResumeAsPlay = false;

		/// <summary>
		/// Stop playing the current song
		/// </summary>
		public abstract void Stop();

		/// <summary>
		/// Pause playing the current song
		/// </summary>
		public abstract void Pause();

		/// <summary>
		/// Resume playing the current song
		/// </summary>
		public abstract void Resume();

		/// <summary>
		/// Reset the player
		/// </summary>
		public abstract void Reset();

		/// <summary>
		/// Called when the associated application is shutting down.
		/// Carry out any final actions
		/// </summary>
		public abstract void Shutdown();

		/// <summary>
		/// Get the current position of the playing song
		/// </summary>
		public abstract int CurrentPosition { get; }

		/// <summary>
		/// Get the duration of the current song
		/// </summary>
		public abstract int Duration { get; }

		/// <summary>
		/// Is a song currently being played
		/// </summary>
		public virtual bool IsPlaying
		{
			get => playing;

			set
			{
				if ( playing != value )
				{
					playing = value;
					Reporter?.PlayStateChanged( playing );
				}
			}
		}

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
		/// Called when the position timer has elapsed
		/// The base class response is to report back the current duration and position
		/// Derived class can perform other processing - such as actually obtaining the duration and position
		/// </summary>
		protected virtual void PositionTimerElapsed()
		{
			if ( IsPlaying == true )
			{
				Reporter?.ProgressReport( CurrentPosition, Duration );
			}
		}

		/// <summary>
		/// Start the progress timer
		/// </summary>
		protected void StartTimer() => positionTimer.Change( TimerPeriod, TimerPeriod );

		/// <summary>
		/// Stop the progress timer
		/// </summary>
		protected void StopTimer() => positionTimer.Change( Timeout.Infinite, Timeout.Infinite );

		/// <summary>
		/// Get the source path for the currently playing song
		/// </summary>
		/// <returns></returns>
		protected string GetSongResource( bool local )
		{
			string resource = "";

			if ( PlaybackManagerModel.CurrentSong != null )
			{
				// Find the Source associated with this song
				Source songSource = PlaybackManagerModel.Sources.SingleOrDefault( d => ( d.Id == PlaybackManagerModel.CurrentSong.SourceId ) );

				if ( songSource != null )
				{
					resource = FormSourceName( songSource, PlaybackManagerModel.CurrentSong.Path, local );
				}
			}

			return resource;
		}

		/// <summary>
		/// Form the name for the song depending on the source type 
		/// </summary>
		/// <param name="songSource"></param>
		/// <param name="songPath"></param>
		/// <returns></returns>
		protected string FormSourceName( Source songSource, string songPath, bool local )
		{
			string sourceName = "";

			// Playing back locally?
			if ( local == true )
			{
				// Source name depends on the source's access method
				switch ( songSource.AccessMethod )
				{
					case Source.AccessType.Local:
					{
						sourceName = Path.Combine( songSource.LocalAccess, songPath.TrimStart( '/' ) );
						break;
					}
					case Source.AccessType.FTP:
					{
						sourceName = Path.Combine( songSource.LocalAccess, Uri.EscapeDataString( songPath.TrimStart( '/' ) ) );
						break;
					}
					case Source.AccessType.UPnP:
					{
						// Find the device assoicated with the source
						PlaybackDevice sourceDevice = DevicesModel.RemoteDevices.FindServer( songSource.Name );
						sourceName = $"http://{sourceDevice.IPAddress}:{sourceDevice.Port}/{songPath}";
						break;
					}
					default:
					break;
				}
			}
			else
			{
				// Playing a song on a remote device. Need to escape the song's path, except for UPnP where the path is escaped already
				if ( songSource.AccessMethod == Source.AccessType.UPnP )
				{
					// Find the device assoicated with the source
					PlaybackDevice sourceDevice = DevicesModel.RemoteDevices.FindServer( songSource.Name );
					sourceName = $"http://{sourceDevice.IPAddress}:{sourceDevice.Port}/{songPath}";
				}
				else
				{
					sourceName = Path.Combine( songSource.RemoteAccess, Uri.EscapeDataString( songPath.TrimStart( '/' ) ) );
				}
			}

			return sourceName;
		}

		/// <summary>
		/// Report that the current song is being played
		/// </summary>
		protected void ReportSongStarted() => Reporter?.SongStarted();

		/// <summary>
		/// Report that the current song has finished
		/// </summary>
		protected void ReportSongFinished() => Reporter?.SongFinished();

		/// <summary>
		/// The instance used to report back significant events
		/// </summary>
		public IPlaybackCallbacks Reporter { get; set; } = null;

		/// <summary>
		/// Details of the playback device at the time of selection
		/// </summary>
		protected PlaybackDevice PlaybackDevice { get; set; } = null;

		/// <summary>
		/// Is the service currently playing
		/// </summary>
		private bool playing = false;

		/// <summary>
		/// At startup the Resume button should be treated as Play.
		/// </summary>
		private bool treatResumeAsPlay = false;

		/// <summary>
		/// The timer used to check the progress of the song
		/// </summary>
		private readonly Timer positionTimer = null;

		/// <summary>
		/// The position timer period in milliseconds
		/// </summary>
		private const int TimerPeriod = 1000;

		/// <summary>
		/// The interface defining the calls back to the application
		/// </summary>
		public interface IPlaybackCallbacks
		{
			void PlayStateChanged( bool isPlaying );
			void SongStarted();
			void SongFinished();
			void ProgressReport( int position, int duration );
		}
	}
}
