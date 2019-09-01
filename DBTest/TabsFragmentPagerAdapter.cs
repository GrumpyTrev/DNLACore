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
			// If the FragmentManger is already populated then use its Fragments instead
			if ( fm.Fragments.Count > 0 )
			{
				this.fragments = new Fragment[ fm.Fragments.Count ];
				fm.Fragments.CopyTo( this.fragments, 0 );
			}
			else
			{
				this.fragments = fragments;
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