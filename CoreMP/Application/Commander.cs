using System;
using System.Collections.Generic;
using System.Text;

namespace CoreMP
{
	public class Commander
	{
		public void SelectLibrary( Library libraryToSelect ) => LibraryManagementController.SelectLibrary( libraryToSelect );

		public void ClearLibraryAsync( Library libraryToClear, Action finishedAction ) => LibraryManagementController.ClearLibraryAsync( libraryToClear, finishedAction );

		public void DeleteLibraryAsync( Library libraryToDelete, Action finishedAction ) => LibraryManagementController.DeleteLibraryAsync( libraryToDelete, finishedAction );

		public bool CheckLibraryEmpty( Library libraryToCheck ) => LibraryManagementController.CheckLibraryEmpty( libraryToCheck );

		public void CreateSourceForLibrary( Library libraryToAddSourceTo ) => LibraryManagementController.CreateSourceForLibrary( libraryToAddSourceTo );

		public void DeleteSource( Source sourceToDelete ) => LibraryManagementController.DeleteSource( sourceToDelete );

		public void CreateLibrary( string libraryName ) => LibraryManagementController.CreateLibrary( libraryName );
	}
}
