using System.Collections.Generic;
using System.Collections.ObjectModel;
using SQLite;

namespace CoreMP
{
	public partial class Library
	{
		/// <summary>
		/// The Source instances associated with this Library
		/// </summary>
		private ObservableCollection<Source> observableSources;
		private List<Source> actualSources;

		[Ignore]
		public List<Source> Sources
		{
			get => actualSources; 
			
			internal set
			{
				actualSources = value;
				observableSources = new ObservableCollection<Source>( actualSources );
				observableSources.CollectionChanged += ( sender, args ) => NotificationHandler.NotifyPropertyChanged( this );
			}
		}

		/// <summary>
		/// Add a new source to the collection and to persistent storage
		/// </summary>
		/// <param name="sourceToAdd"></param>
		/// <returns></returns>
		public void AddSource( Source sourceToAdd )
		{
			actualSources.Add( sourceToAdd );
			observableSources.Add( sourceToAdd );

			// Need to wait for the source to be added to ensure that its ID is available
			DbAccess.InsertAsync( sourceToAdd );

			// Initialise any source data that may not have been set in the new source
			sourceToAdd.InitialiseAccess();
		}

		/// <summary>
		/// Delete the speciifed source from the local collection and database
		/// </summary>
		/// <param name="sourceToDelete"></param>
		public void DeleteSource( Source sourceToDelete )
		{
			actualSources.Remove( sourceToDelete );
			observableSources.Remove( sourceToDelete );
			DbAccess.DeleteAsync( sourceToDelete );
		}
	}
}
