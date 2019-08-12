using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MobileApp
{
	/// <summary>
	/// The Device class represents a DNLA device
	/// </summary>
	class Device
	{
		public string IPAddress { get; set; }
		public string DescriptionURL { get; set; }
		public int Port { get; set; }
		public bool CanPlayMedia { get; set; } = false;
		public string PlayUrl { get; set; }
		public string FriendlyName { get; set; }
	}
}