// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class PropertyGridTable
    {
        private const int _spacing = 12;
        private const int _separatorWidth = 8;
        private const int _separatorSafeDistance = 30;

        public bool Group { get; set; }

        private IEnumerable<Type> _cellTypes;
        private CursorType _currentCursor;
        private CellBase _selectedCell;
        private List<CellBase> _cells;
        private Point _mouseLocation;
        private int _separatorPos, _moveSeparatorAmount;
        private bool _moveSeparator;
        private int _height;
        private bool _skipEdit;
        private Cursor _cursorNormal, _cursorResize;

        public PropertyGridTable()
        {
            InitializeComponent();

            _cellTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.IsSubclassOf(typeof(CellBase)));
            _separatorPos = 100;
            _mouseLocation = new Point(-1, -1);
            _cells = new List<CellBase>();
            _moveSeparator = false;
            _skipEdit = false;
            _cursorResize = new Cursor(CursorType.VerticalSplit);
            _cursorNormal = new Cursor(CursorType.Arrow);

            Group = true;
        }

        public void Clear()
        {
            _cells.Clear();
            ClearChildren();
        }

        private bool ClearChildren()
        {
            var children = pixel1.Children.ToList();
            var ret = children.Count > 1;

            foreach (var control in children)
            {
                if (control != drawable)
                {
                    if (control.Tag is CellBase && (control.Tag as CellBase).OnKill != null)
                        (control.Tag as CellBase).OnKill();

                    pixel1.Remove(control);
                }
            }

            if (ret)
                drawable.Invalidate();

            return ret;
        }

        private Type GetCellType(IEnumerable<Type> types, string name, Type type)
        {
            Type ret = null;

            foreach (var ct in types)
            {
                var attrs = ct.GetCustomAttributes(typeof(CellAttribute), true);

                foreach (CellAttribute a in attrs)
                {
                    if (a.Type == type || type.IsSubclassOf(a.Type))
                    {
                        if (a.Name == name)
                        {
                            ret = ct;
                            break;
                        }

                        if (string.IsNullOrEmpty(a.Name) && ret == null)
                            ret = ct;
                    }
                }
            }

            return ret;
        }

        public void AddEntry(string category, string name, object value, Type type, EventHandler eventHandler = null, bool editable = true)
        {
            var cellType = GetCellType(_cellTypes, name, type);

            var cell = (cellType == null) ? new CellText() : (CellBase)Activator.CreateInstance(cellType);
            cell.Create(category, name, value, type, eventHandler);
            cell.Editable = (cellType != null) && editable;

            _cells.Add(cell);
        }

        public void Update()
        {
            if (Group)
                _cells.Sort((x, y) => string.Compare(x.Category + x.Text, y.Category + y.Text) + (x.Category.Contains("Proc") ? 100 : 0) + (y.Category.Contains("Proc") ? -100 : 0));
            else
                _cells.Sort((x, y) => string.Compare(x.Text, y.Text) + (x.Category.Contains("Proc") ? 100 : 0) + (y.Category.Contains("Proc") ? -100 : 0));

            drawable.Invalidate();
        }

        private void SetCursor(CursorType cursor)
        {
            if (_currentCursor == cursor)
                return;

            _currentCursor = cursor;
            switch (cursor)
            {
                case CursorType.VerticalSplit:
                    drawable.Cursor = _cursorResize;
                    break;
                default:
#if IDE
                    drawable.Cursor = null;
#else
                    drawable.Cursor = _cursorNormal;
#endif
                    break;
            }
        }

        private void DrawGroup(Graphics g, Rectangle rec, string text)
        {
            g.FillRectangle(DrawInfo.BorderColor, rec);
            g.DrawText(DrawInfo.TextFont, DrawInfo.TextColor, rec.X + 1, rec.Y + (rec.Height - DrawInfo.TextFont.LineHeight) / 2, text);
        }

        private void Drawable_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            DrawInfo.SetPixelsPerPoint(g);
            var rec = new Rectangle(0, 0, drawable.Width - 1, DrawInfo.TextHeight + _spacing);
            var overGroup = false;
            string prevCategory = null;

            _separatorPos = Math.Min(Width - _separatorSafeDistance, Math.Max(_separatorSafeDistance, _separatorPos));
            _selectedCell = null;

            g.Clear(DrawInfo.BackColor);

            foreach (var c in _cells)
            {
                rec.Height = c.Height + _spacing;

                // Draw group
                if (prevCategory != c.Category)
                {
                    if (c.Category.Contains("Proc") || Group)
                    {
                        DrawGroup(g, rec, c.Category);
                        prevCategory = c.Category;
                        overGroup |= rec.Contains(_mouseLocation);
                        rec.Y += DrawInfo.TextHeight + _spacing;
                    }
                }

                // Draw cell
                var selected = rec.Contains(_mouseLocation);
                if (selected)
                    _selectedCell = c;
                c.Draw(g, rec, _separatorPos, selected);

                // Draw separator for the current row
                g.FillRectangle(DrawInfo.BorderColor, _separatorPos - 1, rec.Y, 1, rec.Height);

                rec.Y += c.Height + _spacing;
            }

            // Draw separator for not filled rows
            g.FillRectangle(DrawInfo.BorderColor, _separatorPos - 1, rec.Y, 1, Height);

            // Set Height
            var newHeight = Math.Max(rec.Y + 1, Height - 2);
            if (_height != newHeight)
            {
                drawable.Height = _height = newHeight;
                SetWidth();
            }

            if (overGroup) // TODO: Group collapsing/expanding?
                SetCursor(CursorType.Arrow);
            else if (new Rectangle(_separatorPos - _separatorWidth / 2, 0, _separatorWidth, _height).Contains(_mouseLocation))
                SetCursor(CursorType.VerticalSplit);
            else
                SetCursor(CursorType.Arrow);
        }

        private void Drawable_MouseDown(object sender, MouseEventArgs e)
        {
            _skipEdit = ClearChildren();
            if (_currentCursor == CursorType.VerticalSplit)
            {
                _moveSeparator = true;
                _moveSeparatorAmount = (int)e.Location.X - _separatorPos;
            }
        }

        private void Drawable_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_moveSeparator && e.Location.X >= _separatorPos && _selectedCell != null && _selectedCell.Editable && !_skipEdit)
            {
                var action = new Action(() =>
                {
                    if (Global.IsGtk && !_selectedCell.HasDialog)
                    {
                        pixel1.RemoveAll();
                        pixel1 = new PixelLayout();
                        pixel1.Add(drawable, 0, 0);
                        _selectedCell.Edit(pixel1);
                        Content = pixel1;
                    }
                    else
                    {
                        _selectedCell.Edit(pixel1);
                    }

                    drawable.Invalidate();
                });

#if WINDOWS
                (drawable.ControlObject as System.Windows.Controls.Canvas).Dispatcher.BeginInvoke(action,
                    System.Windows.Threading.DispatcherPriority.ContextIdle, null);
#else
                action.Invoke();
#endif
            }
            else
            {
                _moveSeparator = false;
            }
        }

        private void Drawable_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseLocation = new Point((int)e.Location.X, (int)e.Location.Y);

            if (_moveSeparator)
                _separatorPos = _moveSeparatorAmount + _mouseLocation.X;

            drawable.Invalidate();
        }

        private void Drawable_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseLocation = new Point(-1, -1);
            _moveSeparator = false;
            drawable.Invalidate();
        }

        private void PropertyGridTable_SizeChanged(object sender, EventArgs e)
        {
            SetWidth();
            drawable.Invalidate();
        }

        public void SetWidth()
        {
#if WINDOWS
            var action = new Action(() =>
            {
                var scrollsize = (_height >= Height) ? System.Windows.SystemParameters.VerticalScrollBarWidth : 0.0;
                drawable.Width = pixel1.Width = (int)(Width - scrollsize - System.Windows.SystemParameters.BorderWidth * 2);

                foreach (var child in pixel1.Children)
                    if (child != drawable)
                        child.Width = drawable.Width - _separatorPos;
            });

            (drawable.ControlObject as System.Windows.Controls.Canvas).Dispatcher.BeginInvoke(action,
                System.Windows.Threading.DispatcherPriority.ContextIdle, null);
#else
            drawable.Width = pixel1.Width = Width - 2;

            foreach (var child in pixel1.Children)
                if (child != drawable)
                    child.Width = drawable.Width - _separatorPos;
#endif
        }
    }
}
