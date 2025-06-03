using Android.Views;
using System.Collections.Generic;
using System.Linq;

namespace DBTest
{
	/// <summary>
	/// The MenuCommandHandler class links menu items in a menu with CommandRouter commands
	/// </summary>
	public class MenuCommandHandler
	{
		public MenuCommandHandler( IMenu parentMenu )
		{
			// Iterate through the items associated with the menu
			for ( int index = 0; index < parentMenu.Size(); ++index )
			{
				IMenuItem menuItem = parentMenu.GetItem( index );

				// Has this item got a handler associated with it
				CommandHandler handler = CommandRouter.GetHandlerForCommand( menuItem.ItemId );
				if ( handler != null )
				{
					// Add this menu item to the collection
					MenuItems.Add( menuItem.ItemId, menuItem );
				}
			}
		}

		/// <summary>
		/// Check if any of the menus are currently visible
		/// </summary>
		/// <returns></returns>
		public bool AnyMenuItemsVisible() => MenuItems.Values.Any( menu => menu.IsVisible == true );

		/// <summary>
		/// Use the command handler associated with each menu item to determine if the menu item should be shown
		/// </summary>
		/// <param name="selectedObjects"></param>
		public void DetermineMenuItemsVisibility( GroupedSelection selectedObjects )
		{
			foreach ( KeyValuePair<int, IMenuItem> menuPair in MenuItems )
			{
				// Is there a command handler associated with this menu item
				CommandHandler handler = CommandRouter.GetHandlerForCommand( menuPair.Key );
				if ( handler != null )
				{
					menuPair.Value.SetVisible( handler.IsSelectionValidForCommand( selectedObjects, menuPair.Key ) == true );
				}
			}
		}

		/// <summary>
		/// Collection of menu items indexed by id
		/// </summary>
		private readonly Dictionary< int, IMenuItem > MenuItems = new();
	}
}
