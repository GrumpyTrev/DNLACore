using System;

namespace CoreMP
{
	/// <summary>
	/// The ShuffleModeChangedMessage class is used to notify that the shuufle mode has changed
	/// </summary>
	public class ShuffleModeChangedMessage : BaseMessage
	{
		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action action ) => MessageRegistration.Register( action, typeof( ShuffleModeChangedMessage ) );
	}
}
