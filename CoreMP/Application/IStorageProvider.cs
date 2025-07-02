using System.Threading.Tasks;

namespace CoreMP
{
	internal interface IStorageProvider
	{
		public Task LoadStorageAsync();

		public Artist CreateArtist();

		public Album CreateAlbum();

		public Song CreateSong();

		public ArtistAlbum CreateArtistAlbum();

		public Source CreateSource();

		public Library CreateLibrary();
	}
}
