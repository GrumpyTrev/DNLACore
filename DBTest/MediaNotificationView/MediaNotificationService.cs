using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using static Android.Support.V4.Media.Session.MediaControllerCompat;

namespace DBTest
{
	/// <summary>
	/// The MediaNotificationService is used to display notifications of the currently playing song and to respond to controls from the notification
	/// </summary>
	[Service]
	class MediaNotificationService : Service
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

			serviceBinder = new MediaNotificationServiceBinder( this );
		}

		/// <summary>
		/// Called when the service is no longer required
		/// Remove the notification and stop the service
		/// </summary>
		public void Stop()
		{
			RemoveNotification();
			StopSelf();
		}

		/// <summary>
		/// Called when the service is first started and in response to controls from the notification
		/// </summary>
		/// <param name="intent"></param>
		/// <param name="flags"></param>
		/// <param name="startId"></param>
		/// <returns></returns>
		[return: GeneratedEnum]
		public override StartCommandResult OnStartCommand( Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId )
		{
			// One time initialisation of the MediaSesson instance
			if ( mediaSession == null )
			{
				InitMediaSession();
			}

			// Handle any incoming controls
			HandleIncomingActions( intent );

			return base.OnStartCommand( intent, flags, startId );
		}

		/// <summary>
		/// Called when a new song is being played. Update the notification
		/// </summary>
		/// <param name="songPlayed"></param>
		public void SongStarted( Song songPlayed )
		{
			songBeingPlayed = songPlayed;
			UpdateNotification();
		}

		/// <summary>
		/// Called when a song has finished playing
		/// </summary>
		public void SongFinished()
		{
			SongPlaying = false;
			RemoveNotification();
		}

		/// <summary>
		/// Called when the song is paused or played. Update the notification
		/// </summary>
		/// <param name="playing"></param>
		public void IsPlaying( bool playing )
		{
			SongPlaying = playing;
			UpdateNotification();
		}

		/// <summary>
		/// Called when the media played is about to stop
		/// </summary>
		public void PlayStopped()
		{
			SongPlaying = false;
			RemoveNotification();
		}

		/// <summary>
		/// Initialise the MediaSession used to route commands and initialise the notification channel
		/// </summary>
		private void InitMediaSession()
		{
			// The mediaButtonReceiver parameter is required for pre-Lollipop SDK
			ComponentName mediaButtonReceiver = new ComponentName( this, Java.Lang.Class.FromType( typeof( MediaButtonReceiver ) ) );

			mediaSession = new MediaSessionCompat( this, AudioPlayerId, mediaButtonReceiver, null ) { Active = true };
			mediaSession.SetFlags( MediaSessionCompat.FlagHandlesTransportControls );

			transportControls = mediaSession.Controller.GetTransportControls();

			mediaSession.SetCallback( new MediaControlCallback() { Service = this } );

			if ( Build.VERSION.SdkInt >= BuildVersionCodes.O )
			{
				NotificationChannel channel = new NotificationChannel( AudioPlayerId, ChannelName, NotificationImportance.Low )
					{ Description = ChannelDescription };
				( ( NotificationManager )GetSystemService( Context.NotificationService ) ).CreateNotificationChannel( channel );
			}
		}

		/// <summary>
		/// Display a notification using the current song and its playing status
		/// </summary>
		private void UpdateNotification()
		{
			if ( songBeingPlayed != null )
			{
				// Update the session data - not sure why
				mediaSession.SetMetadata( new MediaMetadataCompat.Builder()
					.PutString( MediaMetadataCompat.MetadataKeyTitle, songBeingPlayed.Title )
					.PutString( MediaMetadataCompat.MetadataKeyArtist, ( songBeingPlayed.Artist != null ) ? songBeingPlayed.Artist.Name : "" )
					.Build() );

				// Create an intent for the notification and an icon representing the action that can be performed
				PendingIntent playPauseAction = ( SongPlaying == false ) ? PlaybackAction( PlayActionName ) : PlaybackAction( PauseActionName );
				int notificationAction = ( SongPlaying == false ) ? Android.Resource.Drawable.IcMediaPlay : Android.Resource.Drawable.IcMediaPause;

				// Build and display the notification
				Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder( this, AudioPlayerId )
					.SetShowWhen( false )
					.SetStyle( new Android.Support.V4.Media.App.NotificationCompat.MediaStyle()
						.SetMediaSession( mediaSession.SessionToken )
						.SetShowActionsInCompactView( 0 ) )
					.SetSmallIcon( Android.Resource.Drawable.StatSysHeadset )
					.SetContentTitle( songBeingPlayed.Title )
					.SetContentText( ( songBeingPlayed.Artist != null ) ? songBeingPlayed.Artist.Name : "" )
					.AddAction( notificationAction, "pause", playPauseAction );

				NotificationManagerCompat.From( this ).Notify( NotificationId, builder.Build() );
			}
		}

		/// <summary>
		/// Remove the notification
		/// </summary>
		private void RemoveNotification() => NotificationManagerCompat.From( this ).Cancel( NotificationId );

		/// <summary>
		/// Pass on a request from notification to the associated MediaSession transport callback
		/// </summary>
		/// <param name="playBackAction"></param>
		private void HandleIncomingActions( Intent playBackAction )
		{
			if ( ( playBackAction != null ) && ( playBackAction.Action != null ) )
			{
				if ( playBackAction.Action == PlayActionName )
				{
					transportControls.Play();
				}
				else if ( playBackAction.Action == PauseActionName )
				{
					transportControls.Pause();
				}
			}
		}

		/// <summary>
		/// Create a PendingIntent containing the specified action name
		/// </summary>
		/// <param name="actionName"></param>
		/// <returns></returns>
		private PendingIntent PlaybackAction( string actionName ) => 
			PendingIntent.GetService( this, 0, new Intent( this, typeof( MediaNotificationService ) ).SetAction( actionName ), 0 );

		/// <summary>
		/// The instance used to report back significant events
		/// </summary>
		public IServiceCallbacks Reporter { get; set; } = null;

		/// <summary>
		/// The Binder class for this service defining the interface betweeen the service and the appication
		/// </summary>
		public class MediaNotificationServiceBinder: Binder
		{
			/// <summary>
			/// Create the binder and save the service instance
			/// </summary>
			/// <param name="theService"></param>
			public MediaNotificationServiceBinder( MediaNotificationService theService ) => Service = theService;

			/// <summary>
			/// The service instance passed back to the application
			/// </summary>
			public MediaNotificationService Service { get; } = null;
		}

		/// <summary>
		/// The interface defining the calls back to the application
		/// </summary>
		public interface IServiceCallbacks
		{
			void MediaPlay();
			void MediaPause();
		}

		/// <summary>
		/// THe MediaControlCallback class is used to route Media Session transport callbacks to the service's reporter
		/// </summary>
		private class MediaControlCallback: MediaSessionCompat.Callback
		{
			/// <summary>
			/// Pass on a Play control
			/// </summary>
			public override void OnPlay()
			{
				Service.Reporter?.MediaPlay();
			}

			/// <summary>
			/// Pass on a Pause control
			/// </summary>
			public override void OnPause()
			{
				Service.Reporter?.MediaPause();
			}

			/// <summary>
			/// The MediaNotificationService used to carry out the actions 
			/// </summary>
			public MediaNotificationService Service { private get; set; }
		}

		/// <summary>
		/// The IBinder instance for this service
		/// </summary>
		private IBinder serviceBinder = null;

		/// <summary>
		/// The MediaSession
		/// </summary>
		private MediaSessionCompat mediaSession = null;

		/// <summary>
		/// The TransportControls instance
		/// </summary>
		private TransportControls transportControls = null;

		/// <summary>
		/// The current song being played
		/// </summary>
		private Song songBeingPlayed = null;

		/// <summary>
		/// Is the song being played or is currently paused
		/// </summary>
		private bool SongPlaying { get; set; } = false;

		/// <summary>
		/// Id used for the media session and notification channel
		/// </summary>
		private const string AudioPlayerId = "AudioPlayer";

		/// <summary>
		/// The name of the notification channel
		/// </summary>
		private const string ChannelName = "Playing";

		/// <summary>
		/// The description of the channel
		/// </summary>
		private const string ChannelDescription = "Song notification";

		/// <summary>
		/// String ids for the two actions that can be performed from the notification
		/// </summary>
		private const string PlayActionName = "DNLACore_play";
		private const string PauseActionName = "DNLACore_Pause";

		/// <summary>
		/// Identity used for the notification
		/// </summary>
		private const int NotificationId = 999;
	}
}