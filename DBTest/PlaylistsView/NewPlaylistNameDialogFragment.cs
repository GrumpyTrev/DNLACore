using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
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
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, NameEntered nameCallback, string dialogTitle, int layoutResource, string playlistName )
		{
			reporter = nameCallback;
			title = dialogTitle;
			layout = layoutResource;
			name = playlistName;

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
			View editView = LayoutInflater.From( Context ).Inflate( Resource.Layout.new_playlist_dialogue_layout, null );
			playListName = editView.FindViewById<EditText>( Resource.Id.playlistName );

			if ( ( savedInstanceState == null ) && ( name.Length > 0 ) )
			{
				playListName.Text = name;
			}

			return new AlertDialog.Builder( Context )
				.SetTitle( title )
				.SetView( editView )
				.SetPositiveButton( "Ok", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate {
					// If the media playback control is displayed the keyboard will remain visible, so explicitly get rid of it
					InputMethodManager.FromContext( Context )?.HideSoftInputFromWindow( playListName.WindowToken, HideSoftInputFlags.None );
				} )
				.Create(); ;
		}

		/// <summary>
		/// Install handlers for the Ok button when the dialogue is displayed
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;

			// Install a handler for the Ok button that performs the validation and playlist creation
			alert.GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) =>
			{
				reporter?.Invoke( playListName.Text, this );
			};
		}

		/// <summary>
		/// Called by the command handler to dismiss the dialog, and to close any open input service
		/// </summary>
		public override void Dismiss()
		{
			// If the media playback control is displayed the keyboard will remain visible, so explicitly get rid of it
			InputMethodManager.FromContext( Context )?.HideSoftInputFromWindow( playListName.WindowToken, HideSoftInputFlags.None );

			base.Dismiss();
		}

		/// <summary>
		/// The delegate used to report back the playlist name
		/// </summary>
		private static NameEntered reporter = null;

		/// <summary>
		/// Delegate type used to report back the playlist name
		/// </summary>
		public delegate void NameEntered( string playlistName, NewPlaylistNameDialogFragment playlistNameFragment );

		/// <summary>
		/// The control holding the name of the new playlist
		/// </summary>
		private EditText playListName = null;

		/// <summary>
		/// The title for this dialogue
		/// </summary>
		private static string title = "";

		/// <summary>
		/// The layout resource for the dialogue
		/// </summary>
		private static int layout = -1;

		/// <summary>
		/// The name to preload the playlist name field with
		/// </summary>
		private static string name = "";
	}
}