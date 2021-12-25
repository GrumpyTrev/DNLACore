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
	/// New library name dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class NewLibraryNameDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, NameEntered nameCallback, string dialogTitle, string libraryName )
		{
			reporter = nameCallback;
			title = dialogTitle;
			name = libraryName;

			new NewLibraryNameDialogFragment().Show( manager, "fragment_new_library_name" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public NewLibraryNameDialogFragment()
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
			View editView = LayoutInflater.From( Context ).Inflate( Resource.Layout.new_library_dialogue_layout, null );
			libraryName = editView.FindViewById<EditText>( Resource.Id.libraryName );

			// If not restoring initialise the playlist name and the checkbox content
			if ( savedInstanceState == null )
			{
				if ( name.Length > 0 )
				{
					libraryName.Text = name;
				}
			}

			return new AlertDialog.Builder( Context )
				.SetTitle( title )
				.SetView( editView )
				.SetPositiveButton( "Ok", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate {
					// If the media playback control is displayed the keyboard will remain visible, so explicitly get rid of it
					InputMethodManager.FromContext( Context )?.HideSoftInputFromWindow( libraryName.WindowToken, HideSoftInputFlags.None );
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
			alert.GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) => reporter?.Invoke( libraryName.Text, this );
		}

		/// <summary>
		/// Called by the command handler to dismiss the dialog, and to close any open input service
		/// </summary>
		public override void Dismiss()
		{
			// If the media playback control is displayed the keyboard will remain visible, so explicitly get rid of it
			InputMethodManager.FromContext( Context )?.HideSoftInputFromWindow( libraryName.WindowToken, HideSoftInputFlags.None );

			base.Dismiss();
		}

		/// <summary>
		/// The delegate used to report back the library name
		/// </summary>
		private static NameEntered reporter = null;

		/// <summary>
		/// Delegate type used to report back the library name
		/// </summary>
		public delegate void NameEntered( string libraryName, NewLibraryNameDialogFragment libraryNameFragment );

		/// <summary>
		/// The control holding the name of the new playlist
		/// </summary>
		private EditText libraryName = null;

		/// <summary>
		/// The title for this dialogue
		/// </summary>
		private static string title = "";

		/// <summary>
		/// The name to preload the library name field with
		/// </summary>
		private static string name = "";
	}
}
