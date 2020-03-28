using Android.Support.V4.App;
using Java.Lang;

namespace DBTest
{
	/// <summary>
	/// The TabsFragmentPagerAdapter class subclasses the FragmentPagerAdapter in order to allow the fragments and title to be set externally
	/// </summary>
	class TabsFragmentPagerAdapter: FragmentPagerAdapter
	{
		/// <summary>
		/// Initialise the TabsFragmentPagerAdapter with the specified fragemnts and titles
		/// </summary>
		/// <param name="fm"></param>
		/// <param name="fragments"></param>
		/// <param name="titles"></param>
		public TabsFragmentPagerAdapter( FragmentManager fm, Fragment[] activityFragments, ICharSequence[] fragmentTitles ) : base( fm )
		{
			// Use the provided Fragments initially but replace with any already in the manager
			fragments = activityFragments;

			if ( fm.Fragments.Count > 0 )
			{
				// Copy the manager fragments but only as many as the provided array can contain.
				// Sometime the manager contains the tab page fragments as well as restored active dialog fragments.
				// The first set in the manager are (assumed to be) the tab page fragments. 
				int copyLimit = Math.Min( fm.Fragments.Count, fragments.Length );
				for ( int index = 0; index < copyLimit; ++index )
				{
					fragments[ index ] = fm.Fragments[ index ];
				}
			}

			titles = fragmentTitles;
		}

		/// <summary>
		/// Return the number of fragments in the adapter
		/// </summary>
		public override int Count
		{
			get
			{
				return fragments.Length;
			}
		}

		/// <summary>
		/// Get the fragement at the specified position
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public override Fragment GetItem( int position )
		{
			return fragments[ position ];
		}

		/// <summary>
		/// Get the title from the specified position
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public override ICharSequence GetPageTitleFormatted( int position )
		{
			return titles[ position ];
		}

		/// <summary>
		/// The fragments collection
		/// </summary>
		private readonly Fragment[] fragments;

		// The titles collection
		private readonly ICharSequence[] titles;
	}
}