using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Media.Session;
using CoreMP;
using static Android.Support.V4.Media.App.NotificationCompat;

namespace DBTest
{
	/// <summary>
	/// The MediaNotificationService is used to display notifications of the currently playing song and to respond to controls from the notification
	/// </summary>
	[Service]
	internal class MediaNotificationService : Service
	{
		/// <summary>
		/// Called when the service has been created to return the IBinder instance for the service
		/// </summary>
		/// <param name="intent"></param>
		/// <returns></returns>
		public override IBinder OnBind( Intent _ ) => new MediaNotificationServiceBinder( this );

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
			// One time initialisation 
			if ( mediaStyle == null )
			{
				InitialiseNotification();
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
		/// Initialise the bits and bobs used for the notification
		/// </summary>
		private void InitialiseNotification()
		{
			// The mediaButtonReceiver parameter is required for pre-Lollipop SDK
			ComponentName mediaButtonReceiver = new( this, Java.Lang.Class.FromType( typeof( MediaButtonReceiver ) ) );

			MediaSessionCompat mediaSession = new( this, AudioPlayerId, mediaButtonReceiver, null ) { Active = true };
			mediaStyle = new MediaStyle().SetMediaSession( mediaSession.SessionToken ).SetShowActionsInCompactView( 0 );

			// The play and pause actions to be triggered when the icon is clicked
			playAction = new NotificationCompat.Action( Android.Resource.Drawable.IcMediaPlay, "play", PlaybackAction( PlayActionName ) );
			pauseAction = new NotificationCompat.Action( Android.Resource.Drawable.IcMediaPause, "pause", PlaybackAction( PauseActionName ) );

			if ( Build.VERSION.SdkInt >= BuildVersionCodes.O )
			{
				NotificationChannel channel = new( AudioPlayerId, ChannelName, NotificationImportance.Low ) { Description = ChannelDescription };
				NotificationManager.FromContext( this ).CreateNotificationChannel( channel );
			}
		}

		/// <summary>
		/// Display a notification using the current song and its playing status
		/// </summary>
		private void UpdateNotification()
		{
			if ( songBeingPlayed != null )
			{
				// Build and display the notification
				// This notification causes the emittion of a warning by the android system. This is due to a problem with the 
				// support library that cannot be circumvented.
				NotificationCompat.Builder builder = new NotificationCompat.Builder( this, AudioPlayerId )
					.SetShowWhen( false )
					.SetStyle( mediaStyle )
					.SetSmallIcon( Android.Resource.Drawable.StatSysHeadset )
					.SetContentTitle( songBeingPlayed.Title )
					.SetContentText( ( songBeingPlayed.Artist != null ) ? songBeingPlayed.Artist.Name : "" )
					.SetOngoing( true )
					.AddAction( ( SongPlaying == false ) ? playAction : pauseAction );

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
					Reporter?.MediaPlay();
				}
				else if ( playBackAction.Action == PauseActionName )
				{
					Reporter?.MediaPause();
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
		/// <remarks>
		/// Create the binder and save the service instance
		/// </remarks>
		/// <param name="theService"></param>
		public class MediaNotificationServiceBinder( MediaNotificationService theService ) : Binder
		{

			/// <summary>
			/// The service instance passed back to the application
			/// </summary>
			public MediaNotificationService Service { get; } = theService;
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
		/// The MediaStyle to be supplied to the notification
		/// </summary>
		private MediaStyle mediaStyle = null;

		/// <summary>
		/// The Notification actions
		/// </summary>
		private NotificationCompat.Action playAction = null;
		private NotificationCompat.Action pauseAction = null;

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
