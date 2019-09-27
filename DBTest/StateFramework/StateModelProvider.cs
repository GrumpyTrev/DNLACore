using System;

namespace DBTest
{
	public class StateModelProvider
	{
		public static StateModel Get( Type modelType )
		{
			StateModel model = store.Get( modelType.FullName );

			if ( model == null )
			{
				model = Activator.CreateInstance( modelType ) as StateModel;
				store.Put( modelType.FullName, model );
			}
			else
			{
				model.IsNew = false;
			}

			return model;
		}

		private static StateModelStore store = new StateModelStore();
	}
}