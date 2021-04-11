using System;

namespace DBTest
{
	/// <summary>
	/// The TagDeletedMessage class is used to notify that a Tag has been deleted
	/// </summary>
	class TagDeletedMessage: BaseMessage
	{
		/// <summary>
		/// The Tag that has been deleted
		/// </summary>
		public Tag DeletedTag { private get; set; } = null;

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<Tag> )( DeletedTag );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<Tag> action ) => MessageRegistration.Register( action, typeof( TagDeletedMessage ) );
	}
}