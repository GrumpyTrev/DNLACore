using Android.Content;
using Android.OS;

namespace DBTest
{
	/// <summary>
	/// The KeepAwake class uses a WakeLock provided by the PowerManager to keep the appication running on Android eve if the phone locks
	/// </summary>
	internal class KeepAwake
	{
		/// <summary>
		/// Get an instance of the PowerManager to aquire a wake lock
		/// </summary>
		/// <param name="context"></param>
		public KeepAwake( Context context )
		{
			PowerManager pm = PowerManager.FromContext( context );
			wakeLock = pm.NewWakeLock( WakeLockFlags.Partial, "DBTest" );
		}

		/// <summary>
		/// Aquire the wakelock if not already held
		/// </summary>
		public void AquireLock()
		{
			if ( wakeLock.IsHeld == false )
			{
				wakeLock.Acquire();
			}
		}

		/// <summary>
		/// Release the wakelock if hels
		/// </summary>
		public void ReleaseLock()
		{
			if ( wakeLock.IsHeld == true )
			{
				wakeLock.Release();
			}
		}

		/// <summary>
		/// Lock used to keep the app alive
		/// </summary>
		private readonly PowerManager.WakeLock wakeLock = null;
	}
}
