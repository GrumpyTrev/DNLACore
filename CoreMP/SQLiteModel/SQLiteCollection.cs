using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreMP
{
	public class SQLiteCollection<T> : ModelCollection<T> where T : new()
	{
		public SQLiteCollection( List<T> collection ) => innerList = collection;

		public override T CreateItem() => new T();

		public override async Task AddAsync( T item )
		{
			innerList.Add( item );
			await DbAccess.InsertAsync( item );
		}

		public override void Clear() => throw new NotImplementedException();
	
		public override bool Remove( T item )
		{
			DbAccess.DeleteAsync( item );

			return innerList.Remove( item );
		}
	}
}
