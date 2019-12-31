using System.Collections.Generic;

namespace DBTest
{
	static class FilterManagementModel
	{
		/// <summary>
		/// The list of filters obtaied from the database
		/// </summary>
		public static List< Tag > Tags { get; internal set; } = null;

		/// <summary>
		/// The recently added tag
		/// </summary>
		public static Tag RecentlyAddedTag { get; internal set; } = null;

		/// <summary>
		/// The just played tag
		/// </summary>
		public static Tag JustPlayedTag { get; internal set; } = null;
	}

	/// <summary>
	/// The AppliedTag class records how a tag has been applied (typically to a set of albums)
	/// </summary>
	public class AppliedTag
	{
		/// <summary>
		/// The name of the tag
		/// </summary>
		public string TagName { get; set; } = "";

		/// <summary>
		/// How fully has this tag been applied
		/// </summary>
		public AppliedType Applied { get; set; } = AppliedType.None;

		/// <summary>
		/// The original value against which changes can be determined
		/// </summary>
		public AppliedType OriginalApplied { get; set; } = AppliedType.None;

		/// <summary>
		/// Application level
		/// </summary>
		public enum AppliedType { None, Some, All };
	}
}