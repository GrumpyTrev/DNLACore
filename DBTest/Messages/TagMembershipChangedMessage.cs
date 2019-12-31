using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The TagMembershipChangedMessage class is used to notify that the membership of some tags has changed
	/// </summary>
	class TagMembershipChangedMessage: BaseMessage
	{
		/// <summary>
		/// The names of the tags whose membership has changed
		/// </summary>
		public List< string > ChangedTags { get; set; } = null;
	}
}