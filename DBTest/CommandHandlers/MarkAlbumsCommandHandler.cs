using Android.Widget;
using System;
using Android.Views;
using System.Linq;
using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The MarkAlbumsCommandHandler is used to allow the user to mark the selected albums as Played or Not Played.
	/// </summary>
	internal class MarkAlbumsCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. 
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity )
		{
			// First of all convert a list of ArtistAlbums to a list of Albums
			foreach ( ArtistAlbum artistAlbum in selectedObjects.ArtistAlbums )
			{
				selectedObjects.Albums.Add( artistAlbum.Album );
			}

			// Create a Popup menu containing the 'mark played' and 'Mark not-played' options
			// Could just expand a resource here?
			PopupMenu markMenu = new( commandButton.Context, commandButton );
			markMenu.Menu.Add( 0, 0, 0, "Mark as played" );
			markMenu.Menu.Add( 0, 1, 0, "Mark as not-played" );

			// When a menu item is clicked pass albums to the appropriate controller
			markMenu.MenuItemClick += MenuItemClicked;
			markMenu.Show();
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// It is valid if ArtistAlbum or Album objects have been selected. For ArtistAlbums make sure that no extraneous Songs ave also been selected
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		protected override bool IsSelectionValidForCommand( int _ ) => ( selectedObjects.Albums.Count > 0 ) ||
			( ( selectedObjects.ArtistAlbums.Count > 0 ) && 
			  ( selectedObjects.ArtistAlbums.SelectMany( album => album.Songs ).Count() == selectedObjects.Songs.Count ) );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.mark;

		/// <summary>
		/// When a menu item is clicked pass the albums to the appropriate controller
		/// </summary>
		/// <param name="_"></param>
		/// <param name="args"></param>
		private void MenuItemClicked( object _, PopupMenu.MenuItemClickEventArgs args )
		{
			// Use the menu id to determine what has been selected
			int menuId = args.Item.ItemId;
			if ( menuId == 0 )
			{
				// Mark the selected albums as played
				foreach ( Album album in selectedObjects.Albums )
				{
					MainApp.CommandInterface.AddAlbumToTag( FilterManagementModel.JustPlayedTag, album );
				}

				commandCallback.PerformAction();
			}
			else if ( menuId == 1 )
			{
				// Mark the selected albums as not-played
				foreach ( Album album in selectedObjects.Albums )
				{
					MainApp.CommandInterface.RemoveAlbumFromTag( FilterManagementModel.JustPlayedTag, album );
				}

				commandCallback.PerformAction();
			}
		}
	}
}
