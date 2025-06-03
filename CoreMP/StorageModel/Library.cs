using System;
using System.Collections.Generic;
using SQLite;

namespace CoreMP
{
	public class Library
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		[Obsolete( "Do not create model instances directly", false )]
		public Library() { }

		public virtual int Id { get; set; }

		public string Name { get; set; }

		/// <summary>
		/// The Source instances associated with this Library
		/// </summary>

		public List<Source> LibrarySources { get; internal set; } = new List<Source>();

		/// <summary>
		/// Add a new source to the collection and to persistent storage
		/// </summary>
		/// <param name="sourceToAdd"></param>
		/// <returns></returns>
		public void AddSource( Source sourceToAdd )
		{
			LibrarySources.Add( sourceToAdd );
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			_ = Sources.AddSourceAsync( sourceToAdd );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			// Initialise any source data that may not have been set in the new source
			sourceToAdd.InitialiseAccess();

			// Report the change
			if ( StorageController.Loading == false )
			{
				NotificationHandler.NotifyPropertyChanged( this );
			}
		}

		/// <summary>
		/// Delete the speciifed source from the local collection and database
		/// </summary>
		/// <param name="sourceToDelete"></param>
		public void DeleteSource( Source sourceToDelete )
		{
			_ = LibrarySources.Remove( sourceToDelete );
			Sources.DeleteSource( sourceToDelete );

			// Report the change
			if ( StorageController.Loading == false )
			{
				NotificationHandler.NotifyPropertyChanged( this );
			}
		}
	}
}
