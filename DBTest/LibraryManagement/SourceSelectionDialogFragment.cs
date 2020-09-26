using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Used to allow the user to select a library source to edit
	/// </summary>
	internal class SourceSelectionDialogFragment : DialogFragment, SourceDisplayAdapter.IReporter
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Library displayLibrary, SourceSelected callback, BindDialog bindCallback )
		{
			// Save the parameters so that they are available after a configuration change
			libraryToDisplay = displayLibrary;
			reporter = callback;
			binder = bindCallback;

			new SourceSelectionDialogFragment().Show( manager, "fragment_source_selection" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SourceSelectionDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Create the view here so that we can access the listview
			View layout = LayoutInflater.FromContext( Context ).Inflate( Resource.Layout.source_display_dialogue_layout, null );

			// Create an adapter for the list view to display the main source details
			// Keep a reference to the adapter so that we can refresh the data if a source is changed
			ListView sourceView = layout.FindViewById<ListView>( Resource.Id.sourceList );
			sourceAdapter = new SourceDisplayAdapter( Context, Sources.GetSourcesForLibrary( libraryToDisplay.Id ), sourceView, this );
			sourceView.Adapter = sourceAdapter;

			// Add a header to the ListView
			sourceView.AddHeaderView( LayoutInflater.FromContext( Context ).Inflate( Resource.Layout.source_header_layout, null ) );

			// Create the rest of the dialog
			return new AlertDialog.Builder( Activity )
				.SetTitle( string.Format( "Library {0}", libraryToDisplay.Name ) )
				.SetView( layout )
				.SetPositiveButton( "Done", delegate { } ).Create();
		}

		/// <summary>
		/// Bind this dialogue to its command handler.
		/// The command handler will then update the dialogue's state
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();
			binder.Invoke( this );
		}

		/// <summary>
		/// Unbind this dialogue so that it can be garbage collected if required
		/// </summary>
		public override void OnPause()
		{
			base.OnPause();
			binder.Invoke( null );
		}

		/// <summary>
		/// Called when a source has been selected
		/// Report the selection back to the command handler
		/// </summary>
		/// <param name="selectedSource"></param>
		public void OnSourceSelected( Source selectedSource ) => reporter?.Invoke( selectedSource );

		/// <summary>
		/// Called by the handler when a source item has been changed. Display the changed data.
		/// </summary>
		public void OnSourceChanged() => sourceAdapter.NotifyDataSetChanged();

		/// <summary>
		/// Delegate type used to report back the selected source
		/// </summary>
		public delegate void SourceSelected( Source selectedSource );

		/// <summary>
		/// The delegate to call when a source has been selected for editing
		/// </summary>
		private static SourceSelected reporter = null;

		/// <summary>
		/// Delegate type used to report back the SourceSelectionDialogFragment object
		/// </summary>
		public delegate void BindDialog( SourceSelectionDialogFragment dialogue );

		/// <summary>
		/// The delegate used to report back the SourceSelectionDialogFragment object
		/// </summary>
		private static BindDialog binder = null;

		/// <summary>
		/// The library to display
		/// </summary>
		private static Library libraryToDisplay = null;

		/// <summary>
		/// The Adapter showing the sources
		/// </summary>
		private SourceDisplayAdapter sourceAdapter = null;
	}
}