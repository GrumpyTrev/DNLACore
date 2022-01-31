using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
    public static class Songs
    {
        /// <summary>
        /// Get the Songs collection from storage
        /// </summary>
        /// <returns></returns>
        public static async Task GetDataAsync()
        {
			// Get the current set of songs
			SongCollection = await DbAccess.LoadAsync<Song>();

			// Form the lookups
			foreach ( Song song in SongCollection )
			{
				IdLookup[ song.Id ] = song;
				artistAlbumLookup.AddValue( song.ArtistAlbumId, song );
				albumLookup.AddValue( song.AlbumId, song );
			}
		}

		/// <summary>
		/// Return the Song with the specified Id or null if not found
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Song GetSongById( int id ) => IdLookup.GetValueOrDefault( id );

		/// <summary>
		/// Return all the songs associated with the specified Album
		/// </summary>
		/// <param name="albumId"></param>
		/// <returns></returns>
		public static List<Song> GetAlbumSongs( int albumId ) => albumLookup[ albumId ];

		/// <summary>
		/// Return all the songs associated with the specified ArtistAlbum
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		public static List<Song> GetArtistAlbumSongs( int artistAlbumId ) => artistAlbumLookup.ContainsKey( artistAlbumId ) == true ? 
			artistAlbumLookup[ artistAlbumId ] : new List<Song>();

		/// <summary>
		/// Return all the songs associated with the specified Source
		/// </summary>
		/// <param name="sourceId"></param>
		/// <returns></returns>
		public static List<Song> GetSourceSongs( int sourceId ) => SongCollection.Where( song => song.SourceId == sourceId ).ToList();

        /// <summary>
        /// Return all the songs associated with the specified Source
        /// </summary>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public static List<Song> GetSourceSongsWithName( int sourceId, string name ) => 
            SongCollection.Where( song => ( song.SourceId == sourceId ) && ( song.Title == name ) ).ToList();

		/// <summary>
		/// Add the specified Song to the local collections and persistent storage
		/// </summary>
		/// <param name="songToAdd"></param>
		public static async void AddSongAsync( Song songToAdd )
		{
			// Must wait for this to get the song id
			await DbAccess.InsertAsync( songToAdd );

			lock ( lockObject )
			{
				SongCollection.Add( songToAdd );

				if ( IdLookup.ContainsKey( songToAdd.Id ) == false )
				{
					IdLookup.Add( songToAdd.Id, songToAdd );
					artistAlbumLookup.AddValue( songToAdd.ArtistAlbumId, songToAdd );
					albumLookup.AddValue( songToAdd.AlbumId, songToAdd );
				}
				else
				{
					Logger.Log( $"Song {songToAdd.Title} from {songToAdd.Path} with id {songToAdd.Id} already added" );
				}
			}
		}

		/// <summary>
		/// Remove the supplied list of songs from local and persistent storage.
		/// This is used for bulk deletion, so rather than removing each song from the collection, O(n), reform the collection ignoring
		/// thoses to be delted
		/// </summary>
		/// <param name="songsToDelete"></param>
		public static void DeleteSongs( List<Song> songsToDelete )
		{
			lock ( lockObject )
			{
				// Form a hash from all the song ids being deleted
				HashSet<int> songIds = new( songsToDelete.Select( song => song.Id ) );

				// Make a new collection that only contains entries not in the deleted songs
				SongCollection = SongCollection.Where( song => songIds.Contains( song.Id ) == false ).ToList();

				// Reform the lookups
				artistAlbumLookup = new();
				albumLookup = new();
				IdLookup = new();
				foreach ( Song song in SongCollection )
				{
					IdLookup[ song.Id ] = song;
					artistAlbumLookup.AddValue( song.ArtistAlbumId, song );
					albumLookup.AddValue( song.AlbumId, song );
				}
			}

			DbAccess.DeleteItemsAsync( songsToDelete );
		}

		/// <summary>
		/// Delete a single song from local and peristanet storage
		/// </summary>
		/// <param name="songToDelete"></param>
		public static void DeleteSong( Song songToDelete )
		{
			lock( lockObject )
			{
				if ( IdLookup.ContainsKey( songToDelete.Id ) == true )
				{
					SongCollection.Remove( songToDelete );
					IdLookup.Remove( songToDelete.Id );
					artistAlbumLookup[ songToDelete.ArtistAlbumId ].Remove( songToDelete );
					albumLookup[ songToDelete.AlbumId ].Remove( songToDelete );
				}
			}

			DbAccess.DeleteAsync( songToDelete );
		}

		/// <summary>
		/// The set of Songs currently held in storage
		/// </summary>
		public static List<Song> SongCollection { get; set; } = new List<Song>();

        /// <summary>
        /// Lookup table indexed by song id
        /// </summary>
        private static Dictionary<int, Song> IdLookup { get; set; } = new Dictionary<int, Song>();

		/// <summary>
		/// Lookup table indexed by ArtistAlbum id
		/// </summary>
		private static MultiDictionary<int, Song> artistAlbumLookup = new();

		/// <summary>
		/// Lookup table indexed by Album id
		/// </summary>
		private static MultiDictionary<int, Song> albumLookup = new();

		/// <summary>
		/// Object used to lock collections
		/// </summary>
		private static readonly object lockObject = new();
    }
}
