using System;
using Android.Support.V7.App;
using Android.Views;

namespace DBTest
{
	/// <summary>
	/// The TagCommandHandler class is responsible for routing tag menu selections to specialises handlers
	/// </summary>
	abstract class TagCommandHandler
	{
		/// <summary>
		/// Public constructor providing the context for the dialogues
		/// </summary>
		/// <param name="activityContext"></param>
		public TagCommandHandler( AppCompatActivity activityContext ) => Context = activityContext;

		/// <summary>
		/// Add a submenu to the supplied menu item containing the user tag names
		/// </summary>
		/// <param name="item"></param>
		/// <param name="menuId"></param>
		/// <returns></returns>
		public void PrepareMenu( IMenuItem item, ref int menuId )
		{
			lastId = menuId;
			firstId = lastId;

			item.SubMenu.Clear();
			FilterManagementController.GetUserTagNames().ForEach( name => item.SubMenu.Add( Menu.None, lastId++, Menu.None, name ) );

			menuId = lastId;
		}

		/// <summary>
		/// Called when a menu item has been selected.
		/// Check if it is one of the items handled by this class and process it
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool OnOptionsItemSelected( int id, string name )
		{
			bool handled = false;

			if ( ( id >= firstId ) && ( id < lastId ) )
			{
				ProcessTagCommand( name );
				handled = true;
			}

			return handled;
		}

		/// <summary>
		/// Process the tag command
		/// </summary>
		/// <param name="name"></param>
		protected abstract void ProcessTagCommand( string name );

		/// <summary>
		/// Context to use for dialogue fragment creation
		/// </summary>
		protected AppCompatActivity Context { get; private set; }

		/// <summary>
		/// The id of the first submenu item
		/// </summary>
		private int firstId;

		/// <summary>
		/// The id of the last submenu item
		/// </summary>
		private int lastId;
	}
}