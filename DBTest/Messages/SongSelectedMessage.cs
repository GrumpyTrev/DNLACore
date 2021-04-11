using System;

namespace DBTest
{
	/// <summary>
	/// The SongSelectedMessage class is used to notify that a song has been selected
	/// </summary>
	class SongSelectedMessage: BaseMessage
	{
		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action action ) => MessageRegistration.Register( action, typeof( SongSelectedMessage ) );
	}
}