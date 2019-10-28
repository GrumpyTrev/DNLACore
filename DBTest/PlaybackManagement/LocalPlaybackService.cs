using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using System;
using System.IO;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The LocalPlaybackService is a service used to control the local playing of music using an Android MusicPlayer component
	/// </summary>
	[Service]
	public class LocalPlaybackService: BasePlaybackService, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener, MediaPlayer.IOnCompletionListener
	{
		public override bool OnUnbind( Intent intent )
		{
			if ( localPlayer != null )
			{
				localPlayer.Stop();
				localPlayer.Release();

				IsPlaying = false;
			}

			return false;
		}

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
		}

		/// <summary>
		/// Play the currently selected song
		/// </summary>
		public override void Play()
		{
			if ( ( Playlist != null ) && ( CurrentSongIndex < Playlist.PlaylistItems.Count ) && ( isPreparing == false ) )
			{
				localPlayer.Reset();

				Song songToPlay = Playlist.PlaylistItems[ CurrentSongIndex ].Song;

				// Find the Source associated with this song
				Source songSource = Sources.FirstOrDefault( d => ( d.Id == songToPlay.SourceId ) );

				if ( songSource != null )
				{
					string filename = Path.Combine( songSource.AccessSource, songToPlay.Path.Substring( 1 ).Replace( " ", "%20" ) );

					// Set uri
					Android.Net.Uri trackUri = Android.Net.Uri.Parse( filename );

					try
					{
						localPlayer.SetDataSource( trackUri.ToString() );
						isPreparing = true;
						localPlayer.PrepareAsync();
					}
					catch ( Exception e )
					{
						Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "Error setting data source for : {0} : {1}", filename, e.Message ) );
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
		public override void Seek( int position )
		{
			localPlayer.SeekTo( position );
		}

		/// <summary>
		/// Get the current position of the playing song
		/// </summary>
		public override int Position
		{
			get
			{
				return localPlayer.CurrentPosition;
			}
		}

		/// <summary>
		/// Get the duration of the current song
		/// </summary>
		public override int Duration
		{
			get
			{
				return localPlayer.Duration;
			}
		}

		/// <summary>
		/// Initialise the Android MediaPlayer component
		/// </summary>
		private void InitialiseMediaPlayer()
		{
			localPlayer = new MediaPlayer();
			localPlayer.SetWakeMode( ApplicationContext, WakeLockFlags.Partial );
			localPlayer.SetAudioStreamType( Android.Media.Stream.Music );
			localPlayer.SetOnPreparedListener( this );
			localPlayer.SetOnErrorListener( this );
			localPlayer.SetOnCompletionListener( this );
		}

		private MediaPlayer localPlayer = null;

		private bool isPreparing = false;
	}
}