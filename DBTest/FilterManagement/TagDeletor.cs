using Android.App;
using Android.OS;
using Android.Support.V7.App;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	class TagDeletor: TagCommandHandler
	{
		/// <summary>
		/// Public constructor providing the context for the dialogues
		/// </summary>
		/// <param name="activityContext"></param>
		public TagDeletor( AppCompatActivity activityContext ) : base( activityContext )
		{
		}

		/// <summary>
		/// Process the tag command
		/// </summary>
		/// <param name="name"></param>
		protected override void ProcessTagCommand( string name )
		{
			DeleteConfirmationDialogFragment.NewInstance( string.Format( "Are you sure you want to delete tag: {0}", name ), name )
				.Show( Context.SupportFragmentManager, "fragment_delete_tag" );
		}
	}

	/// <summary>
	/// Dialogue reporting some kind of problem with the requested action
	/// </summary>
	internal class DeleteConfirmationDialogFragment: DialogFragment
	{
		/// <summary>
		/// Create a DeleteConfirmationDialogFragment with the specified arguments
		/// </summary>
		/// <param name="title"></param>
		/// <param name="tagName"></param>
		/// <returns></returns>
		public static DeleteConfirmationDialogFragment NewInstance( string title, string tagName )
		{
			DeleteConfirmationDialogFragment dialog = new DeleteConfirmationDialogFragment { Arguments = new Bundle() };
			dialog.Arguments.PutString( "title", title );
			dialog.Arguments.PutString( "tag", tagName );

			return dialog;
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
					Tag tagToDelete = FilterManagementController.GetTagFromName( Arguments.GetString( "tag", "" ) );
					if ( tagToDelete != null )
					{
						FilterManagementController.DeleteTagAsync( tagToDelete );
					}
				} )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
	}
}