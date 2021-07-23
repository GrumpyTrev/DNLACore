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
            if ( SongCollection == null )
            {
                // Get the current set of songs and form the lookup tables
                SongCollection = await DbAccess.LoadAsync<Song>();

                IdLookup = SongCollection.ToDictionary( alb => alb.Id );

                foreach ( Song song in SongCollection )
                {
                    artistAlbumLookup.AddValue( song.ArtistAlbumId, song );
                }
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
        public static List<Song> GetAlbumSongs( int albumId ) => SongCollection.Where( song => song.AlbumId == albumId ).ToList();

        /// <summary>
        /// Return all the songs associated with the specified ArtistAlbum
        /// </summary>
        /// <param name="artistAlbumId"></param>
        /// <returns></returns>
        public static List<Song> GetArtistAlbumSongs( int artistAlbumId ) => artistAlbumLookup[ artistAlbumId ];

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
        /// The set of Songs currently held in storage
        /// </summary>
        public static List<Song> SongCollection { get; set; } = null;

        /// <summary>
        /// Lookup table indexed by song id
        /// </summary>
        private static Dictionary<int, Song> IdLookup { get; set; } = null;

        /// <summary>
        /// Lookup table indexed by ArtistAlbum id
        /// </summary>
        private static MultiDictionary<int, Song> artistAlbumLookup = new MultiDictionary<int, Song>();
    }
}
