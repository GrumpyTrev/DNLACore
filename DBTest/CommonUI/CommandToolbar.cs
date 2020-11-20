using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The CommandBar class encapsulates a toolbar that contains one or more ImageButtons
	/// </summary>
	public class CommandBar
	{
		public CommandBar( View parentView, int toolbarResource, HandleCommandDelegate handleDelegate )
		{
			// Save the command handling delegate
			commandDelegate = handleDelegate;

			// Create the toolbar
			Toolbar = parentView.FindViewById<Toolbar>( toolbarResource );

			if ( Toolbar != null )
			{
				// Iterate through the children of this toolbar looking for ImageButton
				for ( int index = 0; index < Toolbar.ChildCount; ++index )
				{
					if ( Toolbar.GetChildAt( index ) is AppCompatImageButton imageButton )
					{
						// Get the name of the resource form the id and use it to form the name of the image resource
						string[] packageSplit = Application.Context.Resources.GetResourceName( imageButton.Id ).Split( ':' );
						string imageName = string.Format( "{0}:drawable/{1}", packageSplit[ 0 ], packageSplit[ 1 ].Split( '/' )[ 1 ] );

						imageButton.SetImageResource( Application.Context.Resources.GetIdentifier( imageName, null, null ) );

						// Store the button id and button in an hash table to enable them to be bound
						Buttons.Add( imageButton.Id, imageButton );

						imageButton.Click += ButtonClicked;
					}
				}

				// Hide the toolbar initially
				Toolbar.Visibility = ViewStates.Gone;
			}
		}

		/// <summary>
		/// Display or hide the toolbar
		/// </summary>
		/// <param name="isVisible"></param>
		public bool Visibility
		{
			set
			{
				if ( Toolbar != null )
				{
					Toolbar.Visibility = ( value == true ) ? ViewStates.Visible : ViewStates.Gone;
				}
			}
		}

		/// <summary>
		/// Check if any of the button are currently visible
		/// </summary>
		/// <returns></returns>
		public bool AnyButtonsVisible() => Buttons.Values.Any( button => button.Visibility == ViewStates.Visible );

		/// <summary>
		/// Use the command handler associated with each button to determine if the button should be shown
		/// </summary>
		/// <param name="selectedObjects"></param>
		public void DetermineButtonsVisibility( GroupedSelection selectedObjects )
		{
			foreach ( KeyValuePair<int, AppCompatImageButton> buttonPair in Buttons )
			{
				// Is there a command handler associated with this button
				CommandHandler handler = CommandRouter.GetHandlerForCommand( buttonPair.Key );
				if ( handler != null )
				{
					buttonPair.Value.Visibility = 
						( handler.IsSelectionValidForCommand( selectedObjects, buttonPair.Key ) == true ) ? ViewStates.Visible : ViewStates.Gone;
				}
			}
		}

		/// <summary>
		/// Called when a button has been clicked.
		/// Pass on to the command handler delegate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonClicked( object sender, System.EventArgs e ) => commandDelegate( ( sender as AppCompatImageButton ).Id, sender as AppCompatImageButton );

		/// <summary>
		/// The actual toolbar
		/// </summary>
		public Toolbar Toolbar { get; private set; } = null;

		/// <summary>
		/// The delegate type for binding to the toolbar
		/// </summary>
		public delegate void BindCommandsDelegate( CommandBar commandBar );

		/// <summary>
		/// The delegate for reporting back command invocations
		/// </summary>
		/// <param name="commandId"></param>
		public delegate void HandleCommandDelegate( int commandId, AppCompatImageButton button );

		/// <summary>
		/// Collection of buttons indexed by id
		/// </summary>
		private readonly Dictionary< int, AppCompatImageButton > Buttons = new Dictionary<int, AppCompatImageButton>();

		/// <summary>
		/// The delegate to call when a command has been invoked
		/// </summary>
		private readonly HandleCommandDelegate commandDelegate = null;
	}
}