using System.Threading.Tasks;

namespace CoreMP
{
	internal interface IStorageProvider
	{
		public Task LoadCollectionsAsync();

		public Artist CreateArtist();

		public Album CreateAlbum();

		public Song CreateSong();

		public ArtistAlbum CreateArtistAlbum();

		public Source CreateSource();

		public Library CreateLibrary();
	}
}
