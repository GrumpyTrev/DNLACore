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
	/// Used to allow the user to select a library source to edit
	/// </summary>
	internal class SourceSelectionDialog : DialogFragment
	{
		/// <summary>
		/// Show the dialogue
		/// </summary>
		/// <param name="manager"></param>
		public static void Show( Library displayLibrary, Action<Source> sourceSelectedAction, Action newSourceAction )
		{
			// Save the parameters so that they are available after a configuration change
			libraryToDisplay = displayLibrary;
			sourceSelectedCallback = sourceSelectedAction;
			newSourceReporter = newSourceAction;

			new SourceSelectionDialog().Show( CommandRouter.Manager, "fragment_source_selection" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SourceSelectionDialog()
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
			SourceDisplayAdapter sourceAdapter = new( Context, libraryToDisplay.LibrarySources, sourceView, sourceSelectedCallback.Invoke );

			sourceView.Adapter = sourceAdapter;

			// Register interest in the Library's sources
			NotificationHandler.Register<Library>( [ nameof( Library.AddSource ), nameof( Library.DeleteSource ) ], ( sender ) =>
			{
				if ( sender == libraryToDisplay )
				{
					// Refresh the adapter
					sourceAdapter.SetData( libraryToDisplay.LibrarySources );
				}
			} );

			// Add a header to the ListView
			sourceView.AddHeaderView( LayoutInflater.FromContext( Context ).Inflate( Resource.Layout.source_header_layout, null ) );

			// Create the rest of the dialog
			return new AlertDialog.Builder( Activity )
				.SetTitle( string.Format( "{0} sources", libraryToDisplay.Name ) )
				.SetView( layout )
				// Don't set an handler for the New Source button to prevent automatic dialog closure
				.SetNeutralButton( "New Source", ( EventHandler<DialogClickEventArgs> )null )
				.SetPositiveButton( "Done", delegate { } ).Create();
		}

		/// <summary>
		/// Install a handler for the New button
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();
			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Neutral ).Click += ( _, _ ) => newSourceReporter.Invoke();
		}

		/// <summary>
		/// When the dialog is destroyed get rid of any outstanding notification registrations
		/// </summary>
		public override void OnPause()
		{
			base.OnPause();
			NotificationHandler.Deregister();
		}

		/// <summary>
		/// The delegate to call when a source has been selected for editing
		/// </summary>
		private static Action<Source> sourceSelectedCallback = null;

		/// <summary>
		/// The delegate to call when a new source request has been made
		/// </summary>
		private static Action newSourceReporter = null;

		/// <summary>
		/// The library to display
		/// </summary>
		private static Library libraryToDisplay = null;
	}
}
