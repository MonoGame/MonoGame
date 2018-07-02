Imports Android.App
Imports Android.Content.PM
Imports Android.OS
Imports Android.Views

Namespace BlankApp.Android
    <Activity(Label = "BlankApp.Android",
        MainLauncher = True,
        Icon = "@drawable/icon",
        Theme = "@style/Theme.Splash",
        AlwaysRetainTaskState = True,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.FullUser,
        ConfigurationChanges = ConfigChanges.Orientation Or ConfigChanges.Keyboard Or ConfigChanges.KeyboardHidden Or ConfigChanges.ScreenSize)>
    Public Class Activity1 : Inherits Microsoft.Xna.Framework.AndroidGameActivity
        Protected Overrides Sub OnCreate(ByVal bundle As Bundle)
            base.OnCreate(bundle)
            Dim g As New Game1()
            SetContentView(CType(g.Services.GetService(Of View), View))
            g.Run()
        End Sub
    End Class
End Namespace

