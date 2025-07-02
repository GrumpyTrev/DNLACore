using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreMP
{
	/// <summary>
	/// The ModelCollection class is the base class for collections that need to be persisted.
	/// The persistence mechanism is implemented in the specialised classes that are created to hold the collection
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ModelCollection<T> : IList<T>
	{
		/// <summary>
		/// The following methods are not allowed to be used
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public T this[ int index ] { get => innerList[ index ]; set => throw new NotImplementedException(); }

		/// <summary>
		/// This class is designed for asynch Add operations, so the synch operation is not allowed
		/// </summary>
		/// <param name="item"></param>
		/// <exception cref="NotImplementedException"></exception>
		[Obsolete( "This method is not implemented!", true )]
		public void Add( T item ) => throw new NotImplementedException();

		[Obsolete( "This method is not implemented!", true )]
		public void Insert( int index, T item ) => throw new NotImplementedException();

		[Obsolete( "This method is not implemented!", true )]
		public void RemoveAt( int index ) => throw new NotImplementedException();

		/// <summary>
		/// This is the Add method that specialised classes must implement
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public abstract Task AddAsync( T item );

		/// <summary>
		/// Derived class must implement the clear operation
		/// </summary>
		public abstract void Clear();

		/// <summary>
		/// Derived classes must implement item removal
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public abstract bool Remove( T item );

		// All other interface methods are handled by the actual collection

		public bool IsReadOnly => false;

		public int Count => innerList.Count;

		public bool Contains( T item ) => innerList.Contains( item );

		public void CopyTo( T[] array, int arrayIndex ) => innerList.CopyTo( array, arrayIndex );

		public IEnumerator<T> GetEnumerator() => innerList.GetEnumerator();

		public int IndexOf( T item ) => innerList.IndexOf( item );

		IEnumerator IEnumerable.GetEnumerator() => innerList.GetEnumerator();

		public int FindIndex( Predicate<T> match ) => innerList.FindIndex( match );

		/// <summary>
		/// The actual colletion of objects
		/// </summary>
		protected List<T> innerList = new List<T>();
	}
}
