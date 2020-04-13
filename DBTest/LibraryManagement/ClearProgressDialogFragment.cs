using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Display a progress dialogue whilst clearing the specified dialogue
	/// </summary>
	internal class ClearProgressDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Library libraryToClear )
		{
			// Save the library to clear so that it is available after a configuration change
			LibraryToClear = libraryToClear;

			new ClearProgressDialogFragment().Show( manager, "fragment_clear_progress" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public ClearProgressDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			ClearLibraryAsync();

			return new AlertDialog.Builder( Activity )
				.SetTitle( string.Format( "Clearing library: {0}", LibraryToClear.Name ) )
				.SetPositiveButton( "Ok", delegate { } )
				.SetCancelable( false )
				.Create();
		}

		/// <summary>
		/// Hide the OK button if the library is still being cleared
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			UpdateDialogueState();
		}

		/// <summary>
		/// Clear the selected library and then let the user know
		/// </summary>
		private async void ClearLibraryAsync()
		{
			LibraryBeingCleared = true;
			await LibraryAccess.ClearLibraryAsync( LibraryToClear );
			LibraryBeingCleared = false;

			UpdateDialogueState();
		}

		/// <summary>
		/// Update the state of the dialogue according to whether or not the library is still being cleared
		/// </summary>
		private void UpdateDialogueState()
		{
			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Visibility =
				( LibraryBeingCleared == true ) ? ViewStates.Invisible : ViewStates.Visible;

			if ( LibraryBeingCleared == false )
			{
				Dialog.SetTitle( string.Format( "Library: {0} cleared", LibraryToClear.Name ) );
			}
		}

		/// <summary>
		/// The library to clear
		/// </summary>
		private static Library LibraryToClear { get; set; } = null;

		/// <summary>
		/// Is the library being cleared
		/// </summary>
		private static bool LibraryBeingCleared { get; set; } = false;
	}
}