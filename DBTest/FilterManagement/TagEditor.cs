using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using System.Collections.Generic;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace DBTest
{
	/// <summary>
	/// The TagEditor class is used to handle tag edit commands
	/// </summary>
	class TagEditor : TagCommandHandler
	{
		/// <summary>
		/// Public constructor providing the context for the dialogues
		/// </summary>
		/// <param name="activityContext"></param>
		public TagEditor( AppCompatActivity activityContext ) : base( activityContext )
		{
		}

		/// <summary>
		/// Process the tag command
		/// Determine which tag is being edited and display the edit dialogue for that tag
		/// </summary>
		/// <param name="name"></param>
		protected override void ProcessTagCommand( string name )
		{
			TagEditorDialogFragment.NewInstance( "Edit tag details", name ).Show( Context.SupportFragmentManager, "fragment_edit_tag" );
		}
	}

	/// <summary>
	/// The TagCreator class is used to handle new tag creation commands
	/// </summary>
	class TagCreator
	{
		/// <summary>
		/// Allow the user to create a new tag
		/// </summary>
		/// <param name="activityContext"></param>
		public static void AddNewTag( AppCompatActivity activityContext )
		{
			TagEditorDialogFragment.NewInstance( "New tag details", "" ).Show( activityContext.SupportFragmentManager, "fragment_new_tag" );
		}
	}

	/// <summary>
	/// Tag editor dialogue based on DialogFragment to provide activity configuration support
	/// </summary>
	internal class TagEditorDialogFragment: DialogFragment
	{
		/// <summary>
		/// Create a TagEditorDialogFragment with the specified arguments
		/// </summary>
		/// <param name="title"></param>
		/// <param name="tagName"></param>
		/// <returns></returns>
		public static TagEditorDialogFragment NewInstance( string title, string tagName )
		{
			TagEditorDialogFragment dialog = new TagEditorDialogFragment() { Arguments = new Bundle() };
			dialog.Arguments.PutString( "title", title );
			dialog.Arguments.PutString( "tag", tagName );

			return dialog;
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
			editTag = FilterManagementController.GetTagFromName( Arguments.GetString( "tag", "" ) );

			// Create the custom view and get references to the editable fields
			View tagView = LayoutInflater.From( Context ).Inflate( Resource.Layout.tag_details_dialogue_layout, null );
			tagName = tagView.FindViewById<EditText>( Resource.Id.tagName );
			tagShortName = tagView.FindViewById<EditText>( Resource.Id.tagShortName );
			idSort = tagView.FindViewById<CheckBox>( Resource.Id.idSort );
			maxCount = tagView.FindViewById<EditText>( Resource.Id.maxCount );
			synchLibs = tagView.FindViewById<CheckBox>( Resource.Id.idSynchronise );

			// If editing an existing tag then set the dialogue fields to the current values from the tag
			if ( editTag != null )
			{
				tagName.Text = editTag.Name;
				tagShortName.Text = editTag.ShortName;
				idSort.Checked = editTag.TagOrder;
				maxCount.Text = editTag.MaxCount.ToString();
				synchLibs.Checked = editTag.Synchronise;
			}
			else
			{
				maxCount.Text = "-1";
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

			( ( AlertDialog ) Dialog).GetButton( ( int )DialogButtonType.Positive ).Click += ( sender, args ) => {

				Tag newOrUpdatedTag = new Tag() { Name = tagName.Text, ShortName = tagShortName.Text, MaxCount = int.Parse( maxCount.Text ),
					TagOrder = idSort.Checked, Synchronise = synchLibs.Checked, TaggedAlbums = new List<TaggedAlbum>() };

				// If nothing has changed then tell the user, otherwise carry out the save operation
				if ( editTag != null )
				{
					if ( ( editTag.Name != newOrUpdatedTag.Name ) || ( editTag.ShortName != newOrUpdatedTag.ShortName ) || 
						( editTag.TagOrder != newOrUpdatedTag.TagOrder ) || ( editTag.MaxCount != newOrUpdatedTag.MaxCount ) ||
						( editTag.Synchronise != newOrUpdatedTag.Synchronise ) )
					{
						// Something has changed so attempt to update the tag
						FilterManagementController.UpdateTagAsync( editTag, newOrUpdatedTag, TagUpdated );
					}
					else
					{
						// Nothing has changed
						SomeKindOfErrorDialogFragment.NewInstance( "No changes made to tag" ).Show( Activity.SupportFragmentManager, "fragment_error_tag" );
					}
				}
				else
				{
					// Attempt to add a new tag
					FilterManagementController.CreateTagAsync( newOrUpdatedTag, TagUpdated );
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
				Dialog.Dismiss();
			}
			else
			{
				// Must have been a problem, display the 'name already used' dialogue
				SomeKindOfErrorDialogFragment.NewInstance( "Tag with the same name already exists" ).Show( Activity.SupportFragmentManager, "fragment_error_tag" );
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
		EditText maxCount = null;
		CheckBox synchLibs = null;
	}

	/// <summary>
	/// Dialogue reporting some kind of problem with the requested action
	/// </summary>
	internal class SomeKindOfErrorDialogFragment: DialogFragment
	{
		/// <summary>
		/// Create a SomeKindOfErrorDialogFragment with the specified arguments
		/// </summary>
		/// <param name="title"></param>
		/// <param name="tagName"></param>
		/// <returns></returns>
		public static SomeKindOfErrorDialogFragment NewInstance( string title )
		{
			SomeKindOfErrorDialogFragment dialog = new SomeKindOfErrorDialogFragment { Arguments = new Bundle() };
			dialog.Arguments.PutString( "title", title );

			return dialog;
		}

		/// <summary>
		/// Empty constructor required for DialogFragment
		/// </summary>
		public SomeKindOfErrorDialogFragment()
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