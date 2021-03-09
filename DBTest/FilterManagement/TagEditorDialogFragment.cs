using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace DBTest
{
	/// <summary>
	/// Tag editor dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class TagEditorDialogFragment : DialogFragment
	{
		/// <summary>
		/// Show the dialogue displaying the scan progress and start the scan
		/// </summary>
		/// <param name="manager"></param>
		public static void ShowFragment( FragmentManager manager, string title, string tagName )
		{
			TagEditorDialogFragment dialog = new TagEditorDialogFragment() { Arguments = new Bundle() };
			dialog.Arguments.PutString( "title", title );
			dialog.Arguments.PutString( "tag", tagName );

			dialog.Show( manager, "fragment_edit_tag" );
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public TagEditorDialogFragment()
		{
		}

		/// <summary>
		/// Create the dialogue	
		/// </summary>
		/// <param name="savedInstanceState"></param>
		/// <returns></returns>
		public override Dialog OnCreateDialog( Bundle savedInstanceState )
		{
			// Lookup this tag in the list of tags held by the controller
			editTag = Tags.GetTagByName( Arguments.GetString( "tag", "" ) );

			// Create the custom view and get references to the editable fields
			View tagView = LayoutInflater.From( Context ).Inflate( Resource.Layout.tag_details_dialogue_layout, null );
			tagName = tagView.FindViewById<EditText>( Resource.Id.tagName );
			tagShortName = tagView.FindViewById<EditText>( Resource.Id.tagShortName );
			idSort = tagView.FindViewById<CheckBox>( Resource.Id.idSort );
			synchLibs = tagView.FindViewById<CheckBox>( Resource.Id.idSynchronise );

			// If editing an existing tag then set the dialogue fields to the current values from the tag
			if ( editTag != null )
			{
				tagName.Text = editTag.Name;
				tagShortName.Text = editTag.ShortName;
				idSort.Checked = editTag.TagOrder;
				synchLibs.Checked = editTag.Synchronise;
			}

			// Focus the tag namne field and display the input keyboard
			tagName.RequestFocus();

			// Display the keyboard after the view has got focus - 200ms delay here
			tagName.PostDelayed( () =>
			{
				( ( InputMethodManager )Context.GetSystemService( Android.Content.Context.InputMethodService ) ).ShowSoftInput( tagName, ShowFlags.Implicit );
			}, 200 );

			// Create the AlertDialog with no Save handler (and no dismiss on Save)
			AlertDialog alert = new AlertDialog.Builder( Activity )
				.SetTitle( Arguments.GetString( "title", "" ) )
				.SetView( tagView )
				.SetPositiveButton( "Save", ( EventHandler<DialogClickEventArgs> )null )
				.SetNegativeButton( "Cancel", delegate { } )
				.Create();

			return alert;
		}

		/// <summary>
		/// Install a handler for the Save button
		/// </summary>
		public override void OnResume()
		{
			base.OnResume();

			( ( AlertDialog )Dialog ).GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) => {

				Tag newOrUpdatedTag = new Tag()
				{
					Name = tagName.Text,
					ShortName = tagShortName.Text,
					TagOrder = idSort.Checked,
					Synchronise = synchLibs.Checked,
				};

				// Make sure that the Name is not empty
				if ( newOrUpdatedTag.Name.Length == 0 )
				{
					NotificationDialogFragment.ShowFragment( Activity.SupportFragmentManager, "An empty Tag name is not valid" );
				}
				else
				{
					// Normalise the short name
					if ( newOrUpdatedTag.ShortName.Length == 0 )
					{
						newOrUpdatedTag.ShortName = newOrUpdatedTag.Name;
					}

					// If nothing has changed then tell the user, otherwise carry out the save operation
					if ( editTag != null )
					{
						if ( ( editTag.Name != newOrUpdatedTag.Name ) || ( editTag.ShortName != newOrUpdatedTag.ShortName ) ||
							( editTag.TagOrder != newOrUpdatedTag.TagOrder ) ||	( editTag.Synchronise != newOrUpdatedTag.Synchronise ) )
						{
							// Something has changed so attempt to update the tag
							FilterManagementController.UpdateTag( editTag, newOrUpdatedTag, TagUpdated );
						}
						else
						{
							// Nothing has changed
							NotificationDialogFragment.ShowFragment( Activity.SupportFragmentManager, "No changes made to tag" );
						}
					}
					else
					{
						// Attempt to add a new tag
						FilterManagementController.CreateTagAsync( newOrUpdatedTag, TagUpdated );
					}
				}
			};
		}

		/// <summary>
		/// Called when the result of the tag edit or addition is known
		/// </summary>
		/// <param name="updateOk"></param>
		private void TagUpdated( bool updateOk )
		{
			if ( updateOk == true )
			{
				// Update was OK, dismiss this dialogue
				Dismiss();
			}
			else
			{
				// Must have been a problem, display the 'name already used' dialogue
				NotificationDialogFragment.ShowFragment( Activity.SupportFragmentManager, "Tag with the same name already exists" );
			}
		}

		/// <summary>
		/// The tag beings edited, or null if a new tag is being added
		/// </summary>
		private Tag editTag = null;

		/// <summary>
		/// The dialogue fields updated by the user. These must be available outside of the OnCreateDialog method
		/// </summary>
		EditText tagName = null;
		EditText tagShortName = null;
		CheckBox idSort = null;
		CheckBox synchLibs = null;
	}
}