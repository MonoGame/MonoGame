# Setting Up MonoGame with Xamarin Studio on Windows
This tutorial will guide you how to setup MonoGame on Xamarain Studio for Windows.

## Requirements
You will need the following, to setup MonoGame on Xamarain Studio for Windows:

 - [Xamarin Studio](http://download.xamarin.com/studio/Windows/XamarinStudio-5.10.1.6-0.msi)
 - [MonoGame SDK](http://www.monogame.net/releases/v3.5.1/MonoGameSetup.exe)
 - [MonoGame Templates](http://addins.monodevelop.com/Stable/Win32/5.10.3/MonoDevelop.MonoGame-3.5.0.1677.mpack)

## Setup
 1. First, install Xamarin Studio. Use the default options.
 2. Once you have installed Xamarin Studio, run it, and disable auto-update by clicking on **Help** > **Check for Updates...** and uncheck the checkbox that says **Check automatically**. Ensure that you click the **Close** button instead of **Restart and Install Updates** as Xamarin Studio may have already downloaded the update files automatically.
 3. Install MonoGame SDK. You can optionally uncheck any version of Visual Studio Templates if you do not have Visual Studio installed or you have no desire to develop MonoGame with Visual Studio.
 4. Install MonoGame Templates by launching Xamarain Studio. Then click on **Tools** > **Add-in Manager**. At the *Add-in Manager* windows, click on **Install from file...** and browse to the `MonoDevelop.MonoGame-3.5.0.1677.mpack` file and select it, then click on Open.  

## Conclusion
Now that you have installed MonoGame on Xamarin Studio for Windows, you can follow the [tutorials](tutorials.md) to test the installation.
