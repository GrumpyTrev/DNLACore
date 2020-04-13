using Android.Support.V7.App;

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
		protected override void ProcessTagCommand( string name ) => 
			DeleteConfirmationDialogFragment.ShowFragment( Context.SupportFragmentManager, string.Format( "Are you sure you want to delete tag: {0}", name ), name );
	}
}