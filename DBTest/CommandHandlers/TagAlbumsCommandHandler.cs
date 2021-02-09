using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The TagAlbumsCommandHandler class is used to allow the user to tag the selected albums
	/// </summary>
	class TagAlbumsCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Allow the user to specify which tags to apply
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// If any ArtistAlbums are selected then form a list of Albums from them
			if ( selectedObjects.ArtistAlbums.Count > 0 )
			{
				TagApplicationDialogFragment.ShowFragment( CommandRouter.Manager, selectedObjects.ArtistAlbums.Select( aa => aa.Album ), TagsSelected );
			}
			else
			{
				TagApplicationDialogFragment.ShowFragment( CommandRouter.Manager, selectedObjects.Albums, TagsSelected );
			}
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.Albums.Count > 0 ) || ( selectedObjects.ArtistAlbums.Count > 0 );

		/// <summary>
		/// Called when the user has selected the set of tags to apply
		/// Use the FilterManagementController to apply them
		/// </summary>
		/// <param name="appliedTags"></param>
		private void TagsSelected( List<AppliedTag> appliedTags, IEnumerable<Album> selectedAlbums )
		{
			// Apply the changes.
			FilterManagementController.ApplyTagsAsync( selectedAlbums, appliedTags );

			commandCallback.PerformAction();
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.tag;
	}
}