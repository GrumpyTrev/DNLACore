using System;
using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The TagMembershipChangedMessage class is used to notify that the membership of some tags has changed.
	/// The tags can either be simple tags or group tags (Genre or Year tags)
	/// </summary>
	internal class TagMembershipChangedMessage: BaseMessage
	{
		/// <summary>
		/// The names of the tags whose membership has changed
		/// </summary>
		public List< string > ChangedTags { private get; set; } = null;

		/// <summary>
		/// Override the base Dispatch in order to pass back the contents of the message rather than the message itself
		/// </summary>
		/// <param name="callback"></param>
		public override void Dispatch( Delegate callback ) => ( callback as Action<List<string>> )( ChangedTags );

		/// <summary>
		/// Provide a static Register method in order to check the provided action at compile time
		/// </summary>
		/// <param name="action"></param>
		public static void Register( Action<List<string>> action ) => MessageRegistration.Register( action, typeof( TagMembershipChangedMessage ) );
	}
}
