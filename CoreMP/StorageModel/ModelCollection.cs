using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreMP
{
	public abstract class ModelCollection<T> : IList<T>
	{
		public abstract T CreateItem();

		public T this[ int index ] { get => innerList[ index ]; set => throw new NotImplementedException(); }

		public int Count => innerList.Count;

		public bool IsReadOnly => false;

		[Obsolete( "This method is not implemented!", true )]
		public void Add( T item ) => throw new NotImplementedException();

		public abstract Task AddAsync( T item );

		public abstract void Clear();

		public bool Contains( T item ) => innerList.Contains( item );

		public void CopyTo( T[] array, int arrayIndex ) => innerList.CopyTo( array, arrayIndex );

		public IEnumerator<T> GetEnumerator() => innerList.GetEnumerator();

		public int IndexOf( T item ) => innerList.IndexOf( item );

		public void Insert( int index, T item ) => throw new NotImplementedException();

		public abstract bool Remove( T item );

		public void RemoveAt( int index ) => throw new NotImplementedException();

		IEnumerator IEnumerable.GetEnumerator() => innerList.GetEnumerator();

		public int FindIndex( Predicate<T> match ) => innerList.FindIndex( match );

		/// <summary>
		/// The actual colletion of object
		/// </summary>
		protected List<T> innerList = new List<T>();
	}
}
