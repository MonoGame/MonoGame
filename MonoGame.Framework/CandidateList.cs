using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CandidateList
    {
        public uint dwSize;
        public uint dwStyle;
        public uint dwCount;
        public uint dwSelection;
        public uint dwPageStart;
        public uint dwPageSize;

        /// DWORD[1]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.U4)]
        public uint[] dwOffset;
    }
}
