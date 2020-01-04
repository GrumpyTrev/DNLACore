using Android.App;
using Android.Media;
using Android.OS;
using Android.Runtime;
using System;

namespace DBTest
{
	/// <summary>
	/// The LocalPlaybackService is a service used to control the local playing of music using an Android MusicPlayer component
	/// </summary>
	[Service]
	public class LocalPlaybackService: BasePlaybackService, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener, MediaPlayer.IOnCompletionListener
	{
		/// <summary>
		/// Called whewn the service is first created
		/// Initialise the Android MediaPlayer instance user to actually play the songs
		/// </summary>
		public override void OnCreate()
		{
			base.OnCreate();

			InitialiseMediaPlayer();
		}

		/// <summary>
		/// Called when the MediaPlayer has finished playing the current song
		/// </summary>
		/// <param name="mp"></param>
		public void OnCompletion( MediaPlayer mp )
		{
			IsPlaying = false;

			// Play the next song if there is one
			if ( CurrentSongIndex < ( Playlist.PlaylistItems.Count - 1 ) )
			{
				CurrentSongIndex++;
				Reporter?.SongIndexChanged( CurrentSongIndex );

				Play();
			}
			else
			{
				localPlayer.Reset();
			}
		}

		/// <summary>
		/// Called when the MediaPlayer has encounter an error condition
		/// </summary>
		/// <param name="mp"></param>
		/// <param name="what"></param>
		/// <param name="extra"></param>
		/// <returns></returns>
		public bool OnError( MediaPlayer mp, [GeneratedEnum] MediaError what, int extra )
		{
			return true;
		}

		/// <summary>
		/// Called when the MediaPlayer has finished preparing a song source and is now ready to play the song
		/// </summary>
		/// <param name="mp"></param>
		public void OnPrepared( MediaPlayer mp )
		{
			localPlayer.Start();
			IsPlaying = true;
			isPreparing = false;

			ReportSongPlayed();
		}

		/// <summary>
		/// Called when the application is just about to exit
		/// </summary>
		public override void Shutdown()
		{
			localPlayer.Stop();
			localPlayer.Release();

			IsPlaying = false;

			StopSelf();
		}

		/// <summary>
		/// Play the currently selected song
		/// </summary>
		public override void Play()
		{
			// Prevent this from being called again until it has been processed
			if ( isPreparing == false )
			{
				// Get the source path for the current song
				string filename = GetSongResource( true );
				if ( filename.Length > 0 )
				{
					// Set uri
					Android.Net.Uri trackUri = Android.Net.Uri.Parse( filename );

					try
					{
						localPlayer.Reset();
						localPlayer.SetDataSource( trackUri.ToString() );
						isPreparing = true;
						localPlayer.PrepareAsync();
					}
					catch ( Exception error )
					{
						Logger.Error( string.Format( "Error setting data source for : {0} : {1}", filename, error.Message ) );
					}
				}
			}
		}

		/// <summary>
		/// Stop playing the current song
		/// </summary>
		public override void Stop()
		{
			if ( localPlayer.IsPlaying == true )
			{
				localPlayer.Stop();
				localPlayer.Reset();
				IsPlaying = false;
			}
		}

		/// <summary>
		/// Pause playing the current song
		/// </summary>
		public override void Pause()
		{
			localPlayer.Pause();
			IsPlaying = false;
		}

		/// <summary>
		/// Resume playing the current song
		/// </summary>
		public override void Resume()
		{
			localPlayer.Start();
			IsPlaying = true;
		}

		/// <summary>
		/// Reset the local player
		/// </summary>
		public override void Reset()
		{
			localPlayer.Reset();
			IsPlaying = false;
			isPreparing = false;
		}

		/// <summary>
		/// Seek to the specified position
		/// </summary>
		/// <param name="position"></param>
		public override void Seek( int position ) => localPlayer.SeekTo( position );

		/// <summary>
		/// Get the current position of the playing song
		/// </summary>
		public override int Position
		{
			get
			{
				int position = 0;
				if ( localPlayer.IsPlaying == true )
				{
					position = localPlayer.CurrentPosition;
				}

				return position;
			}
		}

		/// <summary>
		/// Get the duration of the current song
		/// </summary>
		public override int Duration
		{
			get
			{
				int duration = 0;
				if ( localPlayer.IsPlaying == true )
				{
					duration = localPlayer.Duration;
				}

				return duration;
			}
		}

		/// <summary>
		/// Initialise the Android MediaPlayer component
		/// </summary>
		private void InitialiseMediaPlayer()
		{
			localPlayer = new MediaPlayer();
			localPlayer.SetWakeMode( ApplicationContext, WakeLockFlags.Partial );
			localPlayer.SetAudioAttributes( new AudioAttributes.Builder().SetContentType( AudioContentType.Music ).Build() ); 
			localPlayer.SetOnPreparedListener( this );
			localPlayer.SetOnErrorListener( this );
			localPlayer.SetOnCompletionListener( this );
		}

		/// <summary>
		/// The Android MediaPlayer instance user to actually play the songs
		/// </summary>
		private MediaPlayer localPlayer = null;

		/// <summary>
		/// Flag to indicate that the media player is in the middle of preparing a file for playback
		/// </summary>
		private bool isPreparing = false;
	}
}