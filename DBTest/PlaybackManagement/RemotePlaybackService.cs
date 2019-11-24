﻿using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DBTest
{
	/// <summary>
	/// The RemotePlaybackService is a service used to control the remote playing of music via a DLNA device
	/// </summary>
	[Service]
	public class RemotePlaybackService: BasePlaybackService
	{
		/// <summary>
		/// Override the base OnCreate in order to create a timer to use to poll the device for position information
		/// </summary>
		public override void OnCreate()
		{
			base.OnCreate();

			positionTimer = new Timer();
			positionTimer.Elapsed += PositionTimerElapsed;
			positionTimer.Interval = 1000;

			// Get an instance of the PowerManager to aquire a wake lock
			PowerManager pm = ( PowerManager )GetSystemService( Context.PowerService );
			wakeLock = pm.NewWakeLock( WakeLockFlags.Partial, "DBTest" );
		}

		/// <summary>
		/// Play the currently selected song
		/// </summary>
		public async override void Play()
		{
			// Prevent this from being called again until it has been processed
			if ( isPreparing == false )
			{
				// Get the source path for the current song
				string filename = GetSongResource();
				if ( filename.Length > 0 )
				{
					isPreparing = true;

					// Prepare and start playing the song
					if ( await PrepareSong( filename ) == true )
					{
						if ( await PlaySong() == true )
						{
							IsPlaying = true;
							StartTimer();
							AquireLock();
						}
					}

					isPreparing = false;
				}
			}
		}

		/// <summary>
		/// Send a stop request to the DNLA device
		/// </summary>
		public async override void Stop()
		{
			string soapContent = DlnaRequestHelper.MakeSoapRequest( "Stop", "" );

			string request = DlnaRequestHelper.MakeRequest( "POST", PlaybackDevice.PlayUrl, "urn:schemas-upnp-org:service:AVTransport:1#Stop",
				PlaybackDevice.IPAddress, PlaybackDevice.Port, soapContent );

			string response = await DlnaRequestHelper.SendRequest( PlaybackDevice, request );

			if ( DlnaRequestHelper.GetResponseCode( response ) == 200 )
			{
				IsPlaying = false;
				StopTimer();
				ReleaseLock();
			}
		}

		/// <summary>
		/// Called when the application is just about to exit
		/// </summary>
		public override void Shutdown()
		{
			if ( IsPlaying == true )
			{
				Stop();
			}

			StopSelf();
		}

		/// <summary>
		/// Report the position obtained from the remote device
		/// </summary>
		public override int Position
		{
			get
			{
				return positionMilliseconds;
			}
		}

		/// <summary>
		/// Report the duration obtained from the remote device
		/// </summary>
		public override int Duration
		{
			get
			{
				return durationMilliseconds;
			}
		}

		/// <summary>
		/// Send a Pause request to the DNLA device
		/// </summary>
		public async override void Pause()
		{
			string soapContent = DlnaRequestHelper.MakeSoapRequest( "Pause", "" );

			string request = DlnaRequestHelper.MakeRequest( "POST", PlaybackDevice.PlayUrl, "urn:schemas-upnp-org:service:AVTransport:1#Pause",
				PlaybackDevice.IPAddress, PlaybackDevice.Port, soapContent );

			string response = await DlnaRequestHelper.SendRequest( PlaybackDevice, request );

			if ( DlnaRequestHelper.GetResponseCode( response ) == 200 )
			{
				IsPlaying = false;
				StopTimer();
				ReleaseLock();
			}
		}

		public override void Seek( int position )
		{
		}

		public override void Reset()
		{
		}

		/// <summary>
		/// Send a play request to the DNLA device
		/// </summary>
		public async override void Resume()
		{
			if ( await PlaySong() == true )
			{
				IsPlaying = true;
				StartTimer();
				AquireLock();
			}
		}

		/// <summary>
		/// Send the Uri of the song to the DNLA device
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private async Task<bool> PrepareSong( string fileName )
		{
			string soapContent = DlnaRequestHelper.MakeSoapRequest( "SetAVTransportURI",
				string.Format( "<CurrentURI>{0}</CurrentURI>\r\n<CurrentURIMetaData></CurrentURIMetaData>\r\n", fileName ) );

			string request = DlnaRequestHelper.MakeRequest( "POST", PlaybackDevice.PlayUrl,
				"urn:schemas-upnp-org:service:AVTransport:1#SetAVTransportURI", PlaybackDevice.IPAddress, PlaybackDevice.Port, soapContent );

			string response = await DlnaRequestHelper.SendRequest( PlaybackDevice, request );

			return ( DlnaRequestHelper.GetResponseCode( response ) == 200 );
		}

		/// <summary>
		/// Send a Play request to the DNLA device
		/// </summary>
		/// <returns></returns>
		private async Task<bool> PlaySong()
		{
			string soapContent = DlnaRequestHelper.MakeSoapRequest( "Play", "<Speed>1</Speed>\r\n" );

			string request = DlnaRequestHelper.MakeRequest( "POST", PlaybackDevice.PlayUrl, "urn:schemas-upnp-org:service:AVTransport:1#Play",
				PlaybackDevice.IPAddress, PlaybackDevice.Port, soapContent );

			string response = await DlnaRequestHelper.SendRequest( PlaybackDevice, request );

			return ( DlnaRequestHelper.GetResponseCode( response ) == 200 );
		}

		/// <summary>
		/// Called when the position timer has elapsed.
		/// Get the current playback posiotn from the DLNA device
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void PositionTimerElapsed( object sender, ElapsedEventArgs e )
		{
			StopTimer();

			string soapContent = DlnaRequestHelper.MakeSoapRequest( "GetPositionInfo", "" );

			string request = DlnaRequestHelper.MakeRequest( "POST", PlaybackDevice.PlayUrl, "urn:schemas-upnp-org:service:AVTransport:1#GetPositionInfo",
				PlaybackDevice.IPAddress, PlaybackDevice.Port, soapContent );

			string response = await DlnaRequestHelper.SendRequest( PlaybackDevice, request );

			if ( DlnaRequestHelper.GetResponseCode( response ) == 200 )
			{
				string trackDuration = response.TrimStart( "<TrackDuration>" ).TrimAfter( "</TrackDuration>" );
				durationMilliseconds = TimeStringToMilliseconds( trackDuration );

				string relTime = response.TrimStart( "<RelTime>" ).TrimAfter( "</RelTime>" );
				positionMilliseconds = TimeStringToMilliseconds( relTime );

				Logger.Log( string.Format( "Position: {0}, Duration {1}", positionMilliseconds, durationMilliseconds ) );

				// If the duration is 0 this could be due to missing the end of a song, or it can also happen at the 
				// very start of a track. So keep track of this and if it happens a few times switch to the next track
				// If the position is within 1 second of the duration when assume this track has finished and move on to the next track
				if ( ( ( durationMilliseconds == 0 ) && ( ++noPlayCount > 3 ) ) ||
						( ( durationMilliseconds != 0 ) && ( Math.Abs( durationMilliseconds - positionMilliseconds ) < 1050 ) ) )
				{
					Logger.Log( string.Format( "Next song please" ) );

					noPlayCount = 0;

					// Play the next song if there is one
					if ( CurrentSongIndex < ( Playlist.PlaylistItems.Count - 1 ) )
					{
						CurrentSongIndex++;
						Reporter?.SongIndexChanged( CurrentSongIndex );

						IsPlaying = false;
						Play();
					}
					else
					{
						IsPlaying = false;
						ReleaseLock();
					}
				}
				else
				{
					if ( durationMilliseconds != 0 )
					{
						noPlayCount = 0;
					}

					StartTimer();
				}
			}
		}

		private int TimeStringToMilliseconds( string timeString )
		{
			if ( timeString.Length < 8 )
			{
				timeString = timeString.PadLeft( 8, '0' );
			}

			TimeSpan ts = TimeSpan.ParseExact( timeString, @"hh\:mm\:ss", CultureInfo.InvariantCulture );
			return ( int )ts.TotalMilliseconds;
		}

		private void StartTimer()
		{
			positionTimer.Start();
		}

		private void StopTimer()
		{
			positionTimer.Stop();
		}

		/// <summary>
		/// Aquire the wakelock if not already held
		/// </summary>
		private void AquireLock()
		{
			if ( wakeLock.IsHeld == false )
			{
				wakeLock.Acquire();
			}
		}

		/// <summary>
		/// Release the wakelock if hels
		/// </summary>
		private void ReleaseLock()
		{
			if ( wakeLock.IsHeld == true )
			{
				wakeLock.Release();
			}
		}

		private int durationMilliseconds = 0;

		private int positionMilliseconds = 0;

		private bool isPreparing = false;

		private int noPlayCount = 0;

		private Timer positionTimer = null;

		private PowerManager.WakeLock wakeLock = null;
	}
}