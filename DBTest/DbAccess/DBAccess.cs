﻿using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using SQLite;

namespace DBTest
{
	/// <summary>
	/// The DbAccess class is used for DB operations that can be performed generically
	/// </summary>
	static class DbAccess
	{
		/// <summary>
		/// Get all the members of a table
		/// </summary>
		public static async Task<List<T>> LoadAsync<T>() where T : new() => await ConnectionDetailsModel.AsynchConnection.Table<T>().ToListAsync();

		/// <summary>
		/// Delete the specifed item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static async void DeleteAsync<T>( T item ) => await ConnectionDetailsModel.AsynchConnection.DeleteAsync( item );

		/// <summary>
		/// Insert the specifed item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static async Task InsertAsync<T>( T item ) => await ConnectionDetailsModel.AsynchConnection.InsertAsync( item );

		/// <summary>
		/// Insert the specifed item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static async Task InsertAllAsync<T>( IEnumerable<T> items ) => await ConnectionDetailsModel.AsynchConnection.InsertAllAsync( items );

		/// <summary>
		/// Update the specifed item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static async void UpdateAsync<T>( T item ) => await ConnectionDetailsModel.AsynchConnection.UpdateAsync( item );

		/// <summary>
		/// Delete the specified list of items
		/// </summary>
		/// <param name="items"></param>
		public static async void DeleteItemsAsync<T>( IEnumerable<T> items )
		{
			if ( items.Count() > 0 )
			{
				// Use the type of the first item to specify the class of objects being deleted, the collection type <T> could be a base type
				Type type = items.First().GetType();

				// Make sure we use any aliases
				string tableName = type.GetTableName();
				
				// Get the primary key of the class that is going to be used in the query
				PropertyInfo primaryKeyProperty = GetPrimaryKey( type );

				// Again make sure we use any aliases
				string primaryKeyColumnName = primaryKeyProperty.GetColumnName();

				List<List<object>> chunks = Split( items.Select( element => primaryKeyProperty.GetValue( element, null ) ).ToList(), 100 );
				foreach ( List<object> chunk in chunks )
				{
					string deleteQuery = string.Format( "delete from {0} where {1} in ({2})", tableName, primaryKeyColumnName, 
						string.Join( ",", Enumerable.Repeat( "?", chunk.Count ) ) );

					await ConnectionDetailsModel.AsynchConnection.ExecuteAsync( deleteQuery, chunk.ToArray() );
				}
			}
		}

		//
		// All the followimg non-generic access methods are used to access Songs.
		// Songs are different to all the other DB based objects in that they are not all read into memory, so specific queries are required.
		//

		/// <summary>
		/// Get the songs for the specified Album identity
		/// </summary>
		/// <param name="albumId"></param>
		public static async Task<List<Song>> GetAlbumSongsAsync( int albumId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.AlbumId == albumId ) ).ToListAsync();

		/// <summary>
		/// Get a Song entry from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<Song> GetSongAsync( int songId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.Id == songId ) ).FirstAsync();

		/// <summary>
		/// Get a set of song entries from the database
		/// </summary>
		/// <returns></returns>
		public static async Task<List<Song>> GetSongsAsync( IEnumerable<int> songIds )
		{
			// We know that we are only interested in Song entries (could be extended if required
			Type type = typeof( Song );

			// Make sure we use any aliases
			string tableName = type.GetTableName();

			// Get the primary key of the class that is going to be used in the query
			PropertyInfo primaryKeyProperty = GetPrimaryKey( type );

			// Again make sure we use any aliases
			string primaryKeyColumnName = primaryKeyProperty.GetColumnName();

			// Add the fetched songs to this list
			List<Song> songs = new List<Song>();

			// Split the ids into reasonably sized chunks
			List<List<string>> chunks = Split( songIds.Select( id => id.ToString() ).ToList(), 100 );
			foreach ( List<string> chunk in chunks )
			{
				string selectQuery = string.Format( "select * from {0} where {1} in ({2})", 
					tableName, primaryKeyColumnName, string.Join( ",", Enumerable.Repeat( "?", chunk.Count ) ) );

				songs.AddRange( await ConnectionDetailsModel.AsynchConnection.QueryAsync<Song>( selectQuery, chunk.ToArray() ) );
			}

			return songs;
		}

