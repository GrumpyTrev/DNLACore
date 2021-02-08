using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Autoplays class contains the collection of Autoplay records
	/// </summary>
	static class Autoplays
	{
		/// <summary>
		/// Get the Autoplay collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( AutoplayCollection == null )
			{
				// Get the current set of autoplays
				AutoplayCollection = await DbAccess.LoadAsync<Autoplay>();
			}
		}

		/// <summary>
		/// Get the Autoplay record associated with the specified library.
		/// If there is no such record then create one
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static async Task<Autoplay> GetAutoplayAsync( int libraryId )
		{
			Autoplay autoPlay = AutoplayCollection.SingleOrDefault( auto => auto.LibraryId == libraryId);

			if ( autoPlay == null )
			{
				autoPlay = new Autoplay() { LibraryId = libraryId };

				// Need to wait for thus so that it's Id gets set
				await DbAccess.InsertAsync( autoPlay );
			}

			return autoPlay;
		}

		/// <summary>
		/// Link each Autoplay with its stored Populations
		/// </summary>
		/// <returns></returns>
		public static async Task LinkPopulationsAsync()
		{
			await Task.Run( () =>
			{
				foreach ( Autoplay autoplay in AutoplayCollection )
				{
					autoplay.InitialisePopulations();
				}
			} );
		}

		/// <summary>
		/// The set of Autoplays currently held in storage
		/// </summary>
		public static List<Autoplay> AutoplayCollection { get; set; } = null;
	}
}