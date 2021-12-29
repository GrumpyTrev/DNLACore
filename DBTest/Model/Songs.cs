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
            if ( collectionLoaded == false )
            {
                // Get the current set of songs
                List<Song> loadedCollection = await DbAccess.LoadAsync<Song>();

                // Add to the collection held by this class
                AddSongsToCollection( loadedCollection );

                collectionLoaded = true;
            }
        }

        /// <summary>
        /// Return the Song with the specified Id or null if not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Song> GetSongById( int id )
        {
            // If the song is not in the lookup table yet then query the database
            Song retrievedSong = null;
            lock ( lockObject )
            {
                retrievedSong = IdLookup.GetValueOrDefault( id );
            }

            if ( retrievedSong == null )
            {
                retrievedSong = await DbAccess.GetSongAsync( id );

                if ( retrievedSong != null )
                {
                    AddSongsToCollection( new List<Song>() { retrievedSong } );
                }
            }

            return retrievedSong;
        }

        /// <summary>
        /// Return all the songs associated with the specified Album
        /// </summary>
        /// <param name="albumId"></param>
        /// <returns></returns>
        public static async Task< List<Song> > GetAlbumSongs( int albumId )
        {
            List<Song> albumSongs;

            if ( collectionLoaded == false )
            {
                albumSongs = await DbAccess.GetAlbumSongsAsync( albumId );

                AddSongsToCollection( albumSongs );
            }
            else
            {
                albumSongs = SongCollection.Where( song => song.AlbumId == albumId ).ToList();
            }

            return albumSongs;
        }

        /// <summary>
        /// Return all the songs associated with the specified ArtistAlbum
        /// </summary>
        /// <param name="artistAlbumId"></param>
        /// <returns></returns>
        public static async Task<List<Song>> GetArtistAlbumSongs( int artistAlbumId )
        {
            List<Song> albumSongs;

            if ( collectionLoaded == false )
            {
                albumSongs = await DbAccess.GetArtistAlbumSongsAsync( artistAlbumId );

                AddSongsToCollection( albumSongs );
            }
            else
            {
                albumSongs = ArtistAlbumLookup[ artistAlbumId ];
            }

            return albumSongs;
        }

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
		public static async Task AddSongAsync( Song songToAdd )
		{
			await DbAccess.InsertAsync( songToAdd );

			SongCollection.Add( songToAdd );
			IdLookup.Add( songToAdd.Id, songToAdd );
			ArtistAlbumLookup.AddValue( songToAdd.ArtistAlbumId, songToAdd );
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
				IdLookup = SongCollection.ToDictionary( song => song.Id, song => song );

				ArtistAlbumLookup = new MultiDictionary<int, Song>();
				SongCollection.ForEach( song => ArtistAlbumLookup.AddValue( song.ArtistAlbumId, song ) );
			}

			DbAccess.DeleteItemsAsync( songsToDelete );
		}

        /// <summary>
        /// Copy any entries in the collection just loaded into the main SongCollection, except for
        /// those already loadedAdd 
        /// </summary>
        /// <param name="songs"></param>
        private static void AddSongsToCollection( List<Song> songs )
        {
            lock ( lockObject )
            {
                foreach ( Song song in songs )
                {
                    if ( IdLookup.ContainsKey( song.Id ) == false )
                    {
                        SongCollection.Add( song );
                        IdLookup.Add( song.Id, song );
                        ArtistAlbumLookup.AddValue( song.ArtistAlbumId, song );
                    }
                }
            }
        }

		/// <summary>
		/// The set of Songs currently held in storage
		/// </summary>
		public static List<Song> SongCollection { get; set; } = new List<Song>();

		/// <summary>
		/// Has the main set of songs been loaded yet
		/// </summary>
		private static bool collectionLoaded = false;

        /// <summary>
        /// Lookup table indexed by song id
        /// </summary>
        private static Dictionary<int, Song> IdLookup { get; set; } = new Dictionary<int, Song>();

        /// <summary>
        /// Lookup table indexed by ArtistAlbum id
        /// </summary>
        private static MultiDictionary<int, Song> ArtistAlbumLookup = new();

        /// <summary>
        /// Object used to lock collections
        /// </summary>
        private static readonly object lockObject = new();
    }
}
