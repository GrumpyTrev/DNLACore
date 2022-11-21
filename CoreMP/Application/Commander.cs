using System;
using System.Collections.Generic;

namespace CoreMP
{
	public class Commander
	{
		// LibraryManagementController commands
		public void SelectLibrary( Library libraryToSelect ) => libraryManagementController.SelectLibrary( libraryToSelect );

		public void ClearLibraryAsync( Library libraryToClear, Action finishedAction ) => libraryManagementController.ClearLibraryAsync( libraryToClear, finishedAction );

		public void DeleteLibraryAsync( Library libraryToDelete, Action finishedAction ) => libraryManagementController.DeleteLibraryAsync( libraryToDelete, finishedAction );

		public bool CheckLibraryEmpty( Library libraryToCheck ) => libraryManagementController.CheckLibraryEmpty( libraryToCheck );

		public void CreateSourceForLibrary( Library libraryToAddSourceTo ) => libraryManagementController.CreateSourceForLibrary( libraryToAddSourceTo );

		public void DeleteSource( Source sourceToDelete ) => libraryManagementController.DeleteSource( sourceToDelete );

		public void CreateLibrary( string libraryName ) => libraryManagementController.CreateLibrary( libraryName );

		// AlbumsController commands
		public void FilterAlbums( Tag newFilter ) => albumsController.SetNewFilter( newFilter );

		public void SortAlbums() => albumsController.SortData();

		// ArtistsController commands
		public void FilterArtists( Tag newFilter ) => artistsController.SetNewFilter(newFilter );

		public void SortArtists() => artistsController.SortArtists();

		// AutoplayController commands
		public void StartAutoplay( IEnumerable<Song> selectedSongs, IEnumerable<string> genres, bool playNow ) =>
			autoplayController.StartAutoplay( selectedSongs, genres, playNow );

		// FilterManagementController commands
		public void AddAlbumToTag( Tag toTag, Album albumToAdd, bool synchronise = true ) => filterManagementController.AddAlbumToTag( toTag, albumToAdd, synchronise );

		public void RemoveAlbumFromTag( Tag fromTag, Album albumToRemove ) => filterManagementController.RemoveAlbumFromTag( fromTag, albumToRemove );

		public void SynchroniseAlbumPlayedStatus() => filterManagementController.SynchroniseAlbumPlayedStatus();

		// LibraryScanController commands
		public void ScanLibrary( Library libraryToScan, Action scanFinished, Func<bool> scanCancelledCheck ) =>
			libraryScanController.ScanLibraryAsynch( libraryToScan, scanFinished, scanCancelledCheck );

		/// <summary>
		/// The controller instances
		/// </summary>
		private readonly AlbumsController albumsController = new AlbumsController();
		private readonly ArtistsController artistsController = new ArtistsController();
		private readonly AutoplayController autoplayController = new AutoplayController();
		private readonly FilterManagementController filterManagementController = new FilterManagementController();
		private readonly LibraryManagementController libraryManagementController = new LibraryManagementController();
#pragma warning disable IDE0052 // Remove unread private members
		private readonly LibraryNameDisplayController libraryNameDisplayController = new LibraryNameDisplayController();
#pragma warning restore IDE0052 // Remove unread private members
		private readonly LibraryScanController libraryScanController = new LibraryScanController();
	}
}
