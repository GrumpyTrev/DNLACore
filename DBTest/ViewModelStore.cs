using System.Collections.Generic;

namespace DBTest
{
	public class ViewModelStore
	{
		public void Put( string key, ViewModel model )
		{
			if ( store.ContainsKey( key ) == true )
			{
				store[ key ].OnClear();
			}

			store[ key ] = model;
		}

		public ViewModel Get( string key )
		{
			ViewModel model = null;

			if ( store.ContainsKey( key ) == true )
			{
				model = store[ key ];
			}

			return model;
		}

		public void Clear()
		{
			foreach ( ViewModel model in store.Values )
			{
				model.OnClear();
			}
		}

		private readonly Dictionary< string, ViewModel > store = new Dictionary<string, ViewModel>();
	}
}