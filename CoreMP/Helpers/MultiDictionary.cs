using System;
using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The multi dictionary is a dictionary that contains more than one value per key
	/// </summary>
	/// <typeparam name="T">The type of the key</typeparam>
	/// <typeparam name="K">The type of the list contents</typeparam>
	public class MultiDictionary<T, K>: Dictionary<T, List<K>>
	{
		/// <summary>
		/// Adds a new value to the list associated with the specified key
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="newItem">The new item to add</param>
		public void AddValue( T key, K newItem )
		{
			EnsureKey( key );
			this[ key ].Add( newItem );
		}

		/// <summary>
		/// Adds a list of values to append to the list associated with the specified key
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="newItems">The new items to add</param>
		public void AddValues( T key, IEnumerable<K> newItems )
		{
			EnsureKey( key );
			this[ key ].AddRange( newItems );
		}

		/// <summary>
		/// Removes a specific element from the dictionary
		/// If the value list is empty the key is removed from the dictionary
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="value">The value to remove</param>
		/// <returns>Returns false if the key was not found</returns>
		public bool RemoveValue( T key, K value )
		{
			bool found = false;

			if ( ContainsKey( key ) == true )
			{
				found = true;

				this[ key ].Remove( value );

				if ( this[ key ].Count == 0 )
				{
					Remove( key );
				}
			}

			return found;
		}

		/// <summary>
		/// Removes all items that match the prediacte
		/// If the value list is empty the key is removed from the dictionary
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="match">The predicate to match the items</param>
		/// <returns>Returns false if the key was not found</returns>
		public bool RemoveAllValue( T key, Predicate<K> match )
		{
			bool found = false;

			if ( ContainsKey( key ) == true )
			{
				found = true;

				this[ key ].RemoveAll( match );

				if ( this[ key ].Count == 0 )
				{
					Remove( key );
				}
			}

			return found;
		}

		/// <summary>
		/// Checks if the key is already present
		/// </summary>
		/// <param name="key"></param>
		private void EnsureKey( T key )
		{
			if ( ContainsKey( key ) == false )
			{
				this[ key ] = new List< K >( 1 );
			}
			else
			{
				if ( this[ key ] == null )
				{
					this[ key ] = new List< K >( 1 );
				}
			}
		}
	}
}
