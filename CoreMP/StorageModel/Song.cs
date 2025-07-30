using System;

namespace CoreMP
{
	public class Song
	{
		[Obsolete( "Do not create model instances directly", false )]
		public Song() { }

		public virtual int Id { get; set; }

		public string Title { get; set; }

		public int Track { get; set; }

		/// <summary>
		/// The path associated with the song
		/// </summary>
		public virtual string Path { get; set; }

		public DateTime ModifiedTime { get; set; }

		public int Length { get; set; }

		public int AlbumId { get; set; }

		public int SourceId { get; set; }

		public int ArtistAlbumId { get; set; }

		public ScanActionType ScanAction { get; set; }

		/// <summary>
		/// This entry is not in the database but is set for songs that are being played
		/// </summary>
		public virtual Artist Artist { get; set; } = null;

		/// <summary>
		/// The Album that this song is on.
		/// This value is only obtained on demand
		/// </summary>

		public Album Album
		{
			get
			{
				album ??= Albums.GetAlbumById( AlbumId );

				return album;
			}
		}
		private Album album = null;

		public enum ScanActionType { NotMatched, Matched, Differ, New };
	}
}
