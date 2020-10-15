using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Sources class holds a collection of all the Source entries read from storage.
	/// </summary>	
	static class Sources
	{
		/// <summary>
		/// Get the Sources collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( SourceCollection == null )
			{
				// Get the current set of sources
				SourceCollection = await SourceAccess.GetAllSourcesAsync();

				// Set the ScanSource, ScanType, LocalAccess and RemoteAccess fields. 
				SourceCollection.ForEach( source => source.InitialiseAccess() );
			}
		}

		/// <summary>
		/// Return a list of all the sources associated with the specified library
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static List<Source> GetSourcesForLibrary( int libraryId ) => SourceCollection.Where( sou => sou.LibraryId == libraryId ).ToList();

		/// <summary>
		/// Get the sources associated with a library and their associated songs
		/// </summary>
		/// <param name="libraryId"></param>
		/// <returns></returns>
		public static async Task< List<Source> > GetSourcesAndSongsForLibraryAsync( int libraryId )
		{
			List<Source> sources = SourceCollection.Where( sou => sou.LibraryId == libraryId ).ToList();
			sources.ForEach( async source => await source.GetSongsAsync() );

			return sources;
		}

		/// <summary>
		/// The set of Albums currently held in storage
		/// </summary>
		public static List<Source> SourceCollection { get; set; } = null;
	}
}