using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using FluentFTP;
using static Android.Widget.MediaController;

namespace MobileApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IMediaPlayerControl, IServiceConnection
	{

		public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
			bool processed = false;
			bool retVal = true;

			if ( id == Resource.Id.action_settings)
            {
				processed = true;
			}
			else if ( id == Resource.Id.action_shuffle )
			{
				processed = true;
			}
			else if ( id == Resource.Id.action_end )
			{
				UnbindService( this );
				musicBound = false;
				musicSvr = null;
				Finish();
				processed = true;
			}

			if ( processed == false )
			{
				retVal = base.OnOptionsItemSelected( item );
			}

			return retVal;
        }

		protected override void OnCreate( Bundle savedInstanceState )
		{
			base.OnCreate( savedInstanceState );
			SetContentView( Resource.Layout.activity_main );

			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>( Resource.Id.toolbar );
			SetSupportActionBar( toolbar );

			Button scanRemoteLibraryButton = FindViewById<Button>( Resource.Id.scanRemoteLibraryButton );
			scanRemoteLibraryButton.Click += MainActivity_ScanRemoteLibraryClick;

			Button scanButton = FindViewById<Button>( Resource.Id.scanButton );
			scanButton.Click += MainActivity_ScanClick;

			Button castRemoteButton = FindViewById<Button>( Resource.Id.castRemoteButton );
			castRemoteButton.Click += CastRemoteButton_Click;

			Button castLocalButton = FindViewById<Button>( Resource.Id.castLocalButton );
			castLocalButton.Click += CastLocalButton_Click;

			Button playLocalButton = FindViewById<Button>( Resource.Id.playLocalButton );
			playLocalButton.Click += PlayLocalButton_Click;

			Button playRemoteButton = FindViewById<Button>( Resource.Id.playRemoteButton );
			playRemoteButton.Click += PlayRemoteButton_Click;

			// Fill the spinner control with all the available trips and set the selected trip
			deviceSpinner = FindViewById<Spinner>( Resource.Id.deviceSpinner );

			// Create adapter to supply these strings. Use a custom layout for the selected item but the standard layout for the dropdown
			deviceAdapter = new DeviceAdapter( this, Resource.Layout.spinner_item, deviceCollection.ConnectedDevices() );
			deviceAdapter.SetDropDownViewResource( Android.Resource.Layout.SimpleSpinnerDropDownItem );

			// Link the spinner with the device data
			deviceSpinner.Adapter = deviceAdapter;

			discoverer.DeviceDiscovered += Discoverer_DeviceDiscovered;
//			discoverer.DiscoveryDone += Discoverer_DiscoveryDone;
			controller.DeviceSupportsPlayback += Controller_DeviceSupportsPlayback;

			// Start the HTTP server
			localServer = new SimpleHTTPServer( "/storage/emulated/0/", 8080 );

			// Check for read access
			if ( CheckCallingOrSelfPermission( Manifest.Permission.ReadExternalStorage ) == Android.Content.PM.Permission.Granted )
			{
			}

			SetController();
		}

		protected override void OnStart()
		{
			base.OnStart();

			if ( playIntent == null )
			{
				playIntent = new Intent( this, typeof( MusicService ) );
				BindService( playIntent, this, Bind.AutoCreate );
			}
		}

		protected override void OnDestroy()
		{
			if ( musicBound == true )
			{
				UnbindService( this );
				musicSvr = null;
			}

			base.OnDestroy();
		}


		private void SetController()
		{
			musicController = new MusicController( this );
			musicController.SetPrevNextListeners( new ClickHandler() { OnClickAction = () => { PlayNext(); } },
				new ClickHandler() { OnClickAction = () => { PlayPrev(); } } );
			musicController.SetMediaPlayer( this );

			musicController.SetAnchorView( FindViewById<LinearLayout>( Resource.Id.mainLayout ) );

			musicController.Enabled = true;
		}

		private class ClickHandler: Java.Lang.Object, View.IOnClickListener
		{
			public Action OnClickAction;

//			public IntPtr Handle { get; set; }

//			public void Dispose()
//			{
//			}

			public void OnClick( View v )
			{
				OnClickAction();
			}
		}


		private void PlayRemoteButton_Click( object sender, EventArgs e )
		{
			musicSvr.PlaySong( "http://192.168.1.5:80/RemoteMusic/Devo/Freedom%20of%20Choice/1.%20Girl%20U%20Want.mp3" );
		}

		private void PlayLocalButton_Click( object sender, EventArgs e )
		{
			musicSvr.PlaySong( "/storage/emulated/0/temp.mp3" );
		}

		private void CastRemoteButton_Click( object sender, EventArgs e )
		{
			// Which DNLA device is selected
			Device selectedDevice = deviceCollection.FindDevice( ( string )deviceSpinner.SelectedItem );

			controller.Play( selectedDevice, "http://192.168.1.5:80/RemoteMusic/Devo/Freedom%20of%20Choice/1.%20Girl%20U%20Want.mp3" );
		}

		private void CastLocalButton_Click( object sender, EventArgs e )
		{
			// Which DNLA device is selected
			Device selectedDevice = deviceCollection.FindDevice( ( string )deviceSpinner.SelectedItem );

			//controller.Play( selectedDevice, "http://192.168.1.6:8080/Music/Anna Calvi/Hunter/1. As A Man.mp3" );
			controller.Play( selectedDevice, "http://192.168.1.6:8080/temp.mp3" );
		}

		private void MainActivity_ScanRemoteLibraryClick( object sender, EventArgs e )
		{
			// Just a test to try and open a remote file/directory
			//			FileAttributes info = File.GetAttributes( @"http://192.168.1.5:80/RemoteMusic/Devo/Freedom of Choice/1. Girl U Want.mp3" );
			//	FileAttributes info = File.GetAttributes( @"\\192.168.1.9\RemoteMusic\Devo\Freedom of Choice/1. Girl U Want.mp3" );
			//			FileAttributes info = File.GetAttributes( @"\\GRUMPYS-PC\Music\Devo\Freedom of Choice\1. Girl U Want.mp3" );
			//FileAttributes info = File.GetAttributes( @"\\192.168.1.9\D\Music\Devo\Freedom of Choice\1. Girl U Want.mp3" );
			DoFTPStuff();
		}

		private async void DoFTPStuff()
		{
			// create an FTP client
			FtpClient client = new FtpClient( "192.168.1.5" );

			// begin connecting to the server
			await client.ConnectAsync();

			ScanFolder( "", client );

			client.Disconnect();
		}

		private void ScanFolder( string folderName, FtpClient client )
		{
			try
			{
				// Get all the items in the folder
				foreach ( FtpListItem fileItem in client.GetListing( folderName ) )
				{
					// If this is a directory then scan it
					if ( fileItem.Type == FtpFileSystemObjectType.Directory )
					{
						ScanFolder( fileItem.FullName, client );
					}
					else if ( fileItem.Type == FtpFileSystemObjectType.File )
					{
						// Only process MP3 files
						if ( fileItem.Name.ToUpper().EndsWith( ".MP3" ) )
						{
							GetFileTags( fileItem, client );
						}
					}
				}
			}
			catch ( FluentFTP.FtpException ftpProblem )
			{
				Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "FTP exception reading directory list: {0}", ftpProblem.Message ) );
			}
		}

		private void GetFileTags( FtpListItem fileItem, FtpClient client )
		{
			Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "Processing song no {1} : {0}", fileItem.FullName, ++songCount ) );

			try
			{
				// create remote FTP stream and local file stream
				using ( Stream remoteFileStream = client.OpenRead( fileItem.FullName, FtpDataType.Binary ) )
				{
					// Read enough bytes to determine the size of the ID3 header
					byte[] headerBuffer = new byte[ 10 ];
					int len = remoteFileStream.Read( headerBuffer, 0, headerBuffer.Length );
					if ( len > 0 )
					{
						ulong headerSize = 0;

						using ( MemoryStream mStream = new MemoryStream( headerBuffer ) )
						{
							using ( BinaryReader reader = new BinaryReader( mStream ) )
							{
								string id3start = new string( reader.ReadChars( 3 ) );
								if ( id3start == "ID3" )
								{
									int majorVersion = Convert.ToInt32( reader.ReadByte() );
									int minorVersion = Convert.ToInt32( reader.ReadByte() );

									bool[] bits = new bool[ 8 ];
									new BitArray( new byte[] { reader.ReadByte() } ).CopyTo( bits, 0 );
									Array.Reverse( bits );

									headerSize = ReadEncodedSize( reader, 4 );

									if ( headerSize > 0 )
									{
										byte[] id3Header = new byte[ headerSize ];
										int id3len = remoteFileStream.Read( id3Header, 0, id3Header.Length );

										Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "Read ID3 header length {0}", id3len ) );

										ReadFrames( id3Header, majorVersion );
									}
								}
								else
								{
									Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "NO ID3 TAG FOUND!!!" ) );
								}
							}
						}

					}
				}

				// read the FTP response and prevent stale data on the socket
				client.GetReply();
			}
			catch ( FluentFTP.FtpException songProblem )
			{
				Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "FTP exception reading song: {0} : {1}",
					fileItem.FullName, songProblem.Message ) );
			}

		}

		private ulong ReadEncodedSize( BinaryReader reader, int noBytes )
		{
			ulong size = 0;

			byte[] tagSize = reader.ReadBytes( noBytes );
			byte[] bytes = new byte[ noBytes ];

			if ( noBytes == 4 )
			{
				bytes[ 3 ] = ( byte )( tagSize[ 3 ] | ( ( tagSize[ 2 ] & 1 ) << 7 ) );
				bytes[ 2 ] = ( byte )( ( ( tagSize[ 2 ] >> 1 ) & 63 ) | ( ( tagSize[ 1 ] & 3 ) << 6 ) );
				bytes[ 1 ] = ( byte )( ( ( tagSize[ 1 ] >> 2 ) & 31 ) | ( ( tagSize[ 0 ] & 7 ) << 5 ) );
				bytes[ 0 ] = ( byte )( ( tagSize[ 0 ] >> 3 ) & 15 );

				size = ( ulong )( bytes[ 3 ] + ( bytes[ 2 ] << 8 ) + ( bytes[ 1 ] << 16 ) + ( bytes[ 0 ] << 24 ) );
			}
			else
			{
				bytes[ 2 ] = ( byte )( tagSize[ 2 ] | ( ( tagSize[ 1 ] & 1 ) << 7 ) );
				bytes[ 1 ] = ( byte )( ( ( tagSize[ 1 ] >> 1 ) & 63 ) | ( ( tagSize[ 0 ] & 3 ) << 6 ) );
				bytes[ 0 ] = ( byte )( ( tagSize[ 0 ] >> 2 ) & 31 );

				size = ( ulong )( bytes[ 2 ] + ( bytes[ 1 ] << 8 ) + ( bytes[ 0 ] << 16 ) );
			}

			return size;
		}

		private ulong ReadFrameSize( BinaryReader reader, int noBytes )
		{
			ulong size = 0;

			byte[] tagSize = reader.ReadBytes( noBytes );

			if ( noBytes == 4 )
			{
				size = ( ulong )( tagSize[ 3 ] + ( tagSize[ 2 ] << 8 ) + ( tagSize[ 1 ] << 16 ) + ( tagSize[ 0 ] << 24 ) );
			}
			else
			{
				size = ( ulong )( tagSize[ 2 ] + ( tagSize[ 1 ] << 8 ) + ( tagSize[ 0 ] << 16 ) );
			}

			return size;
		}

		private void ReadFrames( byte[] id3Header, int majorVersion )
		{
			using ( MemoryStream mStream = new MemoryStream( id3Header ) )
			{
				using ( BinaryReader reader = new BinaryReader( mStream ) )
				{
					int nameSize = ( majorVersion == 2 ) ? 3 : 4;

					Dictionary<string, byte[]> frames = new Dictionary<string, byte[]>();

					while ( ( reader.BaseStream.Position != reader.BaseStream.Length ) && ( reader.PeekChar() != 0 ) )
					{
						try
						{
							string frameName = new string( reader.ReadChars( nameSize ) );

							ulong frameSize = ReadFrameSize( reader, nameSize );

							// Skip a couple of bytes and then read the frame contents
							reader.ReadByte();
							reader.ReadByte();

							if ( frameSize > 0 )
							{
								byte[] frameContents = reader.ReadBytes( ( int )frameSize );
								frames[ frameName ] = frameContents;
							}
							else
							{
								frames[ frameName ] = new byte[ 0 ];
							}
						}
						catch ( ArgumentException argException )
						{
						}

					}

					string artist = "";
					string title = "";
					string album = "";
					string track = "";

					if ( majorVersion == 2 )
					{
						artist = GetStringTag( frames, "TP1" );
						title = GetStringTag( frames, "TT2" );
						album = GetStringTag( frames, "TAL" );
						track = GetStringTag( frames, "TRK" );
					}
					else
					{
						artist = GetStringTag( frames, "TPE1" );
						title = GetStringTag( frames, "TIT2" );
						album = GetStringTag( frames, "TALB" );
						track = GetStringTag( frames, "TRCK" );
					}

					Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "Artist: {0} Title: {1} Album: {2} Track: {3}", artist, title, album, track ) );

				}
			}
		}

		private string GetStringTag( Dictionary<string, byte[]> hashTable, string tagType )
		{
			string tag = "";

			if ( hashTable.ContainsKey( tagType ) == true )
			{
				byte[] contents = hashTable[ tagType ];
				byte encoding = contents[ 0 ];

				if ( ( encoding == 0 ) || ( encoding == 3 ) )
				{
					// LATIN and UTF-8
					tag = Encoding.GetEncoding( "ISO-8859-1" ).GetString( contents, 1, contents.Length - 1 );
				}
				else if ( encoding == 1 )
				{
					// UCS-2 with 2 byte header
					tag = Encoding.Unicode.GetString( contents, 3, contents.Length - 3 );
				}
				else
				{
					// UTF-16E without 2 byte header
					tag = Encoding.Unicode.GetString( contents, 1, contents.Length - 1 );
				}
			}

			return tag.TrimEnd( '\0' );
		}

		private void MainActivity_ScanClick( object sender, EventArgs e )
		{
			deviceCollection.DeviceCollection.Clear();
			discoverer.GoDiscover();
		}

		private void Discoverer_DiscoveryDone( object sender, EventArgs e )
		{
//			foreach ( Device discoveryDevice in deviceCollection.DeviceCollection )
	//		{
		//		controller.GetTransportService( discoveryDevice );
			//}
		}

		/// <summary>
		/// Called when a device that supports playback has been discovered
		/// The device is already in the device collection but now has the CanPlayMedia flag set
		/// Add the device to the dropdown
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Controller_DeviceSupportsPlayback( object sender, DeviceControl.DevicePlaybackArgs e )
		{
			deviceAdapter.ReloadSpinner( deviceCollection.ConnectedDevices() );
		}

		/// <summary>
		/// Called when a device has been discovered using SSDP
		/// If it is unique add it to the collection and check if it can play media
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Discoverer_DeviceDiscovered( object sender, DeviceDiscovery.DeviceDiscoveredArgs e )
		{
			if ( deviceCollection.AddDevice( e.DeviceDiscovered ) == true )
			{
				Log.WriteLine( LogPriority.Debug, "MobileApp", string.Format( "Discovered IP {0}:{1} Url {2}", e.DeviceDiscovered.IPAddress,
					e.DeviceDiscovered.Port, e.DeviceDiscovered.DescriptionURL ) );

				controller.GetTransportService( e.DeviceDiscovered );
			}
		}

		private void PlayNext()
		{
			musicSvr.PlayNext();
			musicController.Show( 0 );
		}

		//play previous
		private void PlayPrev()
		{
			musicSvr.PlayPrev();
			musicController.Show( 0 );
		}

		public bool CanPause()
		{
			return true;
		}

		public bool CanSeekBackward()
		{
			return true;
		}

		public bool CanSeekForward()
		{
			return true;
		}

		public void Pause()
		{
			throw new NotImplementedException();
		}

		public void SeekTo( int pos )
		{
			throw new NotImplementedException();
		}

		public void Start()
		{
			throw new NotImplementedException();
		}

		public void OnServiceConnected( ComponentName name, IBinder service )
		{
			MusicBinder binder = ( MusicBinder )service;
			musicSvr = binder.Service;
			musicBound = true;
		}

		public void OnServiceDisconnected( ComponentName name )
		{
			musicBound = false;
		}

		/// <summary>
		/// The DeviceDiscovery instance used to find UPnP devices
		/// </summary>
		private DeviceDiscovery discoverer = new DeviceDiscovery();

		/// <summary>
		/// The DeviceControl used to access and control a DNLA device
		/// </summary>
		private DeviceControl controller = new DeviceControl();

		/// <summary>
		/// The collection of unique discovered devices
		/// </summary>
		private Devices deviceCollection = new Devices();

		/// <summary>
		/// The DeviceAdapter used to hold the list of available devices
		/// </summary>
		private DeviceAdapter deviceAdapter;

		private Spinner deviceSpinner = null;

		private int songCount = 0;

		private SimpleHTTPServer localServer;

		private MusicController musicController;

		public int AudioSessionId => throw new NotImplementedException();

		public int BufferPercentage => throw new NotImplementedException();

		public int CurrentPosition => throw new NotImplementedException();

		public int Duration => throw new NotImplementedException();

		public bool IsPlaying => throw new NotImplementedException();

		private Intent playIntent;
		private bool musicBound = false;
		private MusicService musicSvr;

	}
}

