using Android.App;
using Android.OS;
using System.Collections.Generic;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Dialogue checking that an existing playlist should be overwritten during a duplication operation
	/// </summary>
	internal class DuplicatePlaylistDialogFragment : DialogFragment
	{
		/// <summary>
		/// Save the playlist and display the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Playlist selectedPlaylist )
		{
			// Save the playlist statically to survive a rotation.
			playlistToDuplicate = selectedPlaylist;

			new DuplicatePlaylistDialogFragment().Show( manager, "fragment_duplicate_playlist_tag" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public DuplicatePlaylistDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder(Activity )
				.SetTitle( "The playlist already exists in other libraries. Are you sure you want to duplicate it?" )
				.SetPositiveButton( "Yes", delegate {
					// Duplicate the playlist in the other libraries
					PlaylistsController.DuplicatePlaylistAsync( playlistToDuplicate );
				} )
				.SetNegativeButton( "No", delegate { } )
				.Create();

		/// <summary>
		/// The playlist to duplicate
		/// </summary>
		private static Playlist playlistToDuplicate = null;
	}
}