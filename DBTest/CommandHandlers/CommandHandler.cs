using Android.Support.V7.Widget;
using System.Collections.Generic;

namespace DBTest
{
	abstract class CommandHandler
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
		public virtual void HandleCommand( int commandIdentity, GroupedSelection selection, CommandRouter.CommandHandlerCallback callback, AppCompatImageButton button )
		{
			selectedObjects = selection;
			commandCallback = callback;
			commandButton = button;
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
		/// The button on the toolbar that generated this command
		/// </summary>
		protected AppCompatImageButton commandButton = null;
	}
}