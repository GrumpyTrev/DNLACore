using Android.Content;
using Android.Support.V4.App;
using Android.Views;
using System;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The CommandRouter class is used to route commands issued by the user to specific command handlers
	/// </summary>
	internal static class CommandRouter
	{
		/// <summary>
		/// Bind new instances of the command handlers to the router using their command identities
		/// </summary>
		public static void BindHandlers()
		{
			new ScanLibraryCommandHandler().BindToRouter();
			new SelectLibraryCommandHandler().BindToRouter();
			new ClearLibraryCommandHandler().BindToRouter();
			new EditLibraryCommandHandler().BindToRouter();
			new SelectDeviceCommandHandler().BindToRouter();
			new AddSongsToNowPlayingListCommandHandler().BindToRouter();
			new AddSongsToPlaylistCommandHandler().BindToRouter();
			new MoveItemsCommandHandler().BindToRouter();
			new DeletePlaylistItemsCommandHandler().BindToRouter();
			new DuplicatePlaylistCommandHandler().BindToRouter();
			new RenamePlaylistCommandHandler().BindToRouter();
			new MarkAlbumsCommandHandler().BindToRouter();
			new NewLibraryCommandHandler().BindToRouter();
			new SynchAlbumStatusCommandHandler().BindToRouter();
			new DeleteLibraryCommandHandler().BindToRouter();
		}

		/// <summary>
		/// Called by a command handler to bind to the router
		/// </summary>
		/// <param name="commandIdentity"></param>
		/// <param name="commandToBind"></param>
		public static void BindHandler( int commandIdentity, CommandHandler commandToBind ) => router[ commandIdentity ] = commandToBind;

		/// <summary>
		/// Called by the user interface to handle a command
		/// </summary>
		/// <param name="commandIdentity"></param>
		/// <returns></returns>
		public static bool HandleCommand( int commandIdentity )
		{
			bool commandHandled = false;

			CommandHandler handler = router.GetValueOrDefault( commandIdentity );
			if ( handler != null )
			{
				commandHandled = true;
				handler.HandleCommand( commandIdentity );
			}

			return commandHandled;
		}

		/// <summary>
		/// Called by the user interface to handle a command requiring a collection of selected objects
		/// </summary>
		/// <param name="commandIdentity"></param>
		/// <returns></returns>
		public static bool HandleCommand( int commandIdentity, IEnumerable<object> selectedObjects, CommandRouter.CommandHandlerCallback callback,
			View anchorView, Context contextForCommand )
		{
			bool commandHandled = false;

			CommandHandler handler = router.GetValueOrDefault( commandIdentity );
			if ( handler != null )
			{
				commandHandled = true;
				handler.HandleCommand( commandIdentity, new GroupedSelection( selectedObjects ), callback, anchorView, contextForCommand );
			}

			return commandHandled;
		}

		/// <summary>
		/// The FragmentManager that can be used to dialogue fragments
		/// </summary>
		public static FragmentManager Manager { get; set; } = null;

		/// <summary>
		/// Get the handler associated with a command identity
		/// </summary>
		/// <param name="commandId"></param>
		/// <returns></returns>
		public static CommandHandler GetHandlerForCommand( int commandId ) => router.GetValueOrDefault( commandId );

		/// <summary>
		/// Command identity to handler lookup
		/// </summary>
		private static readonly Dictionary<int, CommandHandler> router = [];

		/// <summary>
		/// The CommandHandlerCallback class contains an Action to be performed once the command has been handled
		/// </summary>
		public class CommandHandlerCallback
		{
			/// <summary>
			/// Perfrom the supplied action. If the action is not availabel then set the callbackFailed flag 
			/// </summary>
			public void PerformAction()
			{
				if ( Callback != null )
				{
					callbackFailed = false;
					Callback.Invoke();
				}
				else
				{
					callbackFailed = true;
				}
			}

			/// <summary>
			/// The Action to perform
			/// </summary>
			public Action Callback
			{
				get => action;

				set
				{
					// If this is being set then check if a failed callback needs processing now
					if ( ( value != null ) && ( action == null ) )
					{
						action = value;

						if ( callbackFailed == true )
						{
							PerformAction();
						}
					}
					else
					{
						action = value;
					}
				}
			}

			/// <summary>
			/// The Action to perform
			/// </summary>
			private Action action = null;

			/// <summary>
			/// Set when a callback has been requested and no callback was available
			/// </summary>
			private bool callbackFailed = false;
		}
	}
}
