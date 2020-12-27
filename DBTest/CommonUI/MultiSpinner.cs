using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Widget;

namespace DBTest
{
	public class MultiSpinner : Spinner, IDialogInterfaceOnMultiChoiceClickListener, IDialogInterfaceOnShowListener
	{
		/// <summary>
		/// Constructors required by framework
		/// </summary>
		/// <param name="context"></param>
		public MultiSpinner( Context context ) : base( context ) => viewContext = context;

		public MultiSpinner( Context context, IAttributeSet arg1 ) : base( context, arg1 ) => viewContext = context;

		public MultiSpinner( Context context, IAttributeSet arg1, int arg2 ) : base( context, arg1, arg2 ) => viewContext = context;

		/// <summary>
		/// Called when the spinner control is clicked
		/// Display a dialogue containing the items
		/// </summary>
		/// <returns></returns>
		public override bool PerformClick()
		{
			AlertDialog dialog = new AlertDialog.Builder( viewContext )
				.SetMultiChoiceItems( items.ToArray(), null, this )
				.SetNeutralButton( "Set", ( EventHandler<DialogClickEventArgs> )null )
				.SetPositiveButton( "OK", delegate { ProcessSelections(); } )
				.Create();

			dialog.SetOnShowListener( this );
			dialog.Show();

			return true;
		}

		/// <summary>
		/// Called when the AlertDialog is first displayed
		/// Initialise the selection states of the list vire items and install an handler for the setStatesButton
		/// </summary>
		/// <param name="dialog"></param>
		public void OnShow( IDialogInterface dialog )
		{
			AlertDialog alertDialog = ( AlertDialog )dialog;
			setStatesButton = alertDialog.GetButton( ( int )DialogButtonType.Neutral );
			okButton = alertDialog.GetButton( ( int )DialogButtonType.Positive );

			ListView listView = alertDialog.ListView;

			// Initialise the selection state for all the listview items according to the SelectionRecord
			for ( int selectionIndex = 0; selectionIndex < SelectionRecord.Length; ++selectionIndex )
			{
				listView.SetItemChecked( selectionIndex, SelectionRecord[ selectionIndex ] );
			}

			// Update the state of the states and OK buttons
			UpdateButtonStates();

			// Install a handler for the cancel button so that a cancel can be scheduled rather than acted upon immediately
			setStatesButton.Click += ( sender, args ) => 
			{
				bool allSelected = SelectionRecord.All( sel => sel );

				for ( int selectionIndex = 0; selectionIndex < items.Count; ++selectionIndex )
				{
					SelectionRecord[ selectionIndex ] = !allSelected;
					listView.SetItemChecked( selectionIndex, !allSelected );
				}

				UpdateButtonStates();
			};
		}

		/// <summary>
		/// Called when an item's checkbox is selected. Toggle it's associated sleected flag
		/// </summary>
		/// <param name="dialog"></param>
		/// <param name="which"></param>
		/// <param name="isChecked"></param>
		public void OnClick( IDialogInterface dialog, int which, bool isChecked )
		{
			SelectionRecord[ which ] = isChecked;
			UpdateButtonStates();
		}

		/// <summary>
		/// Called to provide the data for this control
		/// </summary>
		/// <param name="items"></param>
		/// <param name="allText"></param>
		public void SetItems( List<String> data, bool[] selected, string allText )
		{
			// Save the data and the string to display when all items are selected
			items = data;
			allSelectedText = allText;

			// Keep a record of the selected items
			SelectionRecord = selected;

			// The spinner actually only contains a single value. When it is clicked on the dialogue is shown over it.
			Adapter = new ArrayAdapter<string>( viewContext, Resource.Layout.select_dialog_item_material, new string[] { ClosedSpinnerText() } );
		}

		/// <summary>
		/// Called when the user has pressed OK.
		/// Form a string from all the selected items and display that in the spinner
		/// </summary>
		private void ProcessSelections() => 
			Adapter = new ArrayAdapter<String>( viewContext, Resource.Layout.select_dialog_item_material, new string[] { ClosedSpinnerText() } );

		/// <summary>
		/// Determine the text to display when the spinner is closed
		/// </summary>
		/// <returns></returns>
		private string ClosedSpinnerText()
		{
			// Assume all items selected
			string spinnerText = allSelectedText;

			// Check how many items are selected
			if ( SelectionRecord.All( sel => sel ) == false )
			{
				// Not all items are selected. Form a string from all the selected items
				StringBuilder spinnerBuffer = new StringBuilder();
				for ( int itemIndex = 0; itemIndex < items.Count; itemIndex++ )
				{
					if ( SelectionRecord[ itemIndex ] == true )
					{
						spinnerBuffer.Append( items[ itemIndex ] );
						spinnerBuffer.Append( ", " );
					}
				}

				spinnerText = spinnerBuffer.ToString();
				spinnerText = spinnerText.Substring( 0, spinnerText.Length - 2 );
			}

			return spinnerText;
		}

		/// <summary>
		/// Update the text displayed on the setStatesButton
		/// </summary>
		private void UpdateButtonStates()
		{
			setStatesButton.Text = SelectionRecord.All( sel => sel ) ? "Clear" : "Set";
			okButton.Enabled = SelectionRecord.Any( sel => sel );
		}

		/// <summary>
		/// The Context to use to create the dialogue and adapters
		/// </summary>
		private readonly Context viewContext = null;

		/// <summary>
		/// The set of strings to display
		/// </summary>
		private List<string> items;

		/// <summary>
		/// Which items have been selected
		/// </summary>
		public bool[] SelectionRecord { get; private set; } = null;

		/// <summary>
		/// The button used to either clear or set all the item selections
		/// </summary>
		private Button setStatesButton = null;

		/// <summary>
		/// The Ok button that needs to be disabled when all slections have been cleared
		/// </summary>
		private Button okButton = null;

		/// <summary>
		/// Text to display if all items have been selected
		/// </summary>
		private String allSelectedText;
	}
}