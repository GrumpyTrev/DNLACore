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
	}

	/// <summary>
	/// The Source class specifies where a set of somngs can be found on a local or remote device
	/// </summary>
	[Table( "Source" )]
	public partial class Source
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
		/// For local devices this will be the full name of the directory on the device
		/// For remote devices this will be the name that the device's HTTP server responds to
		/// </summary>
		public string FolderName { get; set; }

		/// <summary>
		/// The HTTP port number
		/// </summary>
		public int PortNo { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }
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
	public partial class Artist
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }
	}

	[Table( "Album" )]
	public partial class Album
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }

		public string ArtistName { get; set; }

		public bool Played { get; set; } = false;

		public int Year { get; set; } = 0;

		/// <summary>
		/// The rating is from 0 (bad) to 4 (bril)
		/// </summary>
		public int Rating { get; set; } = 2;

		/// <summary>
		/// The full genre string that could include several genres is included in the database
		/// </summary>
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
	public partial class Playlist
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		public string Name { get; set; }

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }
	}

	[Table( "PlayListItem" )]
	public partial class PlaylistItem
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

	/// <summary>
	/// The Tag class is used to group together one or more albums
	/// </summary>
	[Table( "Tag" )]
	public partial class Tag
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		/// <summary>
		/// The full name of the tag
		/// </summary>
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
		/// Sort tagged albums by tag id
		/// </summary>
		public bool TagOrder { get; set; } = false;

		/// <summary>
		/// Synchronise tagged albums across libraries
		/// </summary>
		public bool Synchronise { get; set; } = false;
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

	[Table( "Autoplay" )]
	public partial class Autoplay
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		/// <summary>
		/// Is Autoplay currently active for the specified library
		/// </summary>
		public bool Active { get; set; } = false;

		[ForeignKey( typeof( Library ) )]
		public int LibraryId { get; set; }

		/// <summary>
		/// The genre populaton used for the last generation
		/// </summary>
		public int LastPopulation { get; set; } = -1;

		/// <summary>
		/// How fast are genres added as songs are played
		/// </summary>
		public SpreadType Spread { get; set; } = SpreadType.Slow;

		public enum SpreadType { NoSpread, Fast, Slow };

		/// <summary>
		/// The maximum number of times the list of genres is expanded 
		/// </summary>
		public int FastSpreadLimit { get; set; } = 2;

		/// <summary>
		/// Are all populations the target of the next generated song, or just populations linked to the current song
		/// </summary>
		public TargetType Target { get; set; } = TargetType.AllPopulations;

		public enum TargetType { AllPopulations, NextPopulation };

		/// <summary>
		/// How are selection weighted.
		/// </summary>
		public WeightType Weight { get; set; } = WeightType.None;

		public enum WeightType { None, Centre, Edge };
	}

	/// <summary>
	/// The GenrePopulation class is used to hold one or more Genres stored as a delimited string
	/// </summary>
	[Table( "GenrePopulation" )]
	public partial class GenrePopulation
	{
		[PrimaryKey, AutoIncrement, Column( "_id" )]
		public int Id { get; set; }

		/// <summary>
		/// The semicolon delimited list of genres held by this class
		/// </summary>
		public string GenreString { get; set; } = "";

		/// <summary>
		/// The population number of this record
		/// </summary>
		public int Index { get; set; } = -1;

		/// <summary>
		/// Link to the Autoplay instance that uses this set of Genres
		/// </summary>
		[ForeignKey( typeof( Autoplay ) )]
		public int AutoplayId { get; set; }
	}
}