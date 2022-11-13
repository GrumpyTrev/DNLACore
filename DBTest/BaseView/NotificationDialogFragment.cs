﻿using Android.App;
using Android.OS;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Dialogue reporting some kind of problem with the requested action
	/// </summary>
	internal class NotificationDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show an alert dialogue with the specified Title and a single OK button
		/// </summary>
		/// <param name="manager"></param>
		/// <param name="title"></param>
		public static void ShowFragment( FragmentManager manager, string title )
		{
			NotificationDialogFragment dialog = new () { Arguments = new Bundle() };
			dialog.Arguments.PutString( "title", title );
			dialog.Show( manager, "fragment_notification_tag" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public NotificationDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState ) =>
			new AlertDialog.Builder( Activity )
				.SetTitle( Arguments.GetString( "title", "" ) )
				.SetPositiveButton( "OK", delegate { } )
				.Create();
	}
}
