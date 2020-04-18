using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    internal abstract class ImeCompositionResultHandler
    {
        internal IntPtr ImeContext { get; set; }

        public int Flag { get; private set; }

        internal ImeCompositionResultHandler(int flag)
        {
            this.Flag = flag;
            this.ImeContext = IntPtr.Zero;
        }

        internal virtual void Update() { }

        internal bool Update(int lParam)
        {
            if ((lParam & Flag) == Flag)
            {
                Update();
                return true;
            }
            return false;
        }
    }

    internal class ImeCompositionString : ImeCompositionResultHandler, IEnumerable<byte>
    {
        private byte[] _values;

        public int Length { get; private set; }

        public byte[] Values { get { return _values; } }

        public byte this[int index] { get { return _values[index]; } }

        internal ImeCompositionString(int flag) : base(flag)
        {
            Clear();
        }

        public IEnumerator<byte> GetEnumerator()
        {
            foreach (byte b in _values)
                yield return b;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            if (Length <= 0) return String.Empty;
            return Encoding.Unicode.GetString(_values, 0, Length);
        }

        internal void Clear()
        {
            _values = new byte[0];
            Length = 0;
        }

        internal override void Update()
        {
            Length = IMM.ImmGetCompositionString(ImeContext, Flag, IntPtr.Zero, 0);
            IntPtr pointer = Marshal.AllocHGlobal(Length);
            try
            {
                IMM.ImmGetCompositionString(ImeContext, Flag, pointer, Length);
                _values = new byte[Length];
                Marshal.Copy(pointer, _values, 0, Length);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
    }

    internal class ImeCompositionInt : ImeCompositionResultHandler
    {
        public int Value { get; private set; }

        internal ImeCompositionInt(int flag) : base(flag) { }

        public override string ToString()
        {
            return Value.ToString();
        }

        internal override void Update()
        {
            Value = IMM.ImmGetCompositionString(ImeContext, Flag, IntPtr.Zero, 0);
        }
    }
}
