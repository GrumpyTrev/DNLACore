using Android.Animation;
using Android.Content;
using Android.Views;
using Android.Widget;
using CoreMP;
using System;

namespace DBTest
{
	internal class MediaControllerView : BaseBoundControl, View.IOnClickListener
	{
		/// <summary>
		/// Bind to the specified view.
		/// </summary>
		/// <param name="menu"></param>
		public override void BindToView( View view, Context _ )
		{
			if ( view != null )
			{
				// Get references to the collapsed and expanded layouts
				collapsedLayout = view.FindViewById<LinearLayout>( Resource.Id.collapsed_layout );
				expandedLayout = view.FindViewById<LinearLayout>( Resource.Id.expanded_layout );

				// Get a reference to the song title text view and set it selected to start the marquee going
				songTitle = view.FindViewById<TextView>( Resource.Id.long_text );
				songTitle.Selected = true;
				songTitle.Text = "";

				// Get references to the other controls
				collapsedProgress = collapsedLayout.FindViewById<ProgressBar>( Resource.Id.progressBar );
				expandedProgress = expandedLayout.FindViewById<ProgressBar>( Resource.Id.progressBar );
				duration = expandedLayout.FindViewById<TextView>( Resource.Id.textDuration );
				position = expandedLayout.FindViewById<TextView>( Resource.Id.textCurrentPosition );

				// Respond to non-command layout clicks to collapse and expande the view
				collapsedLayout.SetOnClickListener( this );
				expandedLayout.SetOnClickListener( this );

				// Respond to play/pause buitton presses
				playButton = view.FindViewById<ImageButton>( Resource.Id.play );
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
				view.FindViewById<ImageButton>( Resource.Id.skip_next ).Click += ( _, _ ) => MainApp.CommandInterface.PlayNext();

				// Process play previous button clicks
				view.FindViewById<ImageButton>( Resource.Id.skip_prev ).Click += ( _, _ ) => MainApp.CommandInterface.PlayPrevious();

				// Assume no playback device available at startup. Hide everything
				DeviceAvailable();

				// Display the appropriate playing/not playing icons 
				PlayStateChanged();

				// Register interest in MediaControllerViewModel changes
				NotificationHandler.Register( typeof( MediaControllerViewModel ), "IsSet", () =>
				{
					// Model data is available
					SetProgress( MediaControllerViewModel.Duration, MediaControllerViewModel.CurrentPosition );
					PlayStateChanged();
					DeviceAvailable();
				} );

				NotificationHandler.Register( typeof( MediaControllerViewModel ), "PlaybackDeviceAvailable", DeviceAvailable );
				NotificationHandler.Register( typeof( MediaControllerViewModel ), "IsPlaying", PlayStateChanged );
				NotificationHandler.Register( typeof( MediaControllerViewModel ), "CurrentPosition", 
					() => SetProgress( MediaControllerViewModel.Duration, MediaControllerViewModel.CurrentPosition ) );
				NotificationHandler.Register( typeof( MediaControllerViewModel ), "SongPlaying", () =>
				{
					if ( MediaControllerViewModel.SongPlaying == null )
					{
						songTitle.Text = "";
						SetProgress( 0, 0 );
					}
					else
					{
						songTitle.Text = string.Format( "{0} : {1}", MediaControllerViewModel.SongPlaying.Title, MediaControllerViewModel.SongPlaying.Artist.Name );
					}
				} );
			}
			else
			{
				// Degister interest in MediaControllerViewModel changes
				NotificationHandler.Deregister();
			}
		}

		/// <summary>
		/// Called when the view is clicked.
		/// Change from the expanded to collapsed layaout according to the current state.
		/// Use an animation to perform the change
		/// </summary>
		/// <param name="_"></param>
		public void OnClick( View _ )
		{
			if ( collapsed == true )
			{
				// Fade the collapsed layout and then show the expanded layout 
				collapsedLayout.Animate()
					.Alpha( 0 )
					.SetDuration( 500 )
					.SetListener( new CollapseViewListener() { ViewToHide = collapsedLayout, ViewToShow = expandedLayout } );
			}
			else
			{
				// Fade the expanded layout and then show the collapsed layout
				expandedLayout.Animate()
					.Alpha( 0 )
					.SetDuration( 500 )
					.SetListener( new CollapseViewListener() { ViewToHide = expandedLayout, ViewToShow = collapsedLayout } );
			}

			// Toggle the collpased state
			collapsed = !collapsed;
		}

		/// <summary>
		/// Called when the choosen playback device is either detected as either available or not available
		/// </summary>
		private void DeviceAvailable()
		{
			if ( MediaControllerViewModel.PlaybackDeviceAvailable == true )
			{
				collapsedLayout.Visibility = ( collapsed == false ) ? ViewStates.Gone : ViewStates.Visible;
				expandedLayout.Visibility = ( collapsed == false ) ? ViewStates.Visible : ViewStates.Gone;
			}
			else
			{
				collapsedLayout.Visibility = ViewStates.Gone;
				expandedLayout.Visibility = ViewStates.Gone;
			}
		}

		/// <summary>
		/// Called when the play state has changed
		/// Display either the play or pause buttons
		/// </summary>
		private void PlayStateChanged() => 
			playButton.SetImageResource( ( MediaControllerViewModel.IsPlaying == true ) ? Resource.Drawable.pause : Resource.Drawable.play );

		/// <summary>
		/// Update the progress controls
		/// </summary>
		/// <param name="length"></param>
		/// <param name="progress"></param>
		private void SetProgress( int length, int progress )
		{
			collapsedProgress.Progress = ( length > 0 ) ? ( 100 * progress ) / length : 0;
			expandedProgress.Progress = ( length > 0 ) ? ( 100 * progress ) / length : 0;
			position.Text = TimeSpan.FromMilliseconds( progress ).ToString( @"mm\:ss" );
			duration.Text = TimeSpan.FromMilliseconds( length ).ToString( @"mm\:ss" );
		}

		/// <summary>
		/// Class used to carry out the fade in animation at the end of the fade out
		/// </summary>
		private class CollapseViewListener : Java.Lang.Object, Animator.IAnimatorListener
		{
			public void OnAnimationCancel( Animator animation ) { }
			public void OnAnimationRepeat( Animator animation )	{ }
			public void OnAnimationStart( Animator animation ) { }

			/// <summary>
			/// Called when the layout being faded out has been faded out. Hide it and fade in the new layout
			/// </summary>
			/// <param name="animation"></param>
			public void OnAnimationEnd( Animator animation )
			{
				ViewToHide.Visibility = ViewStates.Gone;

				ViewToShow.Alpha = 0;
				ViewToShow.Visibility = ViewStates.Visible;
				ViewToShow.Animate()
					.Alpha( 1 )
					.SetDuration( 500 )
					.SetListener( null );
			}

			public View ViewToHide { get; set; }
			public View ViewToShow { get; set; }
		}

		/// <summary>
		/// Is the media controller being show full size or collapsed
		/// </summary>
		private bool collapsed = true;

		/// <summary>
		/// Cached controls from the media controller view
		/// </summary>
		private LinearLayout collapsedLayout = null;
		private LinearLayout expandedLayout = null;
		private TextView songTitle = null;

		private TextView position = null;
		private TextView duration = null;

		private ProgressBar expandedProgress = null;
		private ProgressBar collapsedProgress = null;

		private ImageButton playButton = null;
	}
}
