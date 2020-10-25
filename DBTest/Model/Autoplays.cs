using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBTest
{
	/// <summary>
	/// The Autoplays class contains the collection of Autoplay records
	/// </summary>
	static class Autoplays
	{
		/// <summary>
		/// Get the Autoplay collection from storage
		/// </summary>
		/// <returns></returns>
		public static async Task GetDataAsync()
		{
			if ( AutoplayCollection == null )
			{
				// Get the current set of autoplays
				AutoplayCollection = await AutoplayAccess.GetAutoplaysAsync();
			}
		}

		/// <summary>
		/// The set of Autoplays currently held in storage
		/// </summary>
		public static List<Autoplay> AutoplayCollection { get; set; } = null;
	}
}