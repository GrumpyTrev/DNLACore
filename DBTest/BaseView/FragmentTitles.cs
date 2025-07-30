using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;

namespace DBTest
{
	/// <summary>
	///  The FragmentTitles controls the initialisation of and changes to the tab titles used for the fragments
	/// </summary>
	internal static class FragmentTitles
	{
		/// <summary>
		/// Called to set the initial titles used for the fragments
		/// If this is not the first time the titles have been set then this is ignored
		/// </summary>
		/// <param name="title"></param>
		public static void SetInitialTitles( string[] titles, Fragment[] fragments )
		{
			if ( FragmentTitlesModel.InitialTitles == null )
			{
				FragmentTitlesModel.InitialTitles = titles;

				// Copy these values to the current titles as well
				FragmentTitlesModel.Titles = ( string[] )FragmentTitlesModel.InitialTitles.Clone();

				// A way is needed of associating a fragment with its tab position. Cannot use the actual fragment object itself
				// as it can be destroyed and recreated by the system. 
				// For now map the fragment class name to tab position. This only works if each fragment is only shown in a single tab
				for ( int fragmentIndex = 0; fragmentIndex < fragments.Length; fragmentIndex++ )
				{
					FragmentTitlesModel.FragmentLookup.Add( fragments[ fragmentIndex ].GetType(), fragmentIndex );
				}
			}
		}

		/// <summary>
		/// Get the current titles in a form that the Tab layout can use
		/// </summary>
		/// <returns></returns>
		public static Java.Lang.ICharSequence[] GetTitles() => CharSequence.ArrayFromStringArray( FragmentTitlesModel.Titles );

		/// <summary>
		/// Append a string to the initial fragment title
		/// </summary>
		/// <param name="append"></param>
		public static void AppendToTabTitle( string append, Fragment tabFragment )
		{
			// If the main activity hasn't been specified yet then the title cannot be set. 
			if ( ParentActivity != null )
			{
				// Lookup the type of the fragment
				if ( FragmentTitlesModel.FragmentLookup.TryGetValue( tabFragment.GetType(), out int position ) == true )
				{
					// Either reset the current title to the initial title, or use the initial title to form a new title
					FragmentTitlesModel.Titles[ position ] = ( append.Length == 0 ) ? FragmentTitlesModel.InitialTitles[ position ] :
						string.Format( "{0}{1}", FragmentTitlesModel.InitialTitles[ position ], append );

					_ = layout.GetTabAt( position ).SetText( FragmentTitlesModel.Titles[ position ] );
				}
			}
		}

		/// <summary>
		/// The Activity used to access the controls necessary to set the titles dynamically
		/// </summary>
		public static AppCompatActivity ParentActivity
		{
			private get => parentActivity;
			set
			{
				// If the activity is being set now save the tab layout control for later
				if ( value != null )
				{
					parentActivity = value;
					layout = parentActivity.FindViewById<TabLayout>( Resource.Id.sliding_tabs );
				}
			}
		}

		/// <summary>
		/// The Activity used to access the controls necessary to set the titles dynamically
		/// </summary>
		private static AppCompatActivity parentActivity = null;

		/// <summary>
		/// The TabLayout control holding the titles
		/// </summary>
		private static TabLayout layout = null;
	}

	/// <summary>
	/// The FragmentTitlesModel class holds process instance data for the FragmentTitles class
	/// </summary>
	internal static class FragmentTitlesModel
	{
		/// <summary>
		/// The original titles given to the fragment
		/// </summary>
		internal static string[] InitialTitles { get; set; } = null;

		/// <summary>
		/// The current fragment titles
		/// </summary>
		internal static string[] Titles { get; set; } = null;

		/// <summary>
		/// Lookup table to associate a fragment class type with its tab position
		/// </summary>
		internal static Dictionary<Type, int> FragmentLookup { get; } = [];
	}
}
