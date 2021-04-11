using System;

namespace DBTest
{
	/// <summary>
	/// The StorageDataAvailableMessage class is used to notify that the album data associated with the current directory is now available
	/// </summary>
	class StorageDataAvailableMessage : BaseMessage
	{
		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action action ) => MessageRegistration.Register( action, typeof( StorageDataAvailableMessage ) );
	}
}