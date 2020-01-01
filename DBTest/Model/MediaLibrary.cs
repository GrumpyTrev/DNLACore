using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace DBTest
{
	[Table( "Playback" )]
	public class Playback
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }

		/// <summary>
		/// The index of the song curently selected in the Now Playing Playlist
		/// </summary>
		public int SongIndex { get; set; }

		/// <summary>
		/// The name of the currently selected playback device
		/// </summary>
		public string PlaybackDeviceName { get; set; }
	}

	[Table( "Library" )]
	public class Library
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[OneToMany]
		public List<Source> Sources { get; set; }

		[OneToMany]
		public List<Artist> Artists { get; set; }

		[OneToMany]
		public List<Album> Albums { get; set; }

		[OneToMany]
		public List<Playlist> PlayLists { get; set; }
	}

	[Table( "Source" )]
	public class Source
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }
		public string ScanSource { get; set; }
		public string ScanType { get; set; }
		public string LocalAccess { get; set; }
		public string RemoteAccess { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }

		[OneToMany]
		public List<Song> Songs { get; set; }
	}

	[Table( "Song" )]
	public class Song
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Title { get; set; }
		public int Track { get; set; }

		public string Path { get; set; }

		public DateTime ModifiedTime { get; set; }
		public int Length { get; set; }

		[Ignore]
		public ScanActionType ScanAction { get; set; }

		[ForeignKey( typeof( Album ) )]
		public int AlbumId { get; set; }

		[ForeignKey( typeof( Source ) )]
		public int SourceId { get; set; }

		[ForeignKey( typeof( ArtistAlbum ) )]
		public int ArtistAlbumId { get; set; }

		public enum ScanActionType { NotMatched, Matched, Differ, New };
	}

	[Table( "Artist" )]
	public class Artist
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }

		[OneToMany]
		public List<ArtistAlbum> ArtistAlbums { get; set; }

		/// <summary>
		/// Once all the ArtistAlbums have been read this list is used to hold all the ArtistAlbum and Song entries
		/// in a single list
		/// </summary>
		public List<object> Contents { get; } = new List<object>();

		public void EnumerateContents()
		{
			foreach ( ArtistAlbum album in ArtistAlbums )
			{
				Contents.Add( album );
				Contents.AddRange( album.Songs );
			}
		}
	}

	[Table( "Album" )]
	public class Album
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }

		[OneToMany]
		public List<Song> Songs { get; set; }

		public string ArtistName { get; set; }
	}

	[Table( "ArtistAlbum" ) ]
	public class ArtistAlbum
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[ForeignKey( typeof( Album ) )]
		public int AlbumId { get; set; }

		[OneToOne]
		public Album Album { get; set; }

		[ForeignKey( typeof( Artist ) )]
		public int ArtistId { get; set; }

		[OneToMany]
		public List<Song> Songs { get; set; }
	}

	[Table( "PlayList" )]
	public class Playlist
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }

		[OneToMany]
		public List<PlaylistItem> PlaylistItems { get; set; }
	}

	[Table( "PlayListItem" )]
	public class PlaylistItem
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public int Track { get; set; }

		[ForeignKey( typeof( Playlist ) )]
		public int PlaylistId { get; set; }

		[ForeignKey( typeof( Song ) )]
		public int SongId { get; set; }

		[OneToOne]
		public Song Song { get; set; }

		/// <summary>
		/// This entry is not in the database but needs to be accessed via the Song's ArtistAlbum and its Artist id
		/// </summary>
		[Ignore]
		public Artist Artist { get; set; }
	}

	[Table( "Tag" )]
	public class Tag
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		/// <summary>
		/// Name to be displayed in tab when filter applied
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Is this a user or system tag
		/// </summary>
		public bool UserTag { get; set; } = true;

		/// <summary>
		/// The maximum number of albums that can be tagged (used by system tags only)
		/// </summary>
		public int MaxCount { get; set; } = -1;

		/// <summary>
		/// Sort tagged albums by tag id
		/// </summary>
		public bool TagOrder { get; set; } = false;

		[OneToMany]
		public List<TaggedAlbum> TaggedAlbums { get; set; }
	}

	[Table( "TaggedAlbum" )]
	public class TaggedAlbum
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public int TagIndex { get; set; }

		[ForeignKey( typeof( Album ) )]
		public int AlbumId { get; set; }

		[OneToOne]
		public Album Album { get; set; }

		[ForeignKey( typeof( Tag ) )]
		public int TagId { get; set; }
	}
}