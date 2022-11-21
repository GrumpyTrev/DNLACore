using System.Linq;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The SynchAlbumStatusCommandHandler class is used to synchronise the 'played' status of albums across all libraries.
	/// </summary>
	internal class SynchAlbumStatusCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// Confirm this command to make sure it is intended
		/// </summary>
		/// <param name="_"></param>
		public override void HandleCommand( int _ ) => 
			ConfirmationDialog.Show( "Do you want to synchronise the albums played state across all libraries?",
				() => MainApp.CommandInterface.SynchroniseAlbumPlayedStatus() );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.synch_album_played;
	}
}
