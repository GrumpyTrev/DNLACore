﻿using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;

namespace DBTest
{
	/// <summary>
	/// Base functionality for a playback service
	/// </summary>
	[Service]
	public abstract class BasePlaybackService: Service
	{
		/// <summary>
		/// Called when the service has been created to return the IBinder instance for the service
		/// </summary>
		/// <param name="intent"></param>
		/// <returns></returns>
		public override IBinder OnBind( Intent intent )
		{
			return serviceBinder;
		}

		/// <summary>
		/// Called when the service is first created. Create the binder to pass back the service instance
		/// </summary>
		public override void OnCreate()
		{
			base.OnCreate();

			serviceBinder = new PlaybackBinder( this );
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
		public abstract void Play();

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
		public abstract void Seek( int position );

		/// <summary>
		/// Get the current position of the playing song
		/// </summary>
		public abstract int Position { get; }

		/// <summary>
		/// Get the duration of the current song
		/// </summary>
		public abstract int Duration { get; }

		/// <summary>
		/// Is a song currently being played
		/// </summary>
		public virtual bool IsPlaying
		{
			get
			{
				return playing;
			}

			set
			{
				if ( playing != value )
				{
					playing = value;
					Reporter?.PlayStateChanged();
				}
			}
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
		public Device PlaybackDevice { get; set; } = null;

		/// <summary>
		/// The instance used to report back significant events
		/// </summary>
		public IServiceCallbacks Reporter { get; set; } = null;

		/// <summary>
		/// The IBinder instance for this service
		/// </summary>
		private IBinder serviceBinder = null;

		/// <summary>
		/// Is the service currently playing
		/// </summary>
		private bool playing = false;

		/// <summary>
		/// The Binder class for this service defining the interface betweeen the service and the appication
		/// </summary>
		public class PlaybackBinder: Binder
		{
			public PlaybackBinder( BasePlaybackService theService )
			{
				Service = theService;
			}

			/// <summary>
			/// The service instance passed back to the application
			/// </summary>
			public BasePlaybackService Service { get; } = null;
		}

		/// <summary>
		/// The interface defining the calls back to the application
		/// </summary>
		public interface IServiceCallbacks
		{
			void SongIndexChanged( int songIndex );
			void PlayStateChanged();
		}
	}
}