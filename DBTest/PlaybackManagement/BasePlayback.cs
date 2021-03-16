using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DBTest
{
	/// <summary>
	/// Base functionality for a playback service
	/// </summary>
	public abstract class BasePlayback : Java.Lang.Object
	{
		/// <summary>
		/// Start the position check timer. This will only be processed by derived classes if they are playing
		/// </summary>
		public BasePlayback()
		{
			// Create and start the position/progress timer 
			positionTimer = new Timer( timer => PositionTimerElapsed(), null, TimerPeriod, TimerPeriod );
		}		
		
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
		/// Called when this controller is selected for playback
		/// </summary>
		public void SelectController()
		{
			PlaybackDevice = PlaybackManagerModel.AvailableDevice;

			treatResumeAsPlay = true;

			Selected = true;
		}

		/// <summary>
		/// Called when this controller is deselected
		/// </summary>
		public void DeselectController()
		{
			Stop();
			Reset();

			Selected = false;
		}

		/// <summary>
		/// Called when the Media Control data has been read
		/// Pass the data on to the service if connected
		/// </summary>
		public void MediaControlDataAvailable()
		{
			Playlist = PlaybackManagerModel.NowPlayingPlaylist;
			Sources = PlaybackManagerModel.Sources;
			CurrentSongIndex = PlaybackManagerModel.CurrentSongIndex;
			PlaybackDevice = PlaybackManagerModel.AvailableDevice;
		}

		/// <summary>
		/// Called when the selected song has been changed
		/// Pass it on to the service
		/// </summary>
		public void SongSelected() => CurrentSongIndex = PlaybackManagerModel.CurrentSongIndex;

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
		/// Play the previous song in the list wrapping back to the end if required
		/// </summary>
		public void PlayPrevious()
		{
			IsPlaying = false;

			CurrentSongIndex--;
			if ( CurrentSongIndex < 0 )
			{
				CurrentSongIndex = Playlist.PlaylistItems.Count - 1;
			}

			Reporter?.SongIndexChanged( CurrentSongIndex );
			Play();
		}

		/// <summary>
		/// Play the next song in the list wrapping back to the start if required
		/// </summary>
		public void PlayNext()
		{
			IsPlaying = false;

			CurrentSongIndex++;
			if ( CurrentSongIndex >= Playlist.PlaylistItems.Count )
			{
				CurrentSongIndex = 0;
			}

			Reporter?.SongIndexChanged( CurrentSongIndex );
			Play();
		}

		/// <summary>
		/// Play the currently selected song
		/// </summary>
		public virtual void Play()
		{
			treatResumeAsPlay = false;
		}

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
		/// Seek to the specified position
		/// </summary>
		/// <param name="position"></param>
		public abstract void SeekTo( int position );

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

			if ( ( Playlist != null ) && ( CurrentSongIndex < Playlist.PlaylistItems.Count ) )
			{
				Song songToPlay = ( ( SongPlaylistItem )Playlist.PlaylistItems[ CurrentSongIndex ] ).Song;

				// Find the Source associated with this song
				Source songSource = Sources.SingleOrDefault( d => ( d.Id == songToPlay.SourceId ) );

				if ( songSource != null )
				{
					resource = FormSourceName( songSource, songToPlay.Path, local );
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
			if ( local == true )
			{
				// Need to escape the path if it is held remotely
				if ( songSource.ScanType == "FTP" )
				{
					sourceName = Path.Combine( songSource.LocalAccess, Uri.EscapeDataString( songPath.TrimStart( '/' ) ) );
				}
				else
				{
					sourceName = Path.Combine( songSource.LocalAccess, songPath.TrimStart( '/' ) );
				}
			}
			else
			{
				sourceName = Path.Combine( songSource.RemoteAccess, Uri.EscapeDataString( songPath.TrimStart( '/' ) ) );
			}

			return sourceName;
		}

		/// <summary>
		/// Report that the current song is being played
		/// </summary>
		protected void ReportSongPlayed() => Reporter?.SongPlayed( ( ( SongPlaylistItem )Playlist.PlaylistItems[ CurrentSongIndex ] ).Song );

		/// <summary>
		/// Select the next song to play based on whether or not repeat is on and the number of songs in the playlist
		/// </summary>
		/// <returns></returns>
		protected bool CanPlayNextSong()
		{
			bool canPlay = true;

			if ( CurrentSongIndex < ( Playlist.PlaylistItems.Count - 1 ) )
			{
				CurrentSongIndex++;
				Reporter?.SongIndexChanged( CurrentSongIndex );
			}
			else if ( ( PlaybackModeModel.RepeatOn == true ) && ( Playlist.PlaylistItems.Count > 0 ) )
			{
				// Play the first song
				CurrentSongIndex = 0;
				Reporter?.SongIndexChanged( CurrentSongIndex );
			}
			else
			{
				canPlay = false;
			}

			return canPlay;
		}

		/// <summary>
		/// The playlist of songs to play
		/// </summary>
		public Playlist Playlist { get; set; } = null;

		/// <summary>
		/// The sources associated with the songs
		/// </summary>
		public List<Source> Sources { get; set; } = null;

		/// <summary>
		/// The index of the song currently being played
		/// </summary>
		public int CurrentSongIndex { get; set; } = -1;

		/// <summary>
		/// Details of the playback device
		/// </summary>
		public PlaybackDevice PlaybackDevice { get; set; } = null;

		/// <summary>
		/// The instance used to report back significant events
		/// </summary>
		public IPlaybackCallbacks Reporter { get; set; } = null;

		/// <summary>
		/// Is the service currently playing
		/// </summary>
		private bool playing = false;

		/// <summary>
		/// Keep track of the selection state of this connection
		/// </summary>
		private bool Selected { get; set; } = false;

		/// <summary>
		/// At startup the Resume button should be treated as Play.
		/// </summary>
		private bool treatResumeAsPlay = false;

		/// <summary>
		/// The timer used to check the progress of the song
		/// </summary>
		private Timer positionTimer = null;

		/// <summary>
		/// The position timer period in milliseconds
		/// </summary>
		private const int TimerPeriod = 1000;

		/// <summary>
		/// The interface defining the calls back to the application
		/// </summary>
		public interface IPlaybackCallbacks
		{
			void SongIndexChanged( int songIndex );
			void PlayStateChanged( bool isPlaying );
			void SongPlayed( Song songPlayed );
			void ProgressReport( int position, int duration );
		}
	}
}