using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Tag editor dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class SourceEditDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Source sourceToEdit, SourceChanged callback )
		{
			// Save the parameters so that they are available after a configuration change
			SourceEditDialogFragment.sourceToEdit = sourceToEdit;
			reporter = callback;

			new SourceEditDialogFragment().Show( manager, "fragment_edit_source" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SourceEditDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Create the custom view and get references to the editable fields
			View editView = LayoutInflater.From( Context ).Inflate( Resource.Layout.source_details_dialogue_layout, null );
			sourceName = editView.FindViewById<EditText>( Resource.Id.sourceName );
			folderName = editView.FindViewById<EditText>( Resource.Id.sourceFolder );
			localButton = editView.FindViewById<RadioButton>( Resource.Id.sourceLocal );
			portNo = editView.FindViewById<EditText>( Resource.Id.sourcePort );
			remoteButton = editView.FindViewById<RadioButton>( Resource.Id.sourceRemote );
			ipAddress = editView.FindViewById<EditText>( Resource.Id.sourceIPAddress );

			// Display the source. Get the values from the Source unless the Bundle is available
			if ( savedInstanceState == null )
			{
				sourceName.Text = sourceToEdit.Name;
				folderName.Text = sourceToEdit.FolderName;
				localButton.Checked = sourceToEdit.AccessType == "Local";
				portNo.Text = sourceToEdit.PortNo.ToString();
				remoteButton.Checked = sourceToEdit.AccessType == "Remote";
				ipAddress.Text = sourceToEdit.IPAddress;
			}

			// Create the AlertDialog with no Save handler (and no dismiss on Save)
			return new AlertDialog.Builder( Activity )
				.SetTitle( "Edit source" )
				.SetView( editView )
				.SetPositiveButton( "Save", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();
		}

		/// <summary>
		/// Install a handler for the Save button
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Click += async ( sender, args ) => {

				Source newSource = new Source()
				{
					Name = sourceName.Text,
					FolderName = folderName.Text,
					PortNo = int.Parse( portNo.Text ),
					IPAddress = ipAddress.Text,
					AccessType = ( localButton.Checked == true ) ? "Local" : "Remote"
				};

				// Report back the old and new source records
				reporter.Invoke( sourceToEdit, newSource, this );
			};
		}

		/// <summary>
		/// The source to edit
		/// </summary>
		private static Source sourceToEdit = null;

		/// <summary>
		/// The delegate used to report back source changes
		/// </summary>
		public delegate void SourceChanged( Source originalSource, Source newSource, SourceEditDialogFragment sourceEditDialog );

		/// <summary>
		/// Delegate to report source changes
		/// </summary>
		private static SourceChanged reporter = null;

		/// <summary>
		/// The dialogue fields updated by the user. These must be available outside of the OnCreateDialog method
		/// </summary>
		private EditText sourceName = null;
		private EditText folderName = null;
		private RadioButton localButton = null;
		private EditText portNo = null;
		private RadioButton remoteButton = null;
		private EditText ipAddress = null;

	}
}