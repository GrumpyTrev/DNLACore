using System.Collections.Generic;
using System.Linq;

namespace CoreMP
{
	public static class FilterManagementModel
	{
		/// <summary>
		/// The set of all tag groups available for the user to choose from
		/// </summary>
		public static List<TagGroup> TagGroups { get; internal set; } = new List<TagGroup>();

		/// <summary>
		/// The just played tag
		/// </summary>
		public static Tag JustPlayedTag { get; internal set; } = null;

		/// <summary>
		/// The not played tag
		/// </summary>
		public static Tag NotPlayedTag { get; internal set; } = null;

		/// <summary>
		/// The identity of the album whose song was just played
		/// </summary>
		public static int JustPlayedAlbumId { get; internal set; } = -1;

		/// <summary>
		/// The number of times a song from the same album have been played consequtively
		/// </summary>
		public static int JustPlayedCount { get; internal set; } = 0;

		/// <summary>
		/// The Genre tags
		/// </summary>
		public static TagGroup GenreTags
		{
			get
			{
				genreTags ??= TagGroups.Single( tg => tg.Name == "Genre" );

				return genreTags;
			}
		}

		/// <summary>
		/// The number of times a song must be played for the album to be considered as having been played
		/// </summary>
		public const int JustPlayedLimit = 3;

		/// <summary>
		/// The Genre tag group
		/// </summary>
		private static TagGroup genreTags = null;
	}

	/// <summary>
	/// The AppliedTag class records how a tag has been applied (typically to a set of albums)
	/// </summary>
	public class AppliedTag
	{
		/// <summary>
		/// The source tag
		/// </summary>
		public Tag SourceTag { get; internal set; } = null;

		/// <summary>
		/// How fully has this tag been applied
		/// </summary>
		public AppliedType Applied { get; internal set; } = AppliedType.None;

		/// <summary>
		/// The original value against which changes can be determined
		/// </summary>
		public AppliedType OriginalApplied { get; internal set; } = AppliedType.None;

		/// <summary>
		/// Application level
		/// </summary>
		public enum AppliedType { None, Some, All };
	}
}
