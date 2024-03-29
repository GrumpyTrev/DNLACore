﻿using CoreMP;

namespace DBTest
{
	/// <summary>
	/// The RepeatModeCommandHander class is used to process a request to toggle the Repeat play mode
	/// </summary>
	internal class RepeatModeCommandHander : CommandHandler
	{
		/// <summary>
		/// Called to handle the command. Pass on to the PlaybackModeController
		/// </summary>
		/// <param name="commandIdentity"></param>
		public override void HandleCommand( int commandIdentity ) => CoreMPApp.Instance.CommandInterface.SetRepeat( !PlaybackModeModel.RepeatOn );

		/// <summary>
		/// The command identity associated with this handler
		/// </summary>
		protected override int CommandIdentity { get; } = Resource.Id.repeat_on_off;
	}
}
