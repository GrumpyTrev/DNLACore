using System;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using static Android.Views.View;
using static Android.Widget.ExpandableListView;

namespace DBTest
{
	/// <summary>
	/// The DragHelper class provides common drag functionality to ExpandableListAdapter derived classes
	/// </summary>
	internal class DragHelper 
	{
		/// <summary>
		/// Create a DragHelper for the specified ExpandableListView
		/// </summary>
		/// <param name="parentView"></param>
		public DragHelper( ExpandableListView parentView, View fragmentView, IAdapterInterface adapter )
		{
			// Save the parent ExpandableListView
			listView = parentView;

			// Save the adapter and bind to it
			adapterInterface = adapter;
			adapterInterface.BindDragHelper( this );

			// Get a reference to the image view that's going to be used for the Drag Shadow
			dragView = fragmentView.FindViewById<ImageView>( Resource.Id.dragView );
			dragView.Visibility = ViewStates.Invisible;

			// Catch any drag and drop, and scroll events sent to the parent list view
			parentView.Drag += ListViewDrag;
			parentView.ScrollStateChanged += ScrollStateChanged;
		}

		/// <summary>
		/// Called when an item's drag handler has been touched.
		/// Start a drag and drop operation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void DragHandleTouch( object sender, View.TouchEventArgs e )
		{
			// Don't start dragging if in Action Mode
			if ( adapterInterface.ActionModeInEffect == false )
			{
				if ( e.Event.Action == MotionEventActions.Down )
				{
					// Get the item view from the drag handle
					View itemView = ( View )( ( ImageView )sender ).Tag;

					// Save the Y coordinate of where the song item was touched
					touchOffset = ( int )e.Event.GetY() + ListItemMarginAdjustment;

					// Save the position of the top of the view being dragged
					dragViewY = ( int )itemView.GetY();

					// Draw the contents of the view about to be dragged onto a bitmap
					Bitmap dragImage = Bitmap.CreateBitmap( itemView.Width, itemView.Height, Bitmap.Config.Argb8888 );
					itemView.Draw( new Canvas( dragImage ) );

					// Copy the image to the actual view that's going to be dragged
					dragView.SetImageBitmap( dragImage );

					// Start the drag
					if ( itemView.StartDragAndDrop( null, new CustomDragShadowBuilder( itemView ), null, 0 ) == true )
					{
						// Hide the original view to indicate that it is being dragged
						itemView.Visibility = ViewStates.Invisible;
						itemBeingDragged = ( ( IDragHolder )itemView.Tag ).ItemPosition;
					}

					// Let the adapter know that the user has interacted with the view
					adapterInterface.UserInteraction();

					// Force the limits to be requested next time any drag operation is carried out
					limitsSet = false;
				}
			}
		}

		/// <summary>
		/// Hide or show the specified view depending on whether or not it is being dragged
		/// Update the stored position of the view in case it has changed
		/// </summary>
		/// <param name="viewToCheck"></param>
		public void HideViewIfBeingDragged( View viewToCheck )
		{
			if ( viewToCheck.Tag is IDragHolder dragHolder )
			{
				if ( itemBeingDragged == dragHolder.ItemPosition )
				{
					viewToCheck.Visibility = ViewStates.Invisible;
					dragViewY = ( int )viewToCheck.GetY();
				}
				else
				{
					viewToCheck.Visibility = ViewStates.Visible;
				}
			}
		}

		/// <summary>
		/// Called when the scroll state of the parent view has changed
		/// If the scroll state has gone to idle whilst an item is being dragged, then this is the end of
		/// an automatic scroll. Carry out the movement of the drag item
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ScrollStateChanged( object sender, AbsListView.ScrollStateChangedEventArgs e )
		{
			scrollState = e.ScrollState;

			if ( ( scrollState == ScrollState.Idle ) && ( itemBeingDragged != PackedPositionValueNull ) )
			{
				MoveItem( false );
			}
		}

