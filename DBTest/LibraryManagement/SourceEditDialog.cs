using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using CoreMP;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	/// <summary>
	/// Tag editor dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class SourceEditDialog : DialogFragment, Android.Widget.RadioButton.IOnCheckedChangeListener
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void Show( Source sourceToEdit, Action<Source, Source, Action> changedAction, Action<Source, Action> deletedAction )
		{
			// Save the parameters so that they are available after a configuration change
			SourceEditDialog.sourceToEdit = sourceToEdit;
			changedCallback = changedAction;
			deletedCallback = deletedAction;

			new SourceEditDialog().Show( CommandRouter.Manager, "fragment_edit_source" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SourceEditDialog()
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
			portNo = editView.FindViewById<EditText>( Resource.Id.sourcePort );
			ipAddress = editView.FindViewById<EditText>( Resource.Id.sourceIPAddress );
			localButton = editView.FindViewById<RadioButton>( Resource.Id.sourceLocal );
			ftpButton = editView.FindViewById<RadioButton>( Resource.Id.sourceFTP );
			upnpButton = editView.FindViewById<RadioButton>( Resource.Id.sourceUPnP );

			// Display the source. Get the values from the Source unless the Bundle is available
			if ( savedInstanceState == null )
			{
				sourceName.Text = sourceToEdit.Name;
				folderName.Text = sourceToEdit.FolderName;
				portNo.Text = sourceToEdit.PortNo.ToString();
				localButton.Checked = sourceToEdit.AccessMethod == Source.AccessType.Local;
				ftpButton.Checked = sourceToEdit.AccessMethod == Source.AccessType.FTP;
				upnpButton.Checked = sourceToEdit.AccessMethod == Source.AccessType.UPnP;
				ipAddress.Text = sourceToEdit.IPAddress;

				// Install handlers for the access type radio buttons
				ftpButton.SetOnCheckedChangeListener( this );
				localButton.SetOnCheckedChangeListener( this );
				upnpButton.SetOnCheckedChangeListener( this );

				if ( localButton.Checked == true )
				{
					OnCheckedChanged( localButton, true );
				}
			}

			// Create the AlertDialog with no Save handler (and no dismiss on Save)
			return new AlertDialog.Builder( Activity )
				.SetTitle( "Edit source" )
				.SetView( editView )
				// Install an empty handler for 'Save' and 'Delete' to prevent automatic dialog cancelling
				.SetPositiveButton( "Save", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate { } )
				.SetNeutralButton( "Delete", ( EventHandler<DialogClickEventArgs> )null )
				.Create();
		}

		/// <summary>
		/// Install a handlers for the Save and Delete buttons
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Click += ( _, _ ) =>
			{

#pragma warning disable CS0618 // Type or member is obsolete. Allowed as this Source is only used to hold edited values and is not itseld added to the model
				Source newSource = new()
				{
					Name = sourceName.Text,
					FolderName = folderName.Text,
					PortNo = int.Parse( portNo.Text ),
					IPAddress = ipAddress.Text,
					AccessMethod = localButton.Checked ? Source.AccessType.Local : ftpButton.Checked ? Source.AccessType.FTP : Source.AccessType.UPnP
				};
#pragma warning restore CS0618 // Type or member is obsolete

				// Report back the old and new source records
				changedCallback.Invoke( sourceToEdit, newSource, Dialog.Dismiss );
			};

			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Neutral ).Click += ( _, _ ) => deletedCallback.Invoke( sourceToEdit, Dialog.Dismiss );
		}

		/// <summary>
		/// Called when one of the Access Type buttons has been clicked
		/// If the source is Local then set the ipAddress to the IP address of this device, and the port to the 
		/// fixed Http port. Grey out the associated text boxes
		/// </summary>
		/// <param name="buttonView"></param>
		/// <param name="isChecked"></param>
		/// <exception cref="NotImplementedException"></exception>
		public void OnCheckedChanged( CompoundButton buttonView, bool isChecked )
		{
			if ( isChecked == true )
			{
				if ( buttonView == localButton )
				{
					portNo.Text = CoreMPApp.HttpPort.ToString();
					portNo.Enabled = false;
					ipAddress.Text = sourceToEdit.IPAddress;
					ipAddress.Enabled = false;
				}
				else
				{
					portNo.Enabled = true;
					ipAddress.Enabled = true;
				}
			}
		}

		/// <summary>
		/// The source to edit
		/// </summary>
		private static Source sourceToEdit = null;

		/// <summary>
		/// Delegate to report source changes
		/// </summary>
		private static Action<Source, Source, Action> changedCallback = null;

		/// <summary>
		/// Delegate to report source deletions
		/// </summary>
		private static Action<Source, Action> deletedCallback = null;

		/// <summary>
		/// The dialogue fields updated by the user. These must be available outside of the OnCreateDialog method
		/// </summary>
		private EditText sourceName = null;
		private EditText folderName = null;
		private RadioButton localButton = null;
		private EditText portNo = null;
		private RadioButton ftpButton = null;
		private RadioButton upnpButton = null;
		private EditText ipAddress = null;
	}
}
