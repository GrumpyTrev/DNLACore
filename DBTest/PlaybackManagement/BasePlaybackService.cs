using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

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
		public override IBinder OnBind( Intent intent ) => serviceBinder;

		/// <summary>
		/// Called when the service is first created. Create the binder to pass back the service instance
		/// </summary>
		public override void OnCreate()
		{
			base.OnCreate();

			serviceBinder = new PlaybackBinder( this );
		}

		[return: GeneratedEnum]
		public override StartCommandResult OnStartCommand( Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId ) => 
			base.OnStartCommand( intent, flags, startId );

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
		/// Called when the associated application is shutting down.
		/// Carry out any final actions
		/// </summary>
		public abstract void Shutdown();

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
			get => playing;

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
		/// Get the source path for the currently playing song
		/// </summary>
		/// <returns></returns>
		protected string GetSongResource( bool local )
		{
			string resource = "";

			if ( ( Playlist != null ) && ( CurrentSongIndex < Playlist.PlaylistItems.Count ) )
			{
				Song songToPlay = Playlist.PlaylistItems[ CurrentSongIndex ].Song;

				// Find the Source associated with this song
				Source songSource = Sources.FirstOrDefault( d => ( d.Id == songToPlay.SourceId ) );

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
		protected string FormSourceName( Source songSource, string songPath, bool local ) => 
			( local == true ) ? Path.Combine( songSource.LocalAccess, songPath.TrimStart( '/' ) ) :
				Path.Combine( songSource.RemoteAccess, Uri.EscapeDataString( songPath.TrimStart( '/' ) ) );

		/// <summary>
		/// Report that the current song is being played
		/// </summary>
		protected void ReportSongPlayed() => Reporter?.SongPlayed( Playlist.PlaylistItems[ CurrentSongIndex ].Song );

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
			public PlaybackBinder( BasePlaybackService theService ) => Service = theService;

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
			void SongPlayed( Song songPlayed );
		}
	}
}