		/// <summary>
		/// Called during a drag and drop operation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ListViewDrag( object sender, DragEventArgs e )
		{
			// Only process drag events if this DragHelper is actively dragging
			if ( itemBeingDragged != PackedPositionValueNull )
			{
				// Make sure that the drag limits have been obtained
				if ( limitsSet == false )
				{
					adapterInterface.GetLimits( out minDragPosition, out maxDragPosition, out minItemPosition, out maxItemPosition, itemBeingDragged );
					limitsSet = true;
				}

				DragEvent dragEvent = e.Event;
				switch ( dragEvent.Action )
				{
					case DragAction.Ended:
					{
						itemBeingDragged = PackedPositionValueNull;
						lastDragY = -1;
						movementSinceLastReorder = 0;

						// Hide the drag view and force a redraw
						dragView.Visibility = ViewStates.Invisible;
						adapterInterface.RedrawRequired();
						break;
					}

					case DragAction.Entered:
					{
						break;
					}

					case DragAction.Exited:
					{
						break;
					}

					case DragAction.Location:
					{
						// Adjust the location of the drag image, but don't go out of the list view bounds
						dragView.SetY( Math.Min( Math.Max( minDragPosition, dragEvent.GetY() - touchOffset ), maxDragPosition - dragView.Height ) );

						// If auto scrolling is in progress then ignore any location change
						if ( scrollState == ScrollState.Idle )
						{
							// If moving up and the item is not already at the top of its allowed range, and the item at the top of it's
							// range is not being displayed, and the current drag position is within the top 10% of the parent view then
							// scroll down by one item
							if ( ( movementSinceLastReorder < 0 ) && ( itemBeingDragged > minItemPosition ) &&
								 ( listView.FirstVisiblePosition > listView.GetFlatListPosition( minItemPosition ) ) &&
								 ( dragView.GetY() < listView.Height / 10 ) )
							{
								scrollState = ScrollState.TouchScroll;
								_ = listView.Post( () => listView.SmoothScrollByOffset( -1 ) );
							}
							// If moving down and the item is not already at the bottom of its allowed rang, and the item at the bottom
							// of its range is not being displayed and the current drag position is within the bottom 10% of the parent
							// view then scroll down by one item
							else if ( ( movementSinceLastReorder > 0 ) && ( itemBeingDragged < maxItemPosition ) &&
								( listView.LastVisiblePosition < listView.GetFlatListPosition( maxItemPosition ) ) &&
								( dragView.GetY() > ( listView.Height * 9 ) / 10 ) )
							{
								scrollState = ScrollState.TouchScroll;
								_ = listView.Post( () => listView.SmoothScrollByOffset( 1 ) );
							}
							// Has the location changed
							else if ( lastDragY != dragView.GetY() )
							{
								int amountMoved = ( int )( dragView.GetY() - lastDragY );
								lastDragY = dragView.GetY();

								// If going in same direction then add to movementSinceLastReorder
								if ( Math.Sign( amountMoved ) == Math.Sign( movementSinceLastReorder ) )
								{
									movementSinceLastReorder += amountMoved;
								}
								else
								{
									movementSinceLastReorder = amountMoved;
								}

								// Has the view been dragged far enough to consider a reorder
								if ( Math.Abs( movementSinceLastReorder ) > MovementBeforeReorder )
								{
									// Which direction to check
									if ( movementSinceLastReorder < 0 )
									{
										// Going up. Nothing to do if already at the top.
										if ( itemBeingDragged > minItemPosition )
										{
											if ( dragView.GetY() < ( dragViewY - ( dragView.Height / 2 ) ) )
											{
												MoveItem();
											}
										}
									}
									else
									{
										// Going down. Nothing to do if already at the bottom
										if ( itemBeingDragged < maxItemPosition )
										{
											if ( dragView.GetY() > ( dragViewY + ( dragView.Height / 2 ) ) )
											{
												MoveItem();
											}
										}
									}
								}
							}
						}

						e.Handled = true;
						break;
					}

					case DragAction.Started:
					{
						// Position the DragView over the item selected
						dragView.SetX( 0 );
						dragView.SetY( dragEvent.GetY() - touchOffset );

						// Show it
						dragView.Visibility = ViewStates.Visible;

						e.Handled = true;
						break;
					}

					case DragAction.Drop:
					{
						break;
					}

					default:
					{
						break;
					}
				}

				// Let the adapter know that the user has interacted with the view
				adapterInterface.UserInteraction();
			}
		}

