using System.Collections.Generic;

namespace CoreMP
{
	/// <summary>
	/// The MediaNotificationViewModel allows the MediaNotificationController to pass media status to the Media Notification View 
	/// </summary>
	public static class MediaNotificationViewModel
	{
		/// <summary>
		/// Method introduced to trigger a notification when changes are made to the song being played
		/// </summary>
		public static void SongStarted( Song songStarted ) => NotificationHandler.NotifyPropertyChanged( songStarted );
		
		/// <summary>
		/// Method introduced to trigger a notification when a song has finished playing
		/// </summary>
		public static void SongFinished() => NotificationHandler.NotifyPropertyChanged( null );

		/// <summary>
		/// Method introduced to trigger a notification when the play state of the song changes
		/// </summary>
		public static void IsPlaying( bool isPlaying ) => NotificationHandler.NotifyPropertyChanged( isPlaying );
	}
}
