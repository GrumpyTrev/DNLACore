using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The SQLiteCollection generic class specialises the ModelCollection class in order to 
	/// persist additions and deletions to the database
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SQLiteCollection<T> : ModelCollection<T> where T : new()
	{
		/// <summary>
		/// Public constructor. Save the collection that is being persisted
		/// </summary>
		/// <param name="collection"></param>
		public SQLiteCollection( List<T> collection ) => innerList = collection;

		/// <summary>
		/// Add an item to the collection and to the database
		/// Wait for the database operation to complete
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override async Task AddAsync( T item )
		{
			innerList.Add( item );
			await DbAccess.InsertAsync( item );
		}

		/// <summary>
		/// Clearing is not implemented
		/// </summary>
		/// <exception cref="NotImplementedException"></exception>
		public override void Clear() => throw new NotImplementedException();
	
		/// <summary>
		/// Delete an item from the database and the collection.
		/// Don't wait for the deletion
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool Remove( T item )
		{
			DbAccess.DeleteAsync( item );

			return innerList.Remove( item );
		}
	}
}
