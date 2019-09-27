using System.Collections.Generic;

namespace DBTest
{
	public class StateModelStore
	{
		public void Put( string key, StateModel model )
		{
			if ( store.ContainsKey( key ) == true )
			{
				store[ key ].OnClear();
			}

			store[ key ] = model;
		}

		public StateModel Get( string key )
		{
			StateModel model = null;

			if ( store.ContainsKey( key ) == true )
			{
				model = store[ key ];
			}

			return model;
		}

		public void Clear()
		{
			foreach ( StateModel model in store.Values )
			{
				model.OnClear();
			}
		}

		private readonly Dictionary< string, StateModel > store = new Dictionary<string, StateModel>();
	}
}