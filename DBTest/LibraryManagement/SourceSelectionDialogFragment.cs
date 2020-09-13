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
	internal class SourceSelectionDialogFragment : DialogFragment, SourceDisplayAdapter.IReporter,
		SourceEditDialogFragment.IReporter
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, Library libraryToDisplay )
		{
			// Save the library whose soures are being displayed
			LibraryToDisplay = libraryToDisplay;

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
			sourceAdapter = new SourceDisplayAdapter( Context, Sources.GetSourcesForLibrary( LibraryToDisplay.Id ), sourceView, this );
			sourceView.Adapter = sourceAdapter;

			// Add a header to the ListView
			sourceView.AddHeaderView( LayoutInflater.FromContext( Context ).Inflate( Resource.Layout.source_header_layout, null ) );

			// Create the rest of the dialog
			AlertDialog.Builder builder = new AlertDialog.Builder( Activity )
				.SetTitle( string.Format( "Library {0}", LibraryToDisplay.Name ) )
				.SetView( layout )
				.SetPositiveButton( "Done", delegate { } );

			return builder.Create();
		}

		/// <summary>
		/// Called whn an item has been selected to edit.
		/// Display the SoureEditDialogFragment
		/// </summary>
		/// <param name="selectedSource"></param>
		public void OnSourceSelected( Source selectedSource )
		{
			SourceEditDialogFragment.ShowFragment( Activity.SupportFragmentManager, selectedSource, this );
		}

		/// <summary>
		/// Called when a source item has been changed. Display the changed data.
		/// </summary>
		public void OnSourceChanged()
		{
			sourceAdapter.NotifyDataSetChanged();
		}

		/// <summary>
		/// The library to display
		/// </summary>
		private static Library LibraryToDisplay { get; set; } = null;

		/// <summary>
		/// The Adapter showing the sources
		/// </summary>
		private SourceDisplayAdapter sourceAdapter = null;
	}
}