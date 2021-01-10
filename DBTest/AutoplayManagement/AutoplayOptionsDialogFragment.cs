using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// The AutoplayOptionsDialogFragment displays the current Autoplay options and allows the use to change those options
	/// </summary>
	internal class AutoplayOptionsDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, OptionsChanged selectionCallback )
		{
			// Save the parameters so that they are available after a configuration change
			reporter = selectionCallback;

			new AutoplayOptionsDialogFragment().Show( manager, "fragment_autoplay_options" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public AutoplayOptionsDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Create the custom view and initialise to the current Autoplay record
			View dialogView = LayoutInflater.From( Context ).Inflate( Resource.Layout.autoplay_options_dialogue_layout, null );
			RadioGroup spreadGroup = dialogView.FindViewById<RadioGroup>( Resource.Id.spreadGroup );
			RadioGroup targetGroup = dialogView.FindViewById<RadioGroup>( Resource.Id.targetGroup );
			RadioGroup weightGroup = dialogView.FindViewById<RadioGroup>( Resource.Id.weightGroup );
			Spinner fastSpreadSpinner = dialogView.FindViewById<Spinner>( Resource.Id.fastSpreadLimit );
			LinearLayout fastSpreadLimitLayout = dialogView.FindViewById<LinearLayout>( Resource.Id.fastSpreadLimitLayout );

			ArrayAdapter<int> adapter = new ArrayAdapter<int>( Context, Resource.Layout.select_dialog_item_material, new int[] { 0, 1, 2, 3, 4, 5 } );
			fastSpreadSpinner.Adapter = adapter;

			// Only initialise if we are not restoring
			if ( savedInstanceState == null )
			{
				spreadGroup.Check( spreadGroup.GetChildAt( ( int )AutoplayModel.CurrentAutoplay.Spread ).Id );
				targetGroup.Check( targetGroup.GetChildAt( ( int )AutoplayModel.CurrentAutoplay.Target ).Id );
				weightGroup.Check( weightGroup.GetChildAt( ( int )AutoplayModel.CurrentAutoplay.Weight ).Id );
				fastSpreadSpinner.SetSelection( AutoplayModel.CurrentAutoplay.FastSpreadLimit );
			}

			// Show or hide the spinner (and text)
			fastSpreadLimitLayout.Visibility = ( AutoplayModel.CurrentAutoplay.Spread == Autoplay.SpreadType.Fast ) ? ViewStates.Visible : ViewStates.Gone;

			// We need to know when Fast Spread has been selected so that the spinner can be show or hidden as appropriate
			spreadGroup.CheckedChange += ( sender, args ) =>
			{
				fastSpreadLimitLayout.Visibility = ( ( Autoplay.SpreadType )GetIndexOfSelectedChild( spreadGroup ) == Autoplay.SpreadType.Fast ) 
					? ViewStates.Visible : ViewStates.Gone;
			};

			// Set up the handlers for the buttons
			// This layout contains its own buttons so that their order and position can be controlled
			dialogView.FindViewById<Button>( Resource.Id.auto_cancel ).Click += ( sender, args ) =>	{ Dismiss(); };

			// Report back the new Autoplay record for the Play and Queue buttons
			dialogView.FindViewById<Button>( Resource.Id.auto_play ).Click += ( sender, args ) =>
			{
				reporter.Invoke( CreateNewAutoplay( spreadGroup, targetGroup, weightGroup, fastSpreadSpinner.SelectedItemPosition ), true );
				Dismiss();
			};

			dialogView.FindViewById<Button>( Resource.Id.auto_queue ).Click += ( sender, args ) =>
			{
				reporter.Invoke( CreateNewAutoplay( spreadGroup, targetGroup, weightGroup, fastSpreadSpinner.SelectedItemPosition ), false );
				Dismiss();
			};

			// Create the AlertDialog with the custom view and none of its own buttons
			return new AlertDialog.Builder( Activity )
			.SetTitle( "Autoplay options" )
			.SetView( dialogView )
			.Create();
		}

		/// <summary>
		/// Create a new Autoplay record from the selected radio buttons of the specified radio groups
		/// </summary>
		/// <param name="spread"></param>
		/// <param name="target"></param>
		/// <param name="weight"></param>
		/// <returns></returns>
		private Autoplay CreateNewAutoplay( RadioGroup spread, RadioGroup target, RadioGroup weight, int fastSpreadLimit ) => new Autoplay()
		{
			Spread = ( Autoplay.SpreadType )GetIndexOfSelectedChild( spread ),
			Target = ( Autoplay.TargetType )GetIndexOfSelectedChild( target ),
			Weight = ( Autoplay.WeightType )GetIndexOfSelectedChild( weight ),
			FastSpreadLimit = fastSpreadLimit
		};

		/// <summary>
		/// Get the index of the selected RadioButton in a RadioGroup
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		private int GetIndexOfSelectedChild( RadioGroup group ) => group.IndexOfChild( group.FindViewById<RadioButton>( group.CheckedRadioButtonId ) );

		/// <summary>
		/// The delegate used to report back opton changes
		/// </summary>
		private static OptionsChanged reporter = null;

		/// <summary>
		/// Delegate type used to report back the updated Autoplay options and the selected Autoplay action
		/// </summary>
		public delegate void OptionsChanged( Autoplay newOptions, bool playNow );
	}
}