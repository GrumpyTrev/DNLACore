using Android.Animation;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace DBTest
{
	class MediaControllerView : BaseBoundControl, View.IOnClickListener, MediaControllerController.IMediaReporter
	{
		/// <summary>
		/// Bind to the specified view.
		/// </summary>
		/// <param name="menu"></param>
		public override void BindToView( View view, Context context )
		{
			if ( view != null )
			{
				mediaControllerLayout = view.FindViewById<LinearLayout>( Resource.Id.media_controller_layout );
				collapsedLayout = view.FindViewById<LinearLayout>( Resource.Id.collapsed_layout );
				expandedLayout = view.FindViewById<LinearLayout>( Resource.Id.expanded_layout );

				textView = view.FindViewById<TextView>( Resource.Id.long_text );
				textView.Selected = true;

				collapsedProgress = collapsedLayout.FindViewById<ProgressBar>( Resource.Id.progressBar );
				expandedProgress = expandedLayout.FindViewById<ProgressBar>( Resource.Id.progressBar );

				expandedDuration = expandedLayout.FindViewById<TextView>( Resource.Id.textDuration );
				expandedPosition = expandedLayout.FindViewById<TextView>( Resource.Id.textCurrentPosition );

				if ( collapsed == false )
				{
					collapsedLayout.Visibility = ViewStates.Gone;
					expandedLayout.Visibility = ViewStates.Visible;
				}

				collapsedLayout.SetOnClickListener( this );
				expandedLayout.SetOnClickListener( this );

				ImageButton playButton = view.FindViewById<ImageButton>( Resource.Id.play );
				playButton.Click += ( sender, args) => { };
				ImageButton nextButton = view.FindViewById<ImageButton>( Resource.Id.skip_next );
				nextButton.Click += ( sender, args ) => { };
				ImageButton prevButton = view.FindViewById<ImageButton>( Resource.Id.skip_prev );
				prevButton.Click += ( sender, args ) => { };

				MediaControllerController.DataReporter = this;

				// Create a handler for UI switching
				handler = new Handler( context.MainLooper );
			}
		}

		public void OnClick( View v )
		{
			if ( collapsed == true )
			{
				collapsedLayout.Animate()
					.Alpha( 0 )
					.SetDuration( 500 )
					.SetListener( new CollapseViewListener() { ViewToHide = collapsedLayout, ViewToShow = expandedLayout } );
			}
			else
			{
				expandedLayout.Animate()
					.Alpha( 0 )
					.SetDuration( 500 )
					.SetListener( new CollapseViewListener() { ViewToHide = expandedLayout, ViewToShow = collapsedLayout } );
			}

			collapsed = !collapsed;
		}

		public void DeviceAvailable( bool available )
		{
		}

		public void PlayStateChanged()
		{
		}

		public void ShowMediaControls()
		{
		}

		public void MediaProgress()
		{
			SetProgress( MediaControllerViewModel.Duration, MediaControllerViewModel.CurrentPosition );
		}

		public void DataAvailable()
		{
			SetProgress( MediaControllerViewModel.Duration, MediaControllerViewModel.CurrentPosition );
		}

		private void SetProgress( int duration, int progress )
		{
			handler.Post( () =>
			{
					collapsedProgress.Progress = ( duration > 0 ) ? ( 100 * progress ) / duration : 0;
					expandedProgress.Progress = ( duration > 0 ) ? ( 100 * progress ) / duration : 0;
			} );
		}


		private class CollapseViewListener : Java.Lang.Object, Animator.IAnimatorListener
		{
			public View ViewToHide { get; set; }
			public View ViewToShow { get; set; }

			public void OnAnimationCancel( Animator animation )
			{
			}

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

			public void OnAnimationRepeat( Animator animation )
			{
			}

			public void OnAnimationStart( Animator animation )
			{
			}
		}

		private bool collapsed = true;

		private LinearLayout mediaControllerLayout = null;
		private LinearLayout collapsedLayout = null;
		private LinearLayout expandedLayout = null;
		private TextView textView = null;

		private TextView expandedPosition = null;
		private TextView expandedDuration = null;

		private ProgressBar expandedProgress = null;
		private ProgressBar collapsedProgress = null;

		/// <summary>
		/// The Handler used for UI switching
		/// </summary>
		private Handler handler = null;
	}
}