		/// <summary>
		/// Get the songs associated with a specific ArtistAlbum
		/// </summary>
		/// <param name="artistAlbumId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetArtistAlbumSongsAsync( int artistAlbumId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.ArtistAlbumId == artistAlbumId ) ).ToListAsync();

		/// <summary>
		/// Get all the songs in the specified source with the specified title 
		/// </summary>
		/// <param name="songName"></param>
		/// <param name="sourceId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetMatchingSongAsync( string songName, int sourceId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => ( song.Title == songName ) && ( song.SourceId == sourceId ) ).ToListAsync();

		/// <summary>
		/// Get all the songs associated with the specified source
		/// </summary>
		/// <param name="sourceId"></param>
		/// <returns></returns>
		public static async Task<List<Song>> GetSongsForSourceAsync( int sourceId ) =>
			await ConnectionDetailsModel.AsynchConnection.Table<Song>().Where( song => song.SourceId == sourceId ).ToListAsync();

		/// <summary>
		/// Has this property publi get and set accessors
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <returns></returns>
		private static bool IsPublicInstance( this PropertyInfo propertyInfo ) => ( propertyInfo != null ) &&
				   ( propertyInfo.GetMethod != null ) && ( propertyInfo.GetMethod.IsStatic == false ) && ( propertyInfo.GetMethod.IsPublic == true ) &&
				   ( propertyInfo.SetMethod != null ) && ( propertyInfo.SetMethod.IsStatic == false ) && ( propertyInfo.SetMethod.IsPublic == true );

		/// <summary>
		/// Return the specified attribute of a property
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="property"></param>
		/// <returns></returns>
		private static T GetAttribute<T>( this PropertyInfo property ) where T : Attribute
		{
			T[] attributes = ( T[] )property.GetCustomAttributes( typeof( T ), true );
			return ( attributes.Length > 0 ) ? attributes[ 0 ] : null;
		}

		/// <summary>
		/// Return the specified attribute of a type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		private static T GetAttribute<T>( this Type type ) where T : Attribute
		{
			T[] attributes = ( T[] )type.GetTypeInfo().GetCustomAttributes( typeof( T ), true );
			return ( attributes.Length > 0 ) ? attributes[ 0 ] : null;
		}

		/// <summary>
		/// Return the possibly aliased name for the specified table
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static string GetTableName( this Type type )
		{
			TableAttribute tableAttribute = type.GetAttribute<TableAttribute>();
			return ( ( tableAttribute != null ) && ( tableAttribute.Name != null ) ) ? tableAttribute.Name : type.Name;
		}

		/// <summary>
		/// Return the possibly aliased name for the specified column
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		private static string GetColumnName( this PropertyInfo property )
		{
			ColumnAttribute columnAttribute = property.GetAttribute<ColumnAttribute>();
			return ( ( columnAttribute != null ) && ( columnAttribute.Name != null ) ) ? columnAttribute.Name : property.Name;
		}

		/// <summary>
		/// Get the primary key for the specified type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static PropertyInfo GetPrimaryKey( Type type ) =>
			type.GetRuntimeProperties().Where( property => ( property.IsPublicInstance() == true ) && ( property.GetAttribute<PrimaryKeyAttribute>() != null ) ).FirstOrDefault();

		/// <summary>
		/// Split the supplied list of items into a collection of lists each of a maximum size
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <param name="sliceSize"></param>
		/// <returns></returns>
		private static List<List<T>> Split<T>( List<T> items, int sliceSize = 100 )
		{
			List<List<T>> list = new List<List<T>>();

			for ( int index = 0; index < items.Count; index += sliceSize )
			{
				list.Add( items.GetRange( index, Math.Min( sliceSize, items.Count - index ) ) );
			}

			return list;
		}
	}
}