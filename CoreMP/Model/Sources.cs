﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Sources class holds a collection of all the Source entries read from storage.
	/// </summary>	
	public static class Sources
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
				SourceCollection = await DbAccess.LoadAsync<Source>();

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
		public static List<Source> GetSourcesAndSongsForLibrary( int libraryId )
		{
			List<Source> sources = SourceCollection.Where( sou => sou.LibraryId == libraryId ).ToList();
			foreach ( Source source in sources )
			{
				source.GetSongs();
			}

			return sources;
		}

		/// <summary>
		/// Add a new source to the collection and to persistent storage
		/// </summary>
		/// <param name="sourceToAdd"></param>
		/// <returns></returns>
		public static void AddSource( Source sourceToAdd )
		{
			SourceCollection.Add( sourceToAdd );

			// Need to wait for the source to be added to ensure that its ID is available
			DbAccess.InsertAsync( sourceToAdd );

			// Initialise any source data that may not have been set in the new source
			sourceToAdd.InitialiseAccess();
		}

		/// <summary>
		/// Delete the speciifed source from the local collection and database
		/// </summary>
		/// <param name="sourceToDelete"></param>
		public static void DeleteSource( Source sourceToDelete )
		{
			SourceCollection.Remove( sourceToDelete );
			DbAccess.DeleteAsync( sourceToDelete );
		}

		/// <summary>
		/// The set of Albums currently held in storage
		/// </summary>
		public static List<Source> SourceCollection { get; set; } = null;
	}
}