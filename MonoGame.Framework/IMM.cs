using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Xna.Framework
{
    public class IMM
    {
        [DllImport("imm32.dll", SetLastError = true)]
        public static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hwnd);

        [DllImport("imm32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int ImmGetCompositionString(IntPtr hIMC, int CompositionStringFlag, byte[] buffer, int bufferLength);

        [DllImport("imm32.dll", CharSet = CharSet.Unicode, EntryPoint = "ImmGetCandidateList")]
        public static extern uint ImmGetCandidateList(IntPtr hIMC, uint deIndex, IntPtr candidateList, uint dwBufLen);
    }
}
