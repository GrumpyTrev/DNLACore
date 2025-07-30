using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The Sources class holds a collection of all the Source entries read from storage.
	/// </summary>	
	internal static class Sources
	{
		/// <summary>
		/// Perform any action required after the collection has been loaded
		/// </summary>
		/// <returns></returns>
		public static void CollectionLoaded()
		{
			foreach ( Source source in SourceCollection )
			{
				source.InitialiseAccess();
			}
		}

		/// <summary>
		/// Add a new Source to the storage and the local collections
		/// </summary>
		/// <param name="sourceToAdd"></param>
		public static async Task AddSourceAsync( Source sourceToAdd ) => await SourceCollection.AddAsync( sourceToAdd );

		/// <summary>
		/// Delete the specified Source from the storage and the collections
		/// </summary>
		/// <param name="sourceToDelete"></param>
		/// <returns></returns>
		public static void DeleteSource( Source sourceToDelete ) => SourceCollection.Remove( sourceToDelete );

		/// <summary>
		/// The set of Artists currently held in storage
		/// </summary>
		public static ModelCollection<Source> SourceCollection { get; set; } = null;
	}
}
