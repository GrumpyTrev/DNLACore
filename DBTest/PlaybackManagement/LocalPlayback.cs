using System;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The LocalPlayback class is used to control the local playing of music using an Android MusicPlayer component
	/// </summary>
	public class LocalPlayback : BasePlayback
	{
		/// <summary>
		/// Called when the class instance is first created
		/// Initialise the Android MediaPlayer instance user to actually play the songs
		/// </summary>
		public LocalPlayback( Context context ) => InitialiseMediaPlayer( context );

		/// <summary>
		/// Called when the MediaPlayer has finished playing the current song
		/// </summary>
		public void OnCompletion()
		{
			IsPlaying = false;
			localPlayer.Reset();
			ReportSongFinished();
		}

		/// <summary>
		/// Called when the MediaPlayer has encounter an error condition
		/// </summary>
		/// <param name="what"></param>
		/// <param name="extra"></param>
		/// <returns></returns>
		public bool OnError( [GeneratedEnum] MediaError what, int extra )
		{
			localPlayer.Reset();
			isPreparing = false;

			Logger.Error( string.Format( "Error reported by MediaPlayer : {0} : {1}", what, extra ) );

			return true;
		}

		/// <summary>
		/// Called when the MediaPlayer has finished preparing a song source and is now ready to play the song
		/// </summary>
		public void OnPrepared()
		{
			localPlayer.Start();
			IsPlaying = true;
			isPreparing = false;

			ReportSongStarted();
		}

		/// <summary>
		/// Called when the application is just about to exit
		/// </summary>
		public override void Shutdown()
		{
			localPlayer.Stop();
			localPlayer.Release();

			IsPlaying = false;
		}

		/// <summary>
		/// Play the currently selected song
		/// </summary>
		public override void Play()
		{
			base.Play();

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
		/// Get the current position of the playing song
		/// </summary>
		public override int CurrentPosition
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
		private void InitialiseMediaPlayer( Context context )
		{
			localPlayer = new MediaPlayer();
			localPlayer.SetWakeMode( context, WakeLockFlags.Partial );

			// SetAudioAttributes requires API 21 == Lollipop
			if ( Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop )
			{
				localPlayer.SetAudioAttributes( new AudioAttributes.Builder().SetContentType( AudioContentType.Music ).Build() );
			}
			else
			{
				// Forced to use deprecated SetAudioStreamType for API < 21
				localPlayer.SetAudioStreamType( Stream.Music );
			}

			MediaPlayerInterface playInterface = new( this );
			localPlayer.SetOnPreparedListener( playInterface );
			localPlayer.SetOnErrorListener( playInterface );
			localPlayer.SetOnCompletionListener( playInterface );
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

	internal class MediaPlayerInterface( LocalPlayback localPlayback ) : Java.Lang.Object, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener, MediaPlayer.IOnCompletionListener
	{
		/// <summary>
		/// Called when the MediaPlayer has finished playing the current song
		/// </summary>
		/// <param name="mp"></param>
		public void OnCompletion( MediaPlayer _ ) => playbackInstance.OnCompletion();

		/// <summary>
		/// Called when the MediaPlayer has encounter an error condition
		/// </summary>
		/// <param name="mp"></param>
		/// <param name="what"></param>
		/// <param name="extra"></param>
		/// <returns></returns>
		public bool OnError( MediaPlayer _, [GeneratedEnum] MediaError what, int extra ) => playbackInstance.OnError( what, extra );

		/// <summary>
		/// Called when the MediaPlayer has finished preparing a song source and is now ready to play the song
		/// </summary>
		/// <param name="mp"></param>
		public void OnPrepared( MediaPlayer _ ) => playbackInstance.OnPrepared();

		private readonly LocalPlayback playbackInstance = localPlayback;
	}
}
