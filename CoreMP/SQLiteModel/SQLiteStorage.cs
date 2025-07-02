using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	public class SQLiteStorage : IStorageProvider
	{
		public async Task LoadStorageAsync()
		{
			Songs.SongCollection = new SQLiteCollection<Song>( ( await DbAccess.LoadAsync<SQLiteSong>() ).Cast<Song>().ToList() );
			Albums.AlbumCollection = new SQLiteCollection<Album>( ( await DbAccess.LoadAsync<SQLiteAlbum>() ).Cast<Album>().ToList() );
			Artists.ArtistCollection = new SQLiteCollection<Artist>( ( await DbAccess.LoadAsync<SQLiteArtist>() ).Cast<Artist>().ToList() );
			ArtistAlbums.ArtistAlbumCollection = new SQLiteCollection<ArtistAlbum>( ( await DbAccess.LoadAsync<SQLiteArtistAlbum>() ).Cast<ArtistAlbum>().ToList() );
			Sources.SourceCollection = new SQLiteCollection<Source>( ( await DbAccess.LoadAsync<SQLiteSource>() ).Cast<Source>().ToList() );
			Libraries.LibraryCollection = new SQLiteCollection<Library>( ( await DbAccess.LoadAsync<SQLiteLibrary>() ).Cast<Library>().ToList() );

			Playback.PlaybackInstance = ( await DbAccess.LoadAsync<SQLitePlayback>() )[ 0 ];
		}

		public Artist CreateArtist() => new SQLiteArtist();

		public Album CreateAlbum() => new SQLiteAlbum();

		public Song CreateSong() => new SQLiteSong();

		public ArtistAlbum CreateArtistAlbum() => new SQLiteArtistAlbum();

		public Source CreateSource() => new SQLiteSource();

		public Library CreateLibrary() => new SQLiteLibrary();
	}
}
