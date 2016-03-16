using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MonoGame.Tools.Pipeline
{
	public class MultiSelectTreeview : TreeView
    {
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, int lParam);

        #region Selected Node(s) Properties

        private readonly List<TreeNode> _selectedNodes = null;

        private Color _dragOverNodeForeColor = SystemColors.HighlightText;
        private Color _dragOverNodeBackColor = SystemColors.Highlight;
        private TreeNode _previousNode;

        /// <summary>
        /// The baskground colour of the node being dragged over.
        /// </summary>
        public Color DragOverNodeBackColor
        {
            get
            {
                return this._dragOverNodeBackColor;
            }
            set
            {
                this._dragOverNodeBackColor = value;
            }
        }

        /// <summary>
        /// The foreground colour of the node being dragged over.
        /// </summary>
        public Color DragOverNodeForeColor
        {
            get
            {
                return this._dragOverNodeForeColor;
            }
            set
            {
                this._dragOverNodeForeColor = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IEnumerable<TreeNode> SelectedNodes
		{
			get
			{
				return _selectedNodes;
			}
			set
			{
				ClearSelectedNodes();
				if( value != null )
				{
					foreach( var node in value )
					{
						ToggleNode( node, true );
					}
				}
			}
		}

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	    public IEnumerable<TreeNode> SelectedNodesRecursive
	    {
            get
            {                
                var nodes = new List<TreeNode>();

                foreach (var node in _selectedNodes)
                {
                    if (!nodes.Contains(node))
                        nodes.Add(node);

                    var children = new List<TreeNode>();
                    TreeViewExtensions.AddTreeNodesRecursive(node.Nodes, children);

                    foreach (var child in children)
                    {
                        if (!nodes.Contains(child))
                            nodes.Add(child);
                    }
                }

                return nodes;
            }
	    }

		// Note we use the new keyword to Hide the native treeview's SelectedNode property.
		private TreeNode _selectedNode;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new TreeNode SelectedNode
		{
			get { return _selectedNode; }
			set
			{
				ClearSelectedNodes();
				if( value != null )
				{
					SelectNode( value );
				}
			}
		}

		#endregion

		public MultiSelectTreeview()
		{
			_selectedNodes = new List<TreeNode>();
			base.SelectedNode = null;
		}

		#region Overridden Events

		protected override void OnGotFocus( EventArgs e )
		{
			// Make sure at least one node has a selection
			// this way we can tab to the ctrl and use the 
			// keyboard to select nodes
			try
			{
				if( _selectedNode == null && this.TopNode != null )
				{
					ToggleNode( this.TopNode, true );
				}

				base.OnGotFocus( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			// If the user clicks on a node that was not
			// previously selected, select it now.

			try
			{
				base.SelectedNode = null;

				TreeNode node = this.GetNodeAt( e.Location );
				if( node != null )
				{
					int leftBound = node.Bounds.X; // - 20; // Allow user to click on image
					int rightBound = node.Bounds.Right + 10; // Give a little extra room
					if( e.Location.X > leftBound && e.Location.X < rightBound )
					{
						if( ModifierKeys == Keys.None && ( _selectedNodes.Contains( node ) ) )
						{
							// Potential Drag Operation
							// Let Mouse Up do select
						}
						else
						{							
							SelectNode( node );
						}
					}
				}

				base.OnMouseDown( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			// If the clicked on a node that WAS previously
			// selected then, reselect it now. This will clear
			// any other selected nodes. e.g. A B C D are selected
			// the user clicks on B, now A C & D are no longer selected.
            // JCF: Only do that for left clicks.
            //      Right clicks brings up a context menu which will apply even a multiselection.
			try
			{
			    if (e.Button.HasFlag(MouseButtons.Left))
			    {
			        // Check to see if a node was clicked on 
			        TreeNode node = this.GetNodeAt(e.Location);
			        if (node != null)
			        {
			            if (ModifierKeys == Keys.None && _selectedNodes.Contains(node))
			            {
			                int leftBound = node.Bounds.X; // -20; // Allow user to click on image
			                int rightBound = node.Bounds.Right + 10; // Give a little extra room
			                if (e.Location.X > leftBound && e.Location.X < rightBound)
			                {

			                    SelectNode(node);
			                }
			            }
			        }

                    base.OnMouseUp(e);
			    }			    
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            // Get the node from the mouse position, colour it
            Point pt = this.PointToClient(new Point(drgevent.X, drgevent.Y));
            TreeNode treeNode = this.GetNodeAt(pt);

            // Change node color
            if (this._previousNode != null && this._previousNode != treeNode)
            {
                this._previousNode.BackColor = SystemColors.HighlightText;
                this._previousNode.ForeColor = SystemColors.ControlText;
            }

            if (treeNode != null && treeNode.BackColor != this._dragOverNodeBackColor)
            {
                treeNode.BackColor = this._dragOverNodeBackColor;
                treeNode.ForeColor = this._dragOverNodeForeColor;
            }

            // Scrolling down/up
            if (pt.Y + 10 > this.ClientSize.Height)
                SendMessage(this.Handle, 277, (IntPtr)1, 0);
            else if (pt.Y < this.Top + 10)
                SendMessage(this.Handle, 277, (IntPtr)0, 0);

            // Remember the target node, so we can set it back
            this._previousNode = treeNode;

            base.OnDragOver(drgevent);
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            // Restore node color
            if (this._previousNode != null)
            {
                this._previousNode.BackColor = SystemColors.HighlightText;
                this._previousNode.ForeColor = SystemColors.ControlText;
                this._previousNode = null;
            }

            base.OnDragDrop(drgevent);
        }

        protected override void OnItemDrag( ItemDragEventArgs e )
		{
			// If the user drags a node and the node being dragged is NOT
			// selected, then clear the active selection, select the
			// node being dragged and drag it. Otherwise if the node being
			// dragged is selected, drag the entire selection.
			try
			{
				TreeNode node = e.Item as TreeNode;

				if( node != null )
				{
					if( !_selectedNodes.Contains( node ) )
					{
						SelectSingleNode( node );
						ToggleNode( node, true );
					}
				}

				base.OnItemDrag( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnBeforeSelect( TreeViewCancelEventArgs e )
		{
			// Never allow base.SelectedNode to be set!
			try
			{
				base.SelectedNode = null;
				e.Cancel = true;

				base.OnBeforeSelect( e );
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnAfterSelect( TreeViewEventArgs e )
		{
			// Never allow base.SelectedNode to be set!
			try
			{
				base.OnAfterSelect( e );
				base.SelectedNode = null;
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			// Handle all possible key strokes for the control.
			// including navigation, selection, etc.

			base.OnKeyDown( e );

			if( e.KeyCode == Keys.ShiftKey ) return;

			//this.BeginUpdate();
			bool bShift = ( ModifierKeys == Keys.Shift );

			try
			{
				// Nothing is selected in the tree, this isn't a good state
				// select the top node
				if( _selectedNode == null && this.TopNode != null )
				{
					ToggleNode( this.TopNode, true );
				}

				// Nothing is still selected in the tree, this isn't a good state, leave.
				if( _selectedNode == null ) return;

				if( e.KeyCode == Keys.Left )
				{
					if( _selectedNode.IsExpanded && _selectedNode.Nodes.Count > 0 )
					{
						// Collapse an expanded node that has children
						_selectedNode.Collapse();
					}
					else if( _selectedNode.Parent != null )
					{
						// Node is already collapsed, try to select its parent.
						SelectSingleNode( _selectedNode.Parent );
					}
				}
				else if( e.KeyCode == Keys.Right )
				{
					if( !_selectedNode.IsExpanded )
					{
						// Expand a collpased node's children
						_selectedNode.Expand();
					}
					else
					{
						// Node was already expanded, select the first child
						SelectSingleNode( _selectedNode.FirstNode );
					}
				}
				else if( e.KeyCode == Keys.Up )
				{
					// Select the previous node
					if( _selectedNode.PrevVisibleNode != null )
					{
						SelectNode( _selectedNode.PrevVisibleNode );
					}
				}
				else if( e.KeyCode == Keys.Down )
				{
					// Select the next node
					if( _selectedNode.NextVisibleNode != null )
					{
						SelectNode( _selectedNode.NextVisibleNode );
					}
				}
				else if( e.KeyCode == Keys.Home )
				{
					if( bShift )
					{
						if( _selectedNode.Parent == null )
						{
							// Select all of the root nodes up to this point 
							if( this.Nodes.Count > 0 )
							{
								SelectNode( this.Nodes[0] );
							}
						}
						else
						{
							// Select all of the nodes up to this point under this nodes parent
							SelectNode( _selectedNode.Parent.FirstNode );
						}
					}
					else
					{
						// Select this first node in the tree
						if( this.Nodes.Count > 0 )
						{
							SelectSingleNode( this.Nodes[0] );
						}
					}
				}
				else if( e.KeyCode == Keys.End )
				{
					if( bShift )
					{
						if( _selectedNode.Parent == null )
						{
							// Select the last ROOT node in the tree
							if( this.Nodes.Count > 0 )
							{
								SelectNode( this.Nodes[this.Nodes.Count - 1] );
							}
						}
						else
						{
							// Select the last node in this branch
							SelectNode( _selectedNode.Parent.LastNode );
						}
					}
					else
					{
						if( this.Nodes.Count > 0 )
						{
							// Select the last node visible node in the tree.
							// Don't expand branches incase the tree is virtual
							TreeNode ndLast = this.Nodes[0].LastNode;
							while( ndLast.IsExpanded && ( ndLast.LastNode != null ) )
							{
								ndLast = ndLast.LastNode;
							}
							SelectSingleNode( ndLast );
						}
					}
				}
				else if( e.KeyCode == Keys.PageUp )
				{
					// Select the highest node in the display
					int nCount = this.VisibleCount;
					TreeNode ndCurrent = _selectedNode;
					while( ( nCount ) > 0 && ( ndCurrent.PrevVisibleNode != null ) )
					{
						ndCurrent = ndCurrent.PrevVisibleNode;
						nCount--;
					}
					SelectSingleNode( ndCurrent );
				}
				else if( e.KeyCode == Keys.PageDown )
				{
					// Select the lowest node in the display
					int nCount = this.VisibleCount;
					TreeNode ndCurrent = _selectedNode;
					while( ( nCount ) > 0 && ( ndCurrent.NextVisibleNode != null ) )
					{
						ndCurrent = ndCurrent.NextVisibleNode;
						nCount--;
					}
					SelectSingleNode( ndCurrent );
				}
				else
				{
					// Assume this is a search character a-z, A-Z, 0-9, etc.
					// Select the first node after the current node that 
					// starts with this character
					string sSearch = ( (char) e.KeyValue ).ToString();

					TreeNode ndCurrent = _selectedNode;
					while( ( ndCurrent.NextVisibleNode != null ) )
					{
						ndCurrent = ndCurrent.NextVisibleNode;
						if( ndCurrent.Text.StartsWith( sSearch ) )
						{
							SelectSingleNode( ndCurrent );
							break;
						}
					}
				}
			}
			catch( Exception ex )
			{
				HandleException( ex );
			}
			finally
			{
				this.EndUpdate();
			}
		}

		#endregion

		#region Helper Methods

		private void SelectNode( TreeNode node )
		{
			try
			{
				this.BeginUpdate();

				if( _selectedNode == null || ModifierKeys == Keys.Control )
				{
					// Ctrl+Click selects an unselected node, or unselects a selected node.
					bool bIsSelected = _selectedNodes.Contains( node );
					ToggleNode( node, !bIsSelected );
				}
				else if( ModifierKeys == Keys.Shift )
				{
					// Shift+Click selects nodes between the selected node and here.
					TreeNode ndStart = _selectedNode;
					TreeNode ndEnd = node;

					if( ndStart.Parent == ndEnd.Parent )
					{
						// Selected node and clicked node have same parent, easy case.
						if( ndStart.Index < ndEnd.Index )
						{							
							// If the selected node is beneath the clicked node walk down
							// selecting each Visible node until we reach the end.
							while( ndStart != ndEnd )
							{
								ndStart = ndStart.NextVisibleNode;
								if( ndStart == null ) break;
								ToggleNode( ndStart, true );
							}
						}
						else if( ndStart.Index == ndEnd.Index )
						{
							// Clicked same node, do nothing
						}
						else
						{
							// If the selected node is above the clicked node walk up
							// selecting each Visible node until we reach the end.
							while( ndStart != ndEnd )
							{
								ndStart = ndStart.PrevVisibleNode;
								if( ndStart == null ) break;
								ToggleNode( ndStart, true );
							}
						}
					}
					else
					{
						// Selected node and clicked node have same parent, hard case.
						// We need to find a common parent to determine if we need
						// to walk down selecting, or walk up selecting.

						TreeNode ndStartP = ndStart;
						TreeNode ndEndP = ndEnd;
						int startDepth = Math.Min( ndStartP.Level, ndEndP.Level );

						// Bring lower node up to common depth
						while( ndStartP.Level > startDepth )
						{
							ndStartP = ndStartP.Parent;
						}

						// Bring lower node up to common depth
						while( ndEndP.Level > startDepth )
						{
							ndEndP = ndEndP.Parent;
						}

						// Walk up the tree until we find the common parent
						while( ndStartP.Parent != ndEndP.Parent )
						{
							ndStartP = ndStartP.Parent;
							ndEndP = ndEndP.Parent;
						}

						// Select the node
						if( ndStartP.Index < ndEndP.Index )
						{
							// If the selected node is beneath the clicked node walk down
							// selecting each Visible node until we reach the end.
							while( ndStart != ndEnd )
							{
								ndStart = ndStart.NextVisibleNode;
								if( ndStart == null ) break;
								ToggleNode( ndStart, true );
							}
						}
						else if( ndStartP.Index == ndEndP.Index )
						{
							if( ndStart.Level < ndEnd.Level )
							{
								while( ndStart != ndEnd )
								{
									ndStart = ndStart.NextVisibleNode;
									if( ndStart == null ) break;
									ToggleNode( ndStart, true );
								}
							}
							else
							{
								while( ndStart != ndEnd )
								{
									ndStart = ndStart.PrevVisibleNode;
									if( ndStart == null ) break;
									ToggleNode( ndStart, true );
								}
							}
						}
						else
						{
							// If the selected node is above the clicked node walk up
							// selecting each Visible node until we reach the end.
							while( ndStart != ndEnd )
							{
								ndStart = ndStart.PrevVisibleNode;
								if( ndStart == null ) break;
								ToggleNode( ndStart, true );
							}
						}
					}
				}
				else
				{
					// Just clicked a node, select it
					SelectSingleNode( node );
				}

				OnAfterSelect( new TreeViewEventArgs( _selectedNode ) );
			}
			finally
			{
				this.EndUpdate();
			}
		}

		private void ClearSelectedNodes()
		{
			try
			{
				foreach( TreeNode node in _selectedNodes )
				{
					node.BackColor = this.BackColor;
					node.ForeColor = this.ForeColor;
				}
			}
			finally
			{
				_selectedNodes.Clear();
				_selectedNode = null;
			}
		}

		private void SelectSingleNode( TreeNode node )
		{
			if( node == null )
			{
				return;
			}

			ClearSelectedNodes();
			ToggleNode( node, true );
			node.EnsureVisible();

			OnAfterSelect( new TreeViewEventArgs( _selectedNode ) );
        }

		private void ToggleNode( TreeNode node, bool bSelectNode )
		{
			if( bSelectNode )
			{
				_selectedNode = node;
				if( !_selectedNodes.Contains( node ) )
				{
					_selectedNodes.Add( node );
				}
				node.BackColor = SystemColors.Highlight;
				node.ForeColor = SystemColors.HighlightText;
			}
			else
			{
				_selectedNodes.Remove( node );
				node.BackColor = this.BackColor;
				node.ForeColor = this.ForeColor;
			}
		}

		private void HandleException( Exception ex )
		{
			// Perform some error handling here.
			// We don't want to bubble errors to the CLR. 
			MessageBox.Show( ex.Message );
		}

		#endregion
	}
}
