using Android.Content;
using Android.Widget;

namespace DBTest
{
	/// <summary>
	/// The MediaControllerNoHide class extends the standard MediaController in order to prevent it being hidden
	/// </summary>
	class MediaControllerNoHide : MediaController
	{
		public MediaControllerNoHide( Context theContext ) : base( theContext )
		{
		}

		/// <summary>
		/// Override the Hide to do nothing
		/// </summary>
		public override void Hide()
		{
		}
	}
}