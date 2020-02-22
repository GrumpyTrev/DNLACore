using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Globalization;
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
				string filename = GetSongResource( false );
				if ( filename.Length > 0 )
				{
					isPreparing = true;

					// Prepare and start playing the song
					if ( await PrepareSong( filename, Playlist.PlaylistItems[ CurrentSongIndex ].Song ) == true )
					{
						if ( await PlaySong() == true )
						{
							IsPlaying = true;
							StartTimer();
							AquireLock();

							ReportSongPlayed();
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
			string soapContent = DlnaRequestHelper.MakeSoapRequest( "Stop" );

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
		public override int Position => positionMilliseconds;

		/// <summary>
		/// Report the duration obtained from the remote device
		/// </summary>
		public override int Duration => durationMilliseconds;

		/// <summary>
		/// Send a Pause request to the DNLA device
		/// </summary>
		public async override void Pause()
		{
			string soapContent = DlnaRequestHelper.MakeSoapRequest( "Pause" );

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
		private async Task<bool> PrepareSong( string fileName, Song songToPlay )
		{
			string soapContent = DlnaRequestHelper.MakeSoapRequest( "SetAVTransportURI",
				string.Format( "<CurrentURI>{0}</CurrentURI>\r\n<CurrentURIMetaData>{1}</CurrentURIMetaData>\r\n", fileName, Desc( fileName, songToPlay ) ) );

			string request = DlnaRequestHelper.MakeRequest( "POST", PlaybackDevice.PlayUrl,
				"urn:schemas-upnp-org:service:AVTransport:1#SetAVTransportURI", PlaybackDevice.IPAddress, PlaybackDevice.Port, soapContent );

			string response = await DlnaRequestHelper.SendRequest( PlaybackDevice, request );

			return ( DlnaRequestHelper.GetResponseCode( response ) == 200 );
		}

		/// <summary>
		/// Get the metadata for the specified filename
		/// </summary>
		/// <returns></returns>
		private string Desc( string fileName, Song songToPlay ) =>
			string.Format(
				"<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\" " +
				"xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\">\r\n" +
				"<item id=\"HTTP stream\" restricted=\"0\">\r\n" +
				"<dc:title>{0} : {2}</dc:title>\r\n" +
				"<upnp:class>object.item.audioItem.musicTrack</upnp:class>\r\n" +
				"<res protocolInfo=\"http-get:*:audio/mpeg:*\">{1}</res>\r\n" +
				"</item>\r\n" +
				"</DIDL-Lite>\r\n",
				songToPlay.Title, fileName, songToPlay.Artist.Name );

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

			// Send the GetPositionInfo request and get the response
			string response = await DlnaRequestHelper.SendRequest( PlaybackDevice, 
				DlnaRequestHelper.MakeRequest( "POST", PlaybackDevice.PlayUrl, "urn:schemas-upnp-org:service:AVTransport:1#GetPositionInfo",
					PlaybackDevice.IPAddress, PlaybackDevice.Port, DlnaRequestHelper.MakeSoapRequest( "GetPositionInfo" ) ) );

			if ( DlnaRequestHelper.GetResponseCode( response ) == 200 )
			{
				durationMilliseconds = TimeStringToMilliseconds( response.TrimStart( "<TrackDuration>" ).TrimAfter( "</TrackDuration>" ) );
				positionMilliseconds = TimeStringToMilliseconds( response.TrimStart( "<RelTime>" ).TrimAfter( "</RelTime>" ) );

				Logger.Log( string.Format( "Position: {0}, Duration {1}", positionMilliseconds, durationMilliseconds ) );

				// Assume the track has not finished
				bool nextTrack = false;

				// If a chnage has already been schedukled then do it now
				if ( changeTrackNextTime == true )
				{
					nextTrack = true;
					changeTrackNextTime = false;
				}
				else
				{
					// If the duration is 0 this could be due to missing the end of a song, or it can also happen at the 
					// very start of a track. So keep track of this and if it happens a few times switch to the next track
					if ( durationMilliseconds == 0 )
					{
						if ( ++noPlayCount > 3 )
						{
							nextTrack = true;
							noPlayCount = 0;
						}
					}
					else
					{
						noPlayCount = 0;

						// If the position is within 1/4 second of the duration when assume this track has finished and move on to the next track.
						// If the position is around 1 second of the duration then move on to the next track the next time this position is obtained
						int timeLeft = Math.Abs( durationMilliseconds - positionMilliseconds );

						if ( timeLeft < 250 )
						{
							nextTrack = true;
						}
						else if ( timeLeft < 1050 )
						{
							changeTrackNextTime = true;
						}
					}
				}

				if ( nextTrack == true )
				{
					IsPlaying = false;

					// Play the next song if there is one
					if ( CanPlayNextSong() == true )
					{
						Play();
					}
					else
					{
						ReleaseLock();
					}
				}
				else
				{
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

		/// <summary>
		/// Start the progress timer
		/// </summary>
		private void StartTimer() => positionTimer.Start();

		/// <summary>
		/// Stop the progress timer
		/// </summary>
		private void StopTimer() => positionTimer.Stop();

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

		/// <summary>
		/// The duration as reported from the remote device
		/// </summary>
		private int durationMilliseconds = 0;

		/// <summary>
		/// The position as reported from the remote device
		/// </summary>
		private int positionMilliseconds = 0;

		/// <summary>
		/// Flag indicating that the remote device is busy preparing a song to be played
		/// </summary>
		private bool isPreparing = false;

		/// <summary>
		/// Counter used to detect when the end of a song may have been missed
		/// </summary>
		private int noPlayCount = 0;

		/// <summary>
		/// Flag indicating that the next track should be played the next time the position is obtained
		/// </summary>
		private bool changeTrackNextTime = false;

		/// <summary>
		/// The timer used to check the progress of the song
		/// </summary>
		private Timer positionTimer = null;

		/// <summary>
		/// Lock used to keep the app alive
		/// </summary>
		private PowerManager.WakeLock wakeLock = null;
	}
}