using System.Runtime.CompilerServices;

namespace CoreMP
{
	/// <summary>
	/// The ModelAvailble class works with the NotificationHandler to provide a mechanism to allow classes to register imterest in 
	/// view models becoming available
	/// </summary>
	public class ModelAvailable
	{
		public ModelAvailable( [CallerFilePath] string filePath = "" ) => modelFilePath = filePath;

		/// <summary>
		/// The name of the currently selected library
		/// </summary>
		private bool available = false;

		public bool IsSet
		{
			get => available;
			set
			{
				available = value;
				NotificationHandler.NotifyPropertyChangedPersistent( null, modelFilePath );
			}
		}

		private readonly string modelFilePath = "";
	}
}
