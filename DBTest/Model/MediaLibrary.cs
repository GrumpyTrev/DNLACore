﻿using SQLite;
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

		[Ignore]
		public List<Song> Songs { get; set; }
	}

	[Table( "NewSource" )]
	public class NewSource
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		/// <summary>
		/// The name of this source for display purposes
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Access type - currently "Local" or "Remote"
		/// </summary>
		public string AccessType { get; set; }

		/// <summary>
		/// The IP address of the device where the music is stored
		/// If left blank this indicates that the local IP address of the phone should be used
		/// </summary>
		public string IPAddress { get; set; }

		/// <summary>
		/// The location on the device where the songs can be found.
		/// For local devices this will be the full name of the directory onn the device
		/// For remote devices this will be the name that the device's HTTP server responds to
		/// </summary>
		public string FolderName { get; set; }

		/// <summary>
		/// The HTTP port number
		/// </summary>
		public int PortNo { get; set; }

		/// <summary>
		/// The source used when scanning - derived from above
		/// For remote devices this is '{IPAddress}'
		/// For local devices this is '/{FolderName}/'
		/// </summary>
		[Ignore]
		public string ScanSource { get; set; }

		/// <summary>
		/// The type of access used for scanning - derived from above
		/// For remote devices this will be 'FTP'
		/// For local devices this will be 'Local'
		/// </summary>
		[Ignore]
		public string ScanType { get; set; }

		/// <summary>
		/// The location used to access the songs when playing them locally
		/// For remote devices this will be 'http://{IPAddress}:{PortNo}/{FolderName}'
		/// For local devices this will be '/{FolderName}'
		/// </summary>
		[Ignore]
		public string LocalAccess { get; set; }

		/// <summary>
		/// The location used to access the songs when playing them remotely
		/// For both remote and local devices this will be 'http://{IPAddress}:{PortNo}/{FolderName}'
		/// </summary>
		[Ignore]
		public string RemoteAccess { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }

		[Ignore]
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

		/// <summary>
		/// This entry is not in the database but is set for songs that are being played
		/// </summary>
		[Ignore]
		public Artist Artist { get; set; } = null;

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

		[Ignore]
		public List<ArtistAlbum> ArtistAlbums { get; set; } = new List<ArtistAlbum>();

		/// <summary>
		/// Indicates when all the details for the Artist have been read
		/// </summary>
		[Ignore]
		public bool DetailsRead { get; set; } = false;
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

		public bool Played { get; set; } = false;

		public int Year { get; set; } = 0;

		/// <summary>
		/// The rating is from 0 (bad) to 4 (bril)
		/// </summary>
		public int Rating { get; set; } = 2;

		[ForeignKey( typeof( Genre ) )]
		public int GenreId { get; set; }

		/// <summary>
		/// This entry is not in the database but is set when the album is read from the database
		/// </summary>
		[Ignore]
		public string Genre { get; set; } = "";
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

		/// <summary>
		/// This entry is not in the database but needs to be accessed when an ArtistAlbum is selected
		/// </summary>
		[Ignore]
		public Artist Artist { get; set; }
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

		/// <summary>
		/// Synchronise tagged albums across libraries
		/// </summary>
		public bool Synchronise { get; set; } = false;

		[OneToMany]
		public List<TaggedAlbum> TaggedAlbums { get; set; } = new List<TaggedAlbum>();
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

		public override bool Equals( object obj ) => ( obj == null ) ? false : ( ( ( TaggedAlbum )obj ).AlbumId == AlbumId );

		public override int GetHashCode() => AlbumId.GetHashCode();
	}

	[Table( "Genre" )]
	public class Genre
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[OneToMany]
		public List<Album> Albums { get; set; }
	}
}