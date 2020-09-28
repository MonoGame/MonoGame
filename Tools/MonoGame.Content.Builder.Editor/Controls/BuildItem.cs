using System;
using System.Collections.Generic;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    class BuildItem
    {
        private const int CellHeight = 32;
        private const int Spacing = 10;
        private const int Margin = 10;
        private const string ArrowCollapse  = "▲";
        private const string ArrowExpand = "▼";
        private const int ButtonSpacing = 3;

        public string Text { get; set; }
        public Image Icon { get; set; }
        public int Height { get; set; }
        public int RequestedWidth { get; set; }

        public string Description
        {
            set
            {
                _description.Clear();
                _description.Add(value);
            }
        }

        private readonly float _arrowWidth;
        private readonly float _textOffset;
        private readonly float _imageOffset;
        private readonly float _descSize;

        private List<string> _description;
        private float _descriptionOffset;
        private bool _expanded;
        private bool _selected;

        public BuildItem()
        {
            _arrowWidth = DrawInfo.TextFont.MeasureString(ArrowExpand).Width;
            _textOffset = (CellHeight - DrawInfo.TextHeight) / 2;
            _imageOffset = (CellHeight - 16) / 2;
            _descSize = DrawInfo.TextFont.LineHeight + 4;

            _description = new List<string>();

            Height = CellHeight;
            RequestedWidth = 0;
        }

        public void AddDescription(string text)
        {
            _description.Add(text);
        }

        public void OnClick()
        {
            if (_selected && _description.Count != 0)
            {
                _expanded = !_expanded;

                if (_expanded)
                {
                    _descriptionOffset = (_descSize - DrawInfo.TextHeight) / 2;
                    Height = (int)(CellHeight + Margin + (_descSize * _description.Count));

                    foreach (var des in _description)
                    {
                        var width = DrawInfo.TextFont.MeasureString(des).Width + 4 * Spacing + 16;
                        if (width > RequestedWidth)
                            RequestedWidth = (int)width;
                    }
                }
                else
                {
                    Height = CellHeight;
                    RequestedWidth = 0;
                }
            }
        }

        public void Draw(Graphics g, int y, int width)
        {
            var x = Margin;
            _selected = BuildOutput.MouseLocation.Y > y && BuildOutput.MouseLocation.Y < y + CellHeight;

            // Draw Background
            g.FillRectangle(DrawInfo.BorderColor, 0, y, width, Height);
            g.FillRectangle(_selected ? DrawInfo.HoverBackColor : DrawInfo.BorderColor, 0, y, width, CellHeight);

            // Draw Icon
            g.DrawImage(Icon, x, y + _imageOffset);
            x += 16 + Spacing;

            // Draw Text
            g.DrawText(DrawInfo.TextFont, DrawInfo.GetTextColor(_selected, false), x, y + _textOffset, Text);

            // Draw Expander
            if (_description.Count != 0)
            {
                //g.FillRectangle(_expandSelected ? DrawInfo.HoverBackColor : DrawInfo.BorderColor, rectangle);
                g.DrawText(DrawInfo.TextFont, DrawInfo.GetTextColor(_selected, false), width - Margin - _arrowWidth, y + _textOffset, _expanded ? ArrowCollapse : ArrowExpand);
            }

            // Draw Description
            if (_expanded)
            {
                for (int i = 0; i < _description.Count; i++)
                    g.DrawText(DrawInfo.TextFont, DrawInfo.TextColor, x + Spacing, y + CellHeight + _descriptionOffset + _descSize * i, _description[i]);
            }
        }
    }
}
