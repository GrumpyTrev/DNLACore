using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The TagMembershipChangedMessage class is used to notify that the membership of some tags has changed.
	/// The tags can either be simple tags or group tags (Genre or Year tags)
	/// </summary>
	class TagMembershipChangedMessage: BaseMessage
	{
		/// <summary>
		/// The names of the tags whose membership has changed
		/// </summary>
		public List< string > ChangedTags { get; set; } = null;
	}
}