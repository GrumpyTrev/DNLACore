﻿namespace DBTest
{
	/// <summary>
	/// The SelectedLibraryChangedMessage class is used to notify that the selected library has changed
	/// </summary>
	class SelectedLibraryChangedMessage: BaseMessage
	{
		/// <summary>
		/// The selected library
		/// </summary>
		public int SelectedLibrary { get; set; } = 0;
	}
}