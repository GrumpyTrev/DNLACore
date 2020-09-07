using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	/// <summary>
	/// The SelectionBaseDialogFragment is used as a base class for all dialogues that need to 
	/// select a library to perform some action on
	/// </summary>
	internal abstract class SelectionBaseDialogFragment : DialogFragment
	{
		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SelectionBaseDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			return new AlertDialog.Builder( Activity )
				.SetTitle( Title )
				.SetSingleChoiceItems( Libraries.LibraryNames.ToArray(), InitallySelectedLibraryIndex, delegate 
				{
					// Enable the OK button once a selection has been made
					( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Enabled = true;
				} )
				.SetPositiveButton( "Ok", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
		}

		/// <summary>
		/// Install a handler for the Ok button that gets the selected item from the internal ListView
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			AlertDialog alert = ( AlertDialog )Dialog;

			// If a library has not been selected yet then keep the OK button disabled
			Button okButton = alert.GetButton( ( int )DialogButtonType.Positive );
			okButton.Enabled = ( alert.ListView.CheckedItemPosition >= 0 );

			// Install the handler
			okButton.Click += ( sender, args ) =>
			{
				LibrarySelected( Libraries.LibraryCollection[ alert.ListView.CheckedItemPosition ] );
				alert.Dismiss();
			};
		}

		/// <summary>
		/// Dialogue title to be set in derived classes
		/// </summary>
		protected virtual string Title { get; } = "";

		/// <summary>
		/// The index of the library to initially display selected
		/// </summary>
		protected virtual int InitallySelectedLibraryIndex { get; } = -1;

		/// <summary>
		/// Carry out the action once a library has been selected
		/// </summary>
		/// <param name="libraryToClear"></param>
		protected abstract void LibrarySelected( Library library );
	}
}