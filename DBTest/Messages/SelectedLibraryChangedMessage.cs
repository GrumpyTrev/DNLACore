using System;

namespace DBTest
{
	/// <summary>
	/// The SelectedLibraryChangedMessage class is used to notify that the selected library has changed
	/// </summary>
	class SelectedLibraryChangedMessage: BaseMessage
	{
		/// <summary>
		/// The selected library
		/// </summary>
		public int SelectedLibrary { private get; set; }

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<int> )( SelectedLibrary );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<int> action ) => MessageRegistration.Register( action, typeof( SelectedLibraryChangedMessage ) );
	}
}