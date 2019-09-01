using System;

namespace DBTest
{
	public class ViewModelProvider
	{
		public static ViewModel Get( Type modelType )
		{
			ViewModel model = store.Get( modelType.FullName );

			if ( model == null )
			{
				model = Activator.CreateInstance( modelType ) as ViewModel;
				store.Put( modelType.FullName, model );
			}
			else
			{
				model.IsNew = false;
			}

			return model;
		}

		private static ViewModelStore store = new ViewModelStore();
	}
}