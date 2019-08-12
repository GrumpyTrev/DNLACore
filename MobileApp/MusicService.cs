using System;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;

namespace MobileApp
{
	[Service]
	public class MusicService: Service, MediaPlayer.IOnPreparedListener, MediaPlayer.IOnErrorListener, MediaPlayer.IOnCompletionListener
	{
		private MediaPlayer player;
		private IBinder musicBind;

		public override IBinder OnBind( Intent intent )
		{
			return musicBind;
		}

		public override bool OnUnbind( Intent intent )
		{
			player.Stop();
			player.Release();

			return false;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
		}

		public override void OnCreate()
		{
			base.OnCreate();

//			SongIndex = 0;

			player = new MediaPlayer();

			InitialiseMusicPlayer();

			musicBind = new MusicBinder( this );
		}

//		public List<Song> Songs { private get; set; }

		public void PlaySong( string fileName )
		{
			player.Reset();

			Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "PlaySong playing song : {0}", fileName ) );

			// Set uri
			Android.Net.Uri trackUri = Android.Net.Uri.Parse( fileName );

			try
			{
				player.SetDataSource( ApplicationContext, trackUri );
			}
			catch ( Exception e )
			{
				Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "Error setting data source for : {0} : {1}", fileName, e.Message ) );
			}

			player.PrepareAsync();
		}

		public void OnCompletion( MediaPlayer mp )
		{
			throw new NotImplementedException();
		}

		public bool OnError( MediaPlayer mp, [GeneratedEnum] MediaError what, int extra )
		{
			Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "OnError what: {0} extra: {1}", what, extra ) );
			return true;
		}

		public void OnPrepared( MediaPlayer mp )
		{
			player.Start();
		}

		public int SongIndex { private get; set; }
		
		private void InitialiseMusicPlayer()
		{
			player.SetWakeMode( ApplicationContext, WakeLockFlags.Partial );
			player.SetAudioStreamType( Stream.Music );
			player.SetOnPreparedListener( this );
			player.SetOnCompletionListener( this );
			player.SetOnErrorListener( this );
		}

		public int Position
		{
			get
			{
				return player.CurrentPosition;
			}
		}

		public int Duration
		{
			get
			{
				return player.Duration;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return player.IsPlaying;
			}
		}

		public void PausePlayer()
		{
			player.Pause();
		}

		public void Seek( int posn )
		{
			player.SeekTo( posn );
		}

		public void Go()
		{
			player.Start();
		}

		public void PlayPrev()
		{
//			SongIndex--;
//			if ( SongIndex < 0 )
//			{
//				SongIndex = Songs.Count - 1;
//			}

//			PlaySong();
		}

		public void PlayNext()
		{
//			SongIndex++;
//			if ( SongIndex >= Songs.Count )
//			{
//				SongIndex = 0;
//			}

//			PlaySong();
		}
	}



	public class MusicBinder: Binder
	{
		public MusicBinder( MusicService theService )
		{
			boundService = theService;
		}

		public MusicService Service
		{
			get
			{
				return boundService;
			}
		}

		private MusicService boundService = null;
	}

}