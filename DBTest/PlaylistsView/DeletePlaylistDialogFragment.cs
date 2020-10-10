using Android.App;
using Android.OS;
using System.Collections.Generic;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Dialogue reporting some kind of problem with the requested action
	/// </summary>
	internal class DeletePlaylistDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show an alert dialogue with the specified Title and a single OK button
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Playlist selectedPlaylist, List<PlaylistItem> songsSelected )
		{
			// Save the playlist and songs statically to survive a rotation.
			PlaylistToDelete = selectedPlaylist;
			SongsToDelete = songsSelected;

			new DeletePlaylistDialogFragment().Show( manager, "fragment_delete_playlist_tag" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public DeletePlaylistDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder(Activity )
				.SetTitle( "Do you want to delete the playlist" )
				.SetPositiveButton( "Yes", delegate {
						// Delete the single selected playlist and all of its contents
						PlaylistsController.DeletePlaylist( PlaylistToDelete );
					} )
				.SetNegativeButton( "No", delegate {
					// Just delete the songs. They will all be in the selected playlist
					PlaylistsController.DeletePlaylistItems( PlaylistToDelete, SongsToDelete );
				} )
				.Create();

		/// <summary>
		/// The playlist to delete
		/// </summary>
		private static Playlist PlaylistToDelete { get; set; } = null;

		/// <summary>
		/// The songs to delete
		/// </summary>
		private static List<PlaylistItem> SongsToDelete { get; set; } = null;
	}
}