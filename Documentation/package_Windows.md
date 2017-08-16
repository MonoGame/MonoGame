## Runtime requirements

A Windows game targeting standard desktop Windows will require the following in order to run:
# Windows 7 or above (Windows XP and Vista are unsupported)
# DirectX 11 compliant graphics card and driver
# DirectX June 2010 redistributables
# .Net Framework 4.5
# Windows Media Player

## Handling the Windows Media Player dependency

While most store automatically manage the .Net and DirectX dependencies, they don't verify the presence of Windows Media Player.

Since it is forbidden by its license to redistribute it in any form, it is best to implement a check in your game and to prompt the users to install Windows Media Player if it isn't present. To do so, you can use the following code in your Program.cs file (before Game.Run()):

```
#if WINDOWS
// detect Media Feature Pack presence            
object legacyWMPCheck = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Active Setup\Installed Components\{22d6f312-b0f6-11d0-94ab-0080c74c7e95}", "IsInstalled", null);
if (legacyWMPCheck == null || legacyWMPCheck.ToString() != "1")
{
	System.Windows.Forms.MessageBox.Show("It appears that you don't have Windows Media Player installed. This game needs system features bound to Windows Media Player. Please install the Media Feature Pack corresponding to your Windows version to run this game:"
		+ Environment.NewLine
		+ Environment.NewLine
		+ "Windows 7: http://www.microsoft.com/en-US/download/details.aspx?id=16546"
		+ Environment.NewLine
		+ Environment.NewLine
		+ "Windows 8: http://www.microsoft.com/en-US/download/details.aspx?id=30685"
		+ Environment.NewLine
		+ Environment.NewLine
		+ "Windows 8.1: http://www.microsoft.com/en-US/download/details.aspx?id=40744"
		+ Environment.NewLine
		+ Environment.NewLine
		+ "Windows 10: https://www.microsoft.com/en-US/download/details.aspx?id=48231"
		+ Environment.NewLine
		+ Environment.NewLine
		+ "Windows Vista & XP: http://www.microsoft.com/en-US/download/windows-media-player-details.aspx?id=8163",
		"Missing Windows Media Player", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

	return;
}
#endif
```

Note that this code adds a dependency to System.Windows.Forms.

Windows Media Player is required by MonoGame because WMA files playback relies on system dll's which are only present when Windows Media Player is installed. This is limitation of the Windows ecosystem and it cannot be worked around unless MonoGame drops the WMA format for Windows DirectX projects. For compatibility reasons with XNA (which has the exact same requirement), Windows DirectX projects will stick to WMA.

If you wish to drop WMA in favor of another format, you may want to consider switching your project to DesktopGL (which uses OGG).

## Packaging

There is no need to package a Windows DirectX game. The best practice for distribution is to simply zip your output folder and distribute it as-is.

## Special note for Steam

Make sure that your Steamworks settings are set to verify the presence of .Net 4.5 and Direct X June 2010 redist. Steam will then install them automatically.

## Special note for itch.io

For your game to be compatible with the itch.io desktop application, you only have to zip it. Make sure to use the zip format (rar is forbidden by itch.io) and to have the .exe file at the root of the zip archive.

The itch.io desktop application doesn't check for software requirements yet (it is planned), hence it is recommended to document them in a readme file and on the installation instructions section of your itch.io game page.