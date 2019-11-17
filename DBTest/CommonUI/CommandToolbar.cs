using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The CommandBar class encapsulates a toolbar that contains one or more ImageButtons
	/// </summary>
	public class CommandBar
	{
		public CommandBar( View parentView, int toolbarResource, BindCommandsDelegate bindDelegate, HandleCommandDelegate handleDelegate )
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
					AppCompatImageButton imageButton = Toolbar.GetChildAt( index ) as AppCompatImageButton;
					if ( imageButton != null )
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

				// Allow the command bar to be bound to
				bindDelegate( this );
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
		/// Create a CommandBinder for the specified button
		/// </summary>
		/// <param name="buttonId"></param>
		/// <returns></returns>
		public CommandBinder BindCommand( int buttonId )
		{
			CommandBinder boundCommand = null;

			if ( Buttons.ContainsKey( buttonId ) == true )
			{
				AppCompatImageButton imageButton = Buttons[ buttonId ];

				boundCommand = new CommandBinder( imageButton );
			}

			return boundCommand;
		}

		/// <summary>
		/// Called when a button has been clicked.
		/// Pass on to the command handler delegate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButtonClicked( object sender, System.EventArgs e )
		{
			commandDelegate( ( sender as AppCompatImageButton ).Id );
		}

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
		public delegate void HandleCommandDelegate( int commandId );

		/// <summary>
		/// Collection of buttons indexed by id
		/// </summary>
		private Dictionary< int, AppCompatImageButton > Buttons = new Dictionary<int, AppCompatImageButton>();

		/// <summary>
		/// The delegate to call when a command has been invoked
		/// </summary>
		private readonly HandleCommandDelegate commandDelegate = null;
	}

	/// <summary>
	/// The CommandBinder class is used to bind to an ImageButton so that it's visibility can be changed
	/// </summary>
	public class CommandBinder
	{
		public CommandBinder( AppCompatImageButton button )
		{
			BoundButton = button;
		}
		
		/// <summary>
		/// Show or hide the button
		/// </summary>
		public bool Visible
		{
			get
			{
				return ( BoundButton.Visibility == ViewStates.Visible );
			}

			set
			{
				BoundButton.Visibility = ( value == true ) ? ViewStates.Visible : ViewStates.Gone;
			}
		}

		/// <summary>
		/// The button bound to this command
		/// </summary>
		public AppCompatImageButton BoundButton { get; private set; }
	}
}