using Android.Support.V7.App;

namespace DBTest
{
	/// <summary>
	/// The TagEditor class is used to handle tag edit commands
	/// </summary>
	class TagEditor : TagCommandHandler
	{
		/// <summary>
		/// Public constructor providing the context for the dialogues
		/// </summary>
		/// <param name="activityContext"></param>
		public TagEditor( AppCompatActivity activityContext ) : base( activityContext )
		{
		}

		/// <summary>
		/// Process the tag command
		/// Determine which tag is being edited and display the edit dialogue for that tag
		/// </summary>
		/// <param name="name"></param>
		protected override void ProcessTagCommand( string name )
		{
			TagEditorDialogFragment.ShowFragment( Context.SupportFragmentManager, "Edit tag details", name );
		}
	}

	/// <summary>
	/// The TagCreator class is used to handle new tag creation commands
	/// </summary>
	static class TagCreator
	{
		/// <summary>
		/// Allow the user to create a new tag
		/// </summary>
		public static void AddNewTag() => TagEditorDialogFragment.ShowFragment( CommandRouter.Manager, "New tag details", "" );
	}
}