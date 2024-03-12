
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace MonoGame.Interop;

internal readonly struct GamePtr { }

internal readonly struct GameWindowPtr { }

internal static unsafe partial class GameWrapper
{
    [LibraryImport("monogame", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void MG_GW_SetAllowUserResizing(GamePtr* game, GameWindowPtr* gameWindow, [MarshalAs(UnmanagedType.U1)] bool allowuserresizing);
}
