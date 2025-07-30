using Android.App;
using Android.Content;
using Android.OS;

namespace DBTest
{
	/// <summary>
	/// The ApplicationShutdownService is used to trap application shutdown and pass it back to its interface
	/// </summary>
	[Service]
	internal class ApplicationShutdownService : Service
	{
		/// <summary>
		/// Called when the service has been created to return the IBinder instance for the service
		/// </summary>
		/// <param name="intent"></param>
		/// <returns></returns>
		public override IBinder OnBind( Intent intent ) => serviceBinder;

		/// <summary>
		/// Called when the service is first created. Create the binder to pass back the service instance
		/// </summary>
		public override void OnCreate()
		{
			base.OnCreate();

			serviceBinder = new ApplicationShutdownServiceBinder( this );
		}

		/// <summary>
		/// Called when the application that started this service is removed from the process stack
		/// </summary>
		/// <param name="rootIntent"></param>
		public override void OnTaskRemoved( Intent rootIntent )
		{
			Reporter?.Shutdown();

			base.OnTaskRemoved( rootIntent );
			StopSelf();
		}

		/// <summary>
		/// The instance used to report back significant events
		/// </summary>
		public IServiceCallbacks Reporter { get; set; } = null;

		/// <summary>
		/// The Binder class for this service defining the interface betweeen the service and the appication
		/// </summary>
		public class ApplicationShutdownServiceBinder : Binder
		{
			/// <summary>
			/// Create the binder and save the service instance
			/// </summary>
			/// <param name="theService"></param>
			public ApplicationShutdownServiceBinder( ApplicationShutdownService theService ) => Service = theService;

			/// <summary>
			/// The service instance passed back to the application
			/// </summary>
			public ApplicationShutdownService Service { get; } = null;
		}

		/// <summary>
		/// The interface defining the calls back to the application
		/// </summary>
		public interface IServiceCallbacks
		{
			void Shutdown();
		}

		/// <summary>
		/// The IBinder instance for this service
		/// </summary>
		private IBinder serviceBinder = null;
	}
}
