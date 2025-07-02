namespace CoreMP
{
	/// <summary>
	/// The TagModel is used to allow controllers access to changed tag notifications
	/// </summary>
	internal class TagModel
	{
		/// <summary>
		/// Property used to post tag change notification
		/// </summary>
		public static string ChangedTag
		{
			set => NotificationHandler.NotifyPropertyChangedPersistent( value );
		}
	}
}
