using System.Collections.Generic;

namespace DBTest
{
	/// <summary>
	/// The LibraryScanner class is used to fill and empty library by scanning for songs from the library's sources
	/// </summary>
	class LibraryScanner
	{
		// Public interface specifying the library to scan
		public LibraryScanner( Library libraryToScan )
		{
			scanLibrary = libraryToScan;
		}

		/// <summary>
		/// Scan for songs from the sources associated with this library.
		/// </summary>
		public async void ScanLibrary()
		{
			// Make sure the children links are read as well to the get Source entries
			await LibraryAccess.GetLibraryChildrenAsync( scanLibrary );

			// Iterate all the sources associated with this library. Get the children as well
			List<Source> sources = await LibraryAccess.GetSourcesAsync( scanLibrary.Id, true );

			// Iterate through the sources for this libaray
			foreach ( Source sourceToScan in sources )
			{
				if ( sourceToScan.ScanType == "FTP" )
				{
					await new FTPScanner( new SongStorage( scanLibrary, sourceToScan ) ).Scan( sourceToScan.ScanSource );
				}
			}
		}
			  
		/// <summary>
		/// The libarary to scan
		/// </summary>
		private Library scanLibrary = null;
	}
}