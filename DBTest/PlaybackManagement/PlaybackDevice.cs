namespace DBTest
{
	/// <summary>
	/// The PlaybackDevice class represent a local or remote device that can be selected for media playback
	/// </summary>
	public class PlaybackDevice
	{
		/// <summary>
		/// Override Equals in order to specify what equality means for devices
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals( object obj )
		{
			PlaybackDevice otherDevice = ( PlaybackDevice )obj;
			return ( ( otherDevice.IPAddress == IPAddress ) && ( otherDevice.DescriptionUrl == DescriptionUrl ) &&
					( otherDevice.Port == Port ) );
		}

		/// <summary>
		/// Required due to Equals override
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Return a user friendly description of this device
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format( "IP:{0} Port:{1} Name:{2} Desc:{3}", IPAddress, Port, FriendlyName?.ToString() ?? "Not known", DescriptionUrl );
		}

		/// <summary>
		/// The IP address of the device
		/// </summary>
		public string IPAddress { get; set; }

		/// <summary>
		/// The Url used to interrogate the device for transport capabilities
		/// </summary>
		public string DescriptionUrl { get; set; }

		/// <summary>
		/// The port number to be used to communicate with the device
		/// </summary>
		public int Port { get; set; }

		/// <summary>
		/// Can this device playback media.
		/// Set to false until we discover otherwise
		/// </summary>
		public CanPlayMediaType CanPlayMedia { get; set; } = CanPlayMediaType.Unknown;

		/// <summary>
		/// Can this device playback media
		/// </summary>
		public enum CanPlayMediaType
		{
			Yes, No, Unknown
		};

		/// <summary>
		/// The Url used to send playback requests to the device
		/// </summary>
		public string PlayUrl { get; set; }

		/// <summary>
		/// The user friendly name of the device
		/// </summary>
		public string FriendlyName { get; set; }

		/// <summary>
		/// Is this the local (phone) playback device.
		/// </summary>
		public bool IsLocal { get; set; } = false;
	}
}