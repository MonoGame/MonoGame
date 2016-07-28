// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using Eto.Forms;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Tools.Pipeline
{
    class CharRegion
    {
        public string StartString
        {
            get
            {
                return Start.ToString();
            }
            set
            {
                int temp;
                if (int.TryParse(value, out temp))
                    Start = temp;
            }
        }

        public string StartCharacter
        {
            get
            {
                return ((char)Start).ToString();
            }
        }

        public string EndString
        {
            get
            {
                return End.ToString();
            }
            set
            {
                int temp;
                if (int.TryParse(value, out temp))
                    End = temp;
            }
        }

        public string EndCharacter
        {
            get
            {
                return ((char)End).ToString();
            }
        }

        public int Start, End;

        public CharRegion(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    partial class CRDescDialog : DialogBase
    {
        public CharacterRegionsDescription CharRegions
        {
            get
            {
                var cregs = new CharacterRegion[_collection.Count];

                for (int i = 0; i < _collection.Count; i++)
                    cregs[i] = new CharacterRegion((char)_collection[i].Start, (char)_collection[i].End);

                return new CharacterRegionsDescription { Array = cregs };
            }
        }

        private GridView _gridView;
        private ObservableCollection<CharRegion> _collection;
        private GridColumn _columnStart, _columnStartChar, _columnEnd, _columnEndChar;

        public CRDescDialog(CharacterRegionsDescription cregs)
        {
            InitializeComponent();

            _collection = new ObservableCollection<CharRegion>();

            foreach (var cr in cregs.Array)
                _collection.Add(new CharRegion(cr.Start, cr.End));

            _columnStart = new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<CharRegion, string>(r => r.StartString) },
                HeaderText = "Start",
                Editable = true,
                Resizable = false
            };

            _columnStartChar = new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<CharRegion, string>(r => r.StartCharacter) },
                HeaderText = "Char",
                Resizable = false
            };

            _columnEnd = new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<CharRegion, string>(r => r.EndString) },
                HeaderText = "End",
                Editable = true,
                Resizable = false
            };

            _columnEndChar = new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<CharRegion, string>(r => r.EndCharacter) },
                HeaderText = "Char",
                Resizable = false
            };

            _gridView.DataStore = _collection;
            _gridView.Columns.Add(_columnStart);
            _gridView.Columns.Add(_columnStartChar);
            _gridView.Columns.Add(_columnEnd);
            _gridView.Columns.Add(_columnEndChar);

            _buttonAdd.Click += ButtonAdd_Click;
            _buttonRemove.Click += ButtonRemove_Click;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            var width = (_gridView.Width - 20) / 12;

            _columnStart.Width = width * 4;
            _columnStartChar.Width = width * 2;
            _columnEnd.Width = width * 4;
            _columnEndChar.Width = width * 2;
        }

        private void ButtonRemove_Click (object sender, EventArgs e)
        {
            var item = _gridView.SelectedItem as CharRegion;

            if (item != null)
                _collection.Remove(item);
        }

        private void ButtonAdd_Click (object sender, EventArgs e)
        {
            _collection.Add(new CharRegion(32, 126));
        }
    }
}

