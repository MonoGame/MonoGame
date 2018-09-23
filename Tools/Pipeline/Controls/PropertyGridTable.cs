// MonoGame - Copyright (C) The MonoGame Team
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
        private int _separatorPos;
        private int _moveSeparator;
        private int _height;
        private bool _skipEdit;

        public PropertyGridTable()
        {
            InitializeComponent();

            _cellTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.IsSubclassOf(typeof(CellBase)));
            _separatorPos = 100;
            _mouseLocation = new Point(-1, -1);
            _cells = new List<CellBase>();
            _moveSeparator = -_separatorWidth / 2 - 1;
            _skipEdit = false;

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
            if (_currentCursor != cursor)
            {
                _currentCursor = cursor;
                Cursor = new Cursor(cursor);
            }
        }

        private void DrawGroup(Graphics g, Rectangle rec, string text)
        {
            var font = SystemFonts.Default();
            font = new Font(font.Family, font.Size, FontStyle.Bold);

            g.FillRectangle(DrawInfo.BorderColor, rec);
            g.DrawText(SystemFonts.Default(), DrawInfo.TextColor, rec.X + 1, rec.Y + (rec.Height - font.LineHeight) / 2, text);
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

            if (_cells.Count == 0)
            {
                if (_height != 10)
                    drawable.Height = _height = 10;

                return;
            }

            // Draw separator for not filled rows
            g.FillRectangle(DrawInfo.BorderColor, _separatorPos - 1, 0, 1, Height);

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

            if (_height != rec.Y + 1)
            {
                drawable.Height = _height = rec.Y + 1;
                SetWidth();
            }

            if (overGroup) // TODO: Group collapsing/expanding?
                SetCursor(CursorType.Default);
            else if ((new Rectangle(_separatorPos - _separatorWidth / 2, 0, _separatorWidth, Height)).Contains(_mouseLocation))
                SetCursor(CursorType.VerticalSplit);
            else
                SetCursor(CursorType.Default);
        }

        private void Drawable_MouseDown(object sender, MouseEventArgs e)
        {
            _skipEdit = ClearChildren();
            if (_currentCursor == CursorType.VerticalSplit)
                _moveSeparator = (int)e.Location.X - _separatorPos;
        }

        private void Drawable_MouseUp(object sender, MouseEventArgs e)
        {
            _moveSeparator = -_separatorWidth / 2 - 1;

            if (e.Location.X >= _separatorPos && _selectedCell != null && _selectedCell.Editable && !_skipEdit)
            {
                var action = new Action(() => _selectedCell.Edit(pixel1));

#if WINDOWS
                (drawable.ControlObject as System.Windows.Controls.Canvas).Dispatcher.BeginInvoke(action,
                    System.Windows.Threading.DispatcherPriority.ContextIdle, null);
#else
                action.Invoke();
#endif
            }
        }

        private void Drawable_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseLocation = new Point((int)e.Location.X, (int)e.Location.Y);

            if (_moveSeparator > -_separatorWidth / 2 - 1)
                _separatorPos = _moveSeparator + _mouseLocation.X;

            drawable.Invalidate();
        }

        private void Drawable_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouseLocation = new Point(-1, -1);
            drawable.Invalidate();

            _moveSeparator = -_separatorWidth / 2 - 1;
        }

        private void PropertyGridTable_SizeChanged(object sender, EventArgs e)
        {
#if WINDOWS
            SetWidth();
#endif

#if LINUX
            // force size reallocation
            drawable.Width = pixel1.Width - 2;

            foreach (var child in pixel1.Children)
                if (child != drawable)
                    child.Width = drawable.Width - _separatorPos;
#endif

            drawable.Invalidate();
        }

        public void SetWidth()
        {
#if WINDOWS
            var action = new Action(() =>
            {
                var scrollsize = (_height >= Height) ? System.Windows.SystemParameters.VerticalScrollBarWidth : 0.0;
                drawable.Width = (int)(Width - scrollsize - System.Windows.SystemParameters.BorderWidth * 2);

                foreach (var child in pixel1.Children)
                    if (child != drawable)
                        child.Width = drawable.Width - _separatorPos;
            });

            (drawable.ControlObject as System.Windows.Controls.Canvas).Dispatcher.BeginInvoke(action,
                System.Windows.Threading.DispatcherPriority.ContextIdle, null);

#elif MONOMAC
            drawable.Width = Width; // TODO: Subtract sctollbar size
#endif
        }
    }
}
