using Android.Support.V4.App;
using Java.Lang;

namespace DBTest
{
	class TabsFragmentPagerAdapter: FragmentPagerAdapter
	{
		private readonly Fragment[] fragments;

		private readonly ICharSequence[] titles;

		public TabsFragmentPagerAdapter( FragmentManager fm, Fragment[] fragments, ICharSequence[] titles ) : base( fm )
		{
			// Use the provided Fragments initially but replace with any already in the manager
			this.fragments = fragments;

			if ( fm.Fragments.Count > 0 )
			{
				fm.Fragments.CopyTo( this.fragments, 0 );
			}

			this.titles = titles;
		}

		public override int Count
		{
			get
			{
				return fragments.Length;
			}
		}

		public override Fragment GetItem( int position )
		{
			return fragments[ position ];
		}

		public override ICharSequence GetPageTitleFormatted( int position )
		{
			return titles[ position ];
		}
	}
}