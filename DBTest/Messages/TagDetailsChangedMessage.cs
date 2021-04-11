using System;

namespace DBTest
{
	/// <summary>
	/// The TagDetailsChangedMessage class is used to notify that the properties of a tag have been changed
	/// </summary>
	class TagDetailsChangedMessage: BaseMessage
	{
		/// <summary>
		/// The Tag that has been changed
		/// </summary>
		public Tag ChangedTag { private get; set; } = null;

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<Tag> )( ChangedTag );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<Tag> action ) => MessageRegistration.Register( action, typeof( TagDetailsChangedMessage ) );
	}
}