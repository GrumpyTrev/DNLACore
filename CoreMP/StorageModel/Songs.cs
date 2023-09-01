using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
    internal static class Songs
    {
        /// <summary>
        /// Get the Songs collection from storage
        /// </summary>
        /// <returns></returns>
        public static void CollectionLoaded()
        {
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
		public static List<Song> GetAlbumSongs( int albumId ) => albumLookup.ContainsKey( albumId ) ? albumLookup[ albumId ] : new List<Song>();

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
		public static async Task AddSongAsync( Song songToAdd )
		{
			// Must wait for this to get the song id
			await SongCollection.AddAsync( songToAdd );
			IdLookup.Add( songToAdd.Id, songToAdd );
			artistAlbumLookup.AddValue( songToAdd.ArtistAlbumId, songToAdd );
			albumLookup.AddValue( songToAdd.AlbumId, songToAdd );
		}

		/// <summary>
		/// Remove the supplied list of songs from local and persistent storage.
		/// </summary>
		/// <param name="songsToDelete"></param>
		public static void DeleteSongs( List<Song> songsToDelete )
		{
			foreach ( Song songToDelete in songsToDelete )
			{
				DeleteSong( songToDelete );
			}
		}

		/// <summary>
		/// Delete a single song from local and peristanet storage
		/// </summary>
		/// <param name="songToDelete"></param>
		public static void DeleteSong( Song songToDelete )
		{
			SongCollection.Remove( songToDelete );
			IdLookup.Remove( songToDelete.Id );
			artistAlbumLookup[ songToDelete.ArtistAlbumId ].Remove( songToDelete );
			albumLookup[ songToDelete.AlbumId ].Remove( songToDelete );
		}

		/// <summary>
		/// The set of Songs currently held in storage
		/// </summary>
		public static ModelCollection<Song> SongCollection { get; set; } = null;

        /// <summary>
        /// Lookup table indexed by song id
        /// </summary>
        private static Dictionary<int, Song> IdLookup { get; set; } = new Dictionary<int, Song>();

		/// <summary>
		/// Lookup table indexed by ArtistAlbum id
		/// </summary>
		private static readonly MultiDictionary<int, Song> artistAlbumLookup = new MultiDictionary<int, Song>();

		/// <summary>
		/// Lookup table indexed by Album id
		/// </summary>
		private static readonly MultiDictionary<int, Song> albumLookup = new MultiDictionary<int, Song>();
    }
}
