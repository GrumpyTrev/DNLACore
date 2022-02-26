namespace DBTest
{
	/// <summary>
	/// The AutoplayOptionsCommandHandler class is used to allow the user to update the current Autoplay options and use the updated options to 
	/// start and Autoplay operaton.
	/// The AutoplayOptionsDialogFragment class is used to update the optins and select the Autoplay play/queue option.
	internal class AutoplayOptionsCommandHandler : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Show the AutoplayOptionsDialogFragment dialogue and pass on any option changes to the controller
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => AutoplayOptionsDialogFragment.ShowFragment( CommandRouter.Manager, OptionsSelected );

		/// <summary>
		/// Called when the Autoplay options have been updated and action specified
		/// </summary>
		/// <param name="selectedLibrary"></param>
		private void OptionsSelected( Autoplay newAutoplay, bool playNow )
		{
			// If any Autoplay options have changed then update them
			AutoplayModel.CurrentAutoplay.UpdateOptions( newAutoplay );

			// Pass the request on to the StartAutoPlaylistCommandHandler
			CommandRouter.HandleCommand( playNow ? Resource.Id.auto_play : Resource.Id.auto_queue, selectedObjects.SelectedObjects, commandCallback, commandButton );
		}

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.auto_options;
	}
}