		/// <summary>
		/// Move the item being dragged either up or down
		/// </summary>
		private void MoveItem( bool resetMovement = true )
		{
			long itemToMove = itemBeingDragged;

			if ( movementSinceLastReorder < 0 )
			{
				AdjustDraggedItemPositionBy( -1 );
				adapterInterface.MoveItem( itemToMove, true );
			}
			else
			{
				AdjustDraggedItemPositionBy( 1 );
				adapterInterface.MoveItem( itemToMove, false );
			}

			if ( resetMovement == true )
			{
				movementSinceLastReorder = 0;
			}

			// Force the limits to be requested next time any drag operation is carried out
			limitsSet = false;
		}
		
		private void AdjustDraggedItemPositionBy( int amount )
		{
			if ( GetPackedPositionType( itemBeingDragged ) == PackedPositionType.Group )
			{
				itemBeingDragged = GetPackedPositionForGroup( GetPackedPositionGroup( itemBeingDragged ) + amount );
			}
			else if ( GetPackedPositionType( itemBeingDragged ) == PackedPositionType.Child )
			{
				itemBeingDragged = GetPackedPositionForChild( GetPackedPositionGroup( itemBeingDragged ),
					GetPackedPositionChild( itemBeingDragged ) + amount );
			}
		}

		/// <summary>
		/// The interface back to the Adapter to report significant events
		/// </summary>
		public interface IAdapterInterface
		{
			public bool ActionModeInEffect { get; }

			public void BindDragHelper( DragHelper helper );

			public void UserInteraction();

			public void RedrawRequired();

			public void MoveItem( long itemIndex, bool moveUp );

			public void GetLimits( out int minDrag, out int maxDrag, out long minPosition, out long maxPosition, long dragItemPosition );
		}

		public interface IDragHolder
		{
			public ImageView DragHandle { get; set; }

			public long ItemPosition { get; set; }
		}

		/// <summary>
		/// Packed position of the item being dragged
		/// </summary>
		private long itemBeingDragged = PackedPositionValueNull;

		/// <summary>
		/// The last reported drag position, used to detect actual movement
		/// </summary>
		private float lastDragY = -1;

		/// <summary>
		/// How far the view has to be dragged before re-ordering can be considered
		/// </summary>
		private const int MovementBeforeReorder = 10;

		/// <summary>
		/// How far has the view been moved since the last re-order or change of direction
		/// </summary>
		private int movementSinceLastReorder = 0;

		/// <summary>
		/// The scroll state of the parent list view
		/// </summary>
		private ScrollState scrollState = ScrollState.Idle;

		/// <summary>
		/// The offset within the view being dragged where the first touch occurred
		/// </summary>
		private int touchOffset;

		/// <summary>
		/// The position of the top of the view being dragged
		/// </summary>
		private int dragViewY = 0;

		/// <summary>
		/// Limits to be applied to the drag image and the index of the item being dragged
		/// </summary>
		private int minDragPosition;
		private int maxDragPosition;
		private long minItemPosition;
		private long maxItemPosition;

		/// <summary>
		/// Have the limits been obtained yet
		/// </summary>
		private bool limitsSet = false;

		/// <summary>
		/// When positioning the drag view, take account of the padding/margin
		/// </summary>
		private const int ListItemMarginAdjustment = 10;

		/// <summary>
		/// The view showing the image of the item being dragged
		/// </summary>
		private readonly ImageView dragView = null;

		/// <summary>
		/// The parent list view
		/// </summary>
		private readonly ExpandableListView listView = null;

		/// <summary>
		/// The adapter used to display the data in the list view
		/// </summary>
		private readonly IAdapterInterface adapterInterface = null;

		/// <summary>
		/// The standard DragShadowBuilder is specialised in order to prevent it being drawn
		/// </summary>
		/// <param name="view"></param>
		private class CustomDragShadowBuilder( View view ) : DragShadowBuilder( view )
		{
			/// <summary>
			/// Don't draw the shadow view.
			/// </summary>
			/// <param name="canvas"></param>
			public override void OnDrawShadow( Canvas canvas ) { }
		}
	}
}
