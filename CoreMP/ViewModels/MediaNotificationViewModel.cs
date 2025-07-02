using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The MediaNotificationViewModel allows the MediaNotificationController to pass media status to the MediaNotificationServiceInterface 
	/// </summary>
	public class MediaNotificationViewModel
	{
		/// <summary>
		/// A song has either started or stopped playing - value is null for song finishing
		/// </summary>
		public static Song SongStarted
		{
			set => NotificationHandler.NotifyPropertyChanged( value );
		}

		/// <summary>
		/// Method introduced to trigger a notification when the play state of the song changes
		/// </summary>
		public static bool IsPlaying
		{
			set => NotificationHandler.NotifyPropertyChanged( value );
		}
	}
}
