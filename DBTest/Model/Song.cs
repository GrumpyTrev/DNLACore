using SQLite;

namespace DBTest
{
    public partial class Song
    {
        [Ignore]
        public ScanActionType ScanAction { get; set; }

        /// <summary>
        /// This entry is not in the database but is set for songs that are being played
        /// </summary>
        [Ignore]
        public Artist Artist { get; set; } = null;

        public enum ScanActionType { NotMatched, Matched, Differ, New };
    }
}
