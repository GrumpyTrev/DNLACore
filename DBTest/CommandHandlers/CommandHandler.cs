using Android.Content;
using Android.Views;

namespace DBTest
{
	internal abstract class CommandHandler
	{
		/// <summary>
		/// Called to handle a command that requires no extra parameters
		/// </summary>
		/// <param name="commandIdentity"></param>
		public abstract void HandleCommand( int commandIdentity );

		/// <summary>
		/// Called to handle a command that takes a collection of selected objects and a completion action
		/// </summary>
		/// <param name="commandIdentity"></param>
		/// <param name="selection"></param>
		/// <param name="callback"></param>
		public virtual void HandleCommand( int commandIdentity, GroupedSelection selection, CommandRouter.CommandHandlerCallback callback, View anchorView,
			Context contextForCommand )
		{
			selectedObjects = selection;
			commandCallback = callback;
			commandView = anchorView;
			commandContext = contextForCommand;
			HandleCommand( commandIdentity );
		}

		/// <summary>
		/// Bind this command to the router using its command identity
		/// </summary>
		public virtual void BindToRouter() => CommandRouter.BindHandler( CommandIdentity, this );

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		/// <param name="selectedObjects"></param>
		/// <returns></returns>
		public bool IsSelectionValidForCommand( GroupedSelection selection, int commandId )
		{
			selectedObjects = selection;
			return IsSelectionValidForCommand( commandId );
		}

		/// <summary>
		/// Is the command valid given the selected objects
		/// </summary>
		protected virtual bool IsSelectionValidForCommand( int commandId ) => false;

		/// <summary>
		/// The command identity for the command
		/// </summary>
		protected abstract int CommandIdentity { get; }

		/// <summary>
		/// The selected objects
		/// </summary>
		protected GroupedSelection selectedObjects;

		/// <summary>
		/// The CommandHandlerCallback
		/// </summary>
		protected CommandRouter.CommandHandlerCallback commandCallback = null;

		/// <summary>
		/// The menu item that generated this command
		/// </summary>
		protected View commandView = null;

		/// <summary>
		/// The Context to use for popups etc.
		/// </summary>
		protected Context commandContext = null;
	}
}
