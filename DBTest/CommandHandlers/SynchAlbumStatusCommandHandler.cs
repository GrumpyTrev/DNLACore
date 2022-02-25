using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The SynchAlbumStatusCommandHandler class is used to synchronise the 'played' status of albums across all libraries.
	/// </summary>
	class SynchAlbumStatusCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// Confirm this command to make sure it is intended
		/// </summary>
		/// <param name="_"></param>
		public override void HandleCommand( int _ ) => ConfirmationDialogFragment.ShowFragment( CommandRouter.Manager, SynchroniseOptionSelected,
				"Do you want to synchronise the albums played state across all libraries?" );

		/// <summary>
		/// Called when the user has decided to continue with the synchronisation
		/// </summary>
		/// <param name="synchronise"></param>
		private void SynchroniseOptionSelected( bool synchronise )
		{
			if ( synchronise == true )
			{
				FilterManagementController.SynchroniseAlbumPlayedStatus();
			}
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.synch_album_played;
	}
}
