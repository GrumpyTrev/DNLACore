using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;

namespace DBTest
{
	/// <summary>
	/// The FilterSelection class controls the selection of filters to be applied
	/// </summary>
	class FilterSelection
	{
		/// <summary>
		/// PlaybackConnection constructor
		/// Save the supplied context for binding later on
		/// </summary>
		/// <param name="bindContext"></param>
		public FilterSelection( Context alertContext, FilterSelectionDelegate selectionCallback )
		{
			contextForAlert = alertContext;
			selectionDelegate = selectionCallback;
		}

		/// <summary>
		/// Allow the user to pick one of the available Tags as a filter, or to turn off filtering
		/// </summary>
		/// <param name="currentFilter"></param>
		/// <returns></returns>
		public void SelectFilter( Tag currentFilter )
		{
			// Form a list of choices including None
			List<string> tagNames = FilterManagementModel.Tags.Select( tag => tag.Name ).ToList();
			tagNames.Insert( 0, "None" );

			// Which one of these is currently selected
			int tagIndex = ( currentFilter != null ) ? tagNames.IndexOf( currentFilter.Name ) : 0;

			AlertDialog alert = new AlertDialog.Builder( contextForAlert )
				.SetTitle( "Apply filter" )
				.SetSingleChoiceItems( tagNames.ToArray(), tagIndex,
					new EventHandler<DialogClickEventArgs>( delegate ( object sender, DialogClickEventArgs e )
					{
						tagIndex = e.Which;
					} ) )
				.SetPositiveButton( "OK", delegate 
				{
					// Convert the index back to a Tag and report back
					selectionDelegate( ( tagIndex == 0 ) ? null : FilterManagementModel.Tags[ tagIndex - 1 ] );
				} )
				.SetNegativeButton( "Cancel", delegate { } )
				.Show();
		}

		/// <summary>
		/// Delegate used to report back the result of the filter selection
		/// </summary>
		/// <param name="newFilter"></param>
		public delegate void FilterSelectionDelegate( Tag newFilter );

		/// <summary>
		/// Context to use for building the selection dialogue
		/// </summary>
		private readonly Context contextForAlert = null;

		/// <summary>
		/// The delegate to call when a filter has been selected
		/// </summary>
		private readonly FilterSelectionDelegate selectionDelegate = null;
	}
}