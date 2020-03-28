using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.InputMethods;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// New playlist name dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class NewPlaylistNameDialogFragment : DialogFragment
	{
		public static void ShowFragment( FragmentManager manager )
		{
			new NewPlaylistNameDialogFragment().Show( manager, "fragment_new_playlist_name" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public NewPlaylistNameDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Show a dialogue asking for a new playlist name. Don't install handlers for Ok/Cancel yet.
			// This prevents the default Dismiss action after the buttons are clicked
			playListName = new EditText( Context ) { Hint = "Enter new playlist name" };

			// If this dialog is being restored then get the saved playlist name
			if ( savedInstanceState != null )
			{
				playListName.Text = savedInstanceState.GetString( PlaylistNameTag );
			}

			AlertDialog alert = new AlertDialog.Builder( Context )
				.SetTitle( "New playlist" )
				.SetView( playListName )
				.SetPositiveButton( "Ok", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", ( EventHandler<DialogClickEventArgs> )null )
				.Create();

			return alert;
		}

		/// <summary>
		/// Install handlers for the Ok and Cancel buttons when the dialogue is displayed
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;

			// Install a handler for the Ok button that performs the validation and playlist creation
			alert.GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) =>
			{
				string alertText = "";

				if ( playListName.Text.Length == 0 )
				{
					alertText = "An empty name is not valid.";
				}
				else if ( PlaylistsViewModel.PlaylistNames.Contains( playListName.Text ) == true )
				{
					alertText = "A playlist with that name already exists.";
				}
				else
				{
					PlaylistsController.AddPlaylistAsync( playListName.Text );

					// If the media playback control is displayed the keyboard will remain visible, so explicitly get rid of it
					InputMethodManager imm = ( InputMethodManager )Context.GetSystemService( Context.InputMethodService );
					imm.HideSoftInputFromWindow( playListName.WindowToken, 0 );

					alert.Dismiss();
				}

				// Display an error message if the playlist name is not valid. Do not dismiss the dialog
				if ( alertText.Length > 0 )
				{
					NotificationDialogFragment.ShowFragment( Activity.SupportFragmentManager, alertText );
				}
			};

			// Install a handler for the cancel button so that the keyboard can be explicitly hidden
			alert.GetButton( ( int )DialogButtonType.Negative ).Click += ( sender, args ) =>
			{
				// If the media playback control is displayed the keyboard will remain visible, so explicitly get rid of it
				InputMethodManager imm = ( InputMethodManager )Context.GetSystemService( Context.InputMethodService );
				imm.HideSoftInputFromWindow( playListName.WindowToken, 0 );

				alert.Dismiss();
			};
		}

		/// <summary>
		/// Override the base method in order to save the current playlist name
		/// </summary>
		/// <param name="outState"></param>
		public override void OnSaveInstanceState( Bundle outState )
		{
			base.OnSaveInstanceState( outState );
			outState.PutString( PlaylistNameTag, playListName.Text );
		}

		/// <summary>
		/// The control holding the name of the new playlist
		/// </summary>
		private EditText playListName = null;

		/// <summary>
		/// Storage name for the playlist
		/// </summary>
		private const string PlaylistNameTag = "playlistName";
	}
}