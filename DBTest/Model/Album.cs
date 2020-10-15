using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;

namespace DBTest
{
	/// <summary>
	/// The Album class contains a named set of songs associated with one or more artists
	/// </summary>
	public partial class Album
	{
		/// <summary>
		/// Get teh songs associated with this Album
		/// </summary>
		public async Task GetSongsAsync()
		{
			if ( Songs == null )
			{
				Songs = await AlbumAccess.GetAlbumSongsAsync( Id );
			}
		}

		/// <summary>
		/// Set or clear the played flag
		/// </summary>
		/// <param name="newState"></param>
		public void SetPlayedFlag( bool newState )
		{
			Played = newState;

			// No need to wait for the storage to complete
			AlbumAccess.UpdateAlbumAsync( this );

			// Report the change
			new AlbumPlayedStateChangedMessage() { AlbumChanged = this }.Send();
		}

		[Ignore]
		public List<Song> Songs { get; set; } = null;
	}
}