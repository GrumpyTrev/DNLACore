namespace DBTest
{
	/// <summary>
	/// The ViewModel class manages data fro an activity or fragment and importantly only gets destroyed when the
	/// parent application is no longer required.
	/// </summary>
	public abstract class ViewModel
	{
		/// <summary>
		/// This method will be called when this model is no longer required
		/// </summary>
		public virtual void OnClear()
		{
		}

		/// <summary>
		/// On the initial creation of the model this flag is set.
		/// On subsequent retrievals from the store this flag will be cleared
		/// </summary>
		public bool IsNew { get; set; } = true;
	}
}