namespace DBTest
{
	public interface IAdapterActionHandler
	{
		void EnteredActionMode();

		void SelectedItemsChanged( int selectedItemsCount );
	}
}