using Android.Views;
using Android.Widget;
using CoreMP;
using System;

namespace DBTest
{
	internal class MediaControlsView
	{
		/// <summary>
		/// Bind to the specified view.
		/// </summary>
		/// <param name="menu"></param>
		public void BindToView( View parentView, int viewResource )
		{
			if ( parentView != null )
			{
				// Get a reference to the top level layout control so that it's visibility can be changed
				mainLayout = parentView.FindViewById<ViewGroup>( viewResource );

				// Get a reference to the song title text view and set it selected to start the marquee going
				songTitle = parentView.FindViewById<TextView>( Resource.Id.long_text );

				if ( songTitle != null )
				{
					songTitle.Selected = true;
					songTitle.Text = "";
				}

				artistName = parentView.FindViewById<TextView>( Resource.Id.artistName );
				artistName?.SetText( "", null );

				// Get references to the other controls
				songProgress = mainLayout.FindViewById<ProgressBar>( Resource.Id.progressBar );
				position = mainLayout.FindViewById<TextView>( Resource.Id.textCurrentPosition );
				duration = mainLayout.FindViewById<TextView>( Resource.Id.textDuration );

				// Respond to play/pause buitton presses
				playButton = mainLayout.FindViewById<ImageButton>( Resource.Id.play );
				playButton.Click += ( _, _ ) => 
				{
					if ( MediaControllerViewModel.IsPlaying == true )
					{
						MainApp.CommandInterface.Pause();
					}
					else
					{
						MainApp.CommandInterface.Start();
					}
				};

				// Process play next button clicks
				if ( mainLayout.FindViewById<ImageButton>( Resource.Id.skip_next ) != null)
				{
					mainLayout.FindViewById<ImageButton>( Resource.Id.skip_next ).Click += ( _, _ ) => MainApp.CommandInterface.PlayNext();
				}

				// Process play previous button clicks
				if ( mainLayout.FindViewById<ImageButton>( Resource.Id.skip_prev ) != null )
				{
					mainLayout.FindViewById<ImageButton>( Resource.Id.skip_prev ).Click += ( _, _ ) => MainApp.CommandInterface.PlayPrevious();
				}

				// Save Repeat and Shuffle buttons for later mode updates
				repeatButton = mainLayout.FindViewById<ImageButton>( Resource.Id.repeat );
				shuffleButton = mainLayout.FindViewById<ImageButton>( Resource.Id.shuffle );

				// Process repeat and shuffle clicks if the buttons exist
				if ( repeatButton != null )
				{
					repeatButton.Click += ( _, _ ) => MainApp.CommandInterface.SetRepeat( !MediaControllerViewModel.RepeatOn );
				}

				if ( shuffleButton != null )
				{
					shuffleButton.Click += ( _, _ ) => MainApp.CommandInterface.SetShuffle( !MediaControllerViewModel.ShuffleOn );
				}

				// Display the appropriate playing/not playing icons 
				PlayStateChanged();

				// Register interest in MediaControllerViewModel changes
				NotificationHandler.Register<MediaControllerViewModel>( ModelDataAvailable, InstanceId.ToString() );
				NotificationHandler.Register<MediaControllerViewModel>( nameof( MediaControllerViewModel.IsPlaying ), PlayStateChanged, InstanceId.ToString() );
				NotificationHandler.Register<MediaControllerViewModel>( nameof( MediaControllerViewModel.CurrentPosition), SetProgress, InstanceId.ToString() );
				NotificationHandler.Register<MediaControllerViewModel>( nameof( MediaControllerViewModel.SongPlaying ), SongPlaying, InstanceId.ToString() );
				NotificationHandler.Register<MediaControllerViewModel>( nameof( MediaControllerViewModel.RepeatOn ), RepeatChanged, InstanceId.ToString() );
				NotificationHandler.Register<MediaControllerViewModel>( nameof( MediaControllerViewModel.ShuffleOn ), ShuffleChanged, InstanceId.ToString() );
			}
			else
			{
				// Degister interest in MediaControllerViewModel changes
				NotificationHandler.Deregister( InstanceId.ToString() );
			}
		}

		/// <summary>
		/// Called when the play state has changed
		/// Display either the play or pause buttons
		/// </summary>
		private void PlayStateChanged() => 
			playButton.SetImageResource( ( MediaControllerViewModel.IsPlaying == true ) ? Resource.Drawable.pause : Resource.Drawable.play );

		private void RepeatChanged() => 
			repeatButton?.SetImageResource( ( MediaControllerViewModel.RepeatOn == true ) ? Resource.Drawable.repeat_on : Resource.Drawable.repeat );

		private void ShuffleChanged() =>
			shuffleButton?.SetImageResource( ( MediaControllerViewModel.ShuffleOn == true ) ? Resource.Drawable.shuffle_on : Resource.Drawable.shuffle );

		/// <summary>
		/// Update the progress controls
		/// </summary>
		private void SetProgress()
		{
			songProgress.Progress = ( MediaControllerViewModel.Duration > 0 ) ? 100 * MediaControllerViewModel.CurrentPosition / MediaControllerViewModel.Duration : 0;
			position.Text = TimeSpan.FromMilliseconds( MediaControllerViewModel.CurrentPosition ).ToString( @"mm\:ss" );
			duration?.SetText( TimeSpan.FromMilliseconds( MediaControllerViewModel.Duration ).ToString( @"mm\:ss" ), null );
		}

		/// <summary>
		/// Update the details of the song being played
		/// </summary>
		private void SongPlaying()
		{
			if ( MediaControllerViewModel.SongPlaying == null )
			{
				songTitle?.SetText("", null);
				artistName?.SetText( "", null );
				SetProgress();
			}
			else
			{
				songTitle?.SetText( MediaControllerViewModel.SongPlaying.Title, null );
				artistName?.SetText( MediaControllerViewModel.SongPlaying.Artist.Name, null );
			}
		}

		/// <summary>
		/// Called when the MediaControllerViewModel data is available
		/// </summary>
		private void ModelDataAvailable()
		{
			SetProgress();
			PlayStateChanged();
		}

		/// <summary>
		/// Get a unique Id for this instance
		/// </summary>
		private Guid InstanceId { get; } = Guid.NewGuid();

		/// <summary>
		/// Cached controls from the media controls view
		/// </summary>
		private ViewGroup mainLayout = null;
		private TextView songTitle = null;
		private TextView artistName = null;
		private TextView position = null;
		private TextView duration = null;
		private ProgressBar songProgress = null;
		private ImageButton playButton = null;
		private ImageButton repeatButton = null;
		private ImageButton shuffleButton = null;
	}
}
