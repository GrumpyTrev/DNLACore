using System.Collections.Generic;

namespace DBTest
{
	static class FilterManagementModel
	{
		/// <summary>
		/// The list of filters
		/// </summary>
		public static List< Tag > Tags { get; internal set; } = null;

		/// <summary>
		/// The libraries
		/// </summary>
		public static List<Library> Libraries { get; internal set; } = null;

		/// <summary>
		/// The ste of all tag groups available for the user to choose from
		/// </summary>
		public static List<TagGroup> TagGroups { get; internal set; } = new List<TagGroup>();

		/// <summary>
		/// The recently added tag
		/// </summary>
		public static Tag RecentlyAddedTag { get; internal set; } = null;

		/// <summary>
		/// The just played tag
		/// </summary>
		public static Tag JustPlayedTag { get; internal set; } = null;

		/// <summary>
		/// The identity of the album whose song was just played
		/// </summary>
		public static int JustPlayedAlbumId { get; set; } = -1;

		/// <summary>
		/// The number of times a song from the same album have been played consequtively
		/// </summary>
		public static int JustPlayedCount { get; set; } = 0;

		/// <summary>
		/// The number of times a song must be played for the album to be considered as having been played
		/// </summary>
		public const int JustPlayedLimit = 3;
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