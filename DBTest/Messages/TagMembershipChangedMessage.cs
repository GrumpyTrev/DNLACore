using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The TagMembershipChangedMessage class is used to notify that the membership of some tags has changed
	/// </summary>
	class TagMembershipChangedMessage: BaseMessage
	{
		/// <summary>
		/// The playlist that the songs have been added to
		/// </summary>
		public List< string > ChangedTags { get; set; } = null;
	}
}