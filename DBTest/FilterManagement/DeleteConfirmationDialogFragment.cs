using Android.App;
using Android.OS;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Dialogue reporting some kind of problem with the requested action
	/// </summary>
	internal class DeleteConfirmationDialogFragment: DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the specified list of tags and the current tag
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, string title, string tagName )
		{
			DeleteConfirmationDialogFragment dialog = new DeleteConfirmationDialogFragment { Arguments = new Bundle() };
			dialog.Arguments.PutString( "title", title );
			dialog.Arguments.PutString( "tag", tagName );

			dialog.Show( manager, "fragment_delete_tag" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public DeleteConfirmationDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder( Activity )
				.SetTitle( Arguments.GetString( "title", "" ) )
				.SetPositiveButton( "OK", delegate 
				{
					Tag tagToDelete = Tags.GetTagByName( Arguments.GetString( "tag", "" ) );
					if ( tagToDelete != null )
					{
						FilterManagementController.DeleteTag( tagToDelete );
					}
				} )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
	}
}