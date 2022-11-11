using System;

namespace CoreMP
{
	/// <summary>
	/// The interface that the UI component has to implement
	/// </summary>
	public interface ICoreMP : Logger.ILogger
	{
		/// <summary>
		/// The path for the persistent storage
		/// </summary>
		public string StoragePath { get; }

		/// <summary>
		/// Post an action on the UI thread
		/// </summary>
		/// <param name="post"></param>
		public void PostAction( Action post );
	}
}
