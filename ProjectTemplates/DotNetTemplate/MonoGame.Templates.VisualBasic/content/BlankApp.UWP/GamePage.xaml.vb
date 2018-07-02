Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices.WindowsRuntime
Imports Windows.Foundation
Imports Windows.Foundation.Collections
Imports Windows.UI.Xaml
Imports Windows.UI.Xaml.Controls
Imports Windows.UI.Xaml.Controls.Primitives
Imports Windows.UI.Xaml.Data
Imports Windows.UI.Xaml.Input
Imports Windows.UI.Xaml.Media
Imports Windows.UI.Xaml.Navigation

' The Blank Page item template is documented at http:'go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

Namespace BlankApp.UWP
    ''' <summary>
    ''' An empty page that can be used on its own or navigated to within a Frame.
    ''' </summary>
    Partial Public NotInheritable Class GamePage : Inherits Page
        Private ReadOnly _game As Game1

        Public Sub New()
            this.InitializeComponent()

            ' Create the game.
            Dim launchArguments = String.Empty
            _game = MonoGame.Framework.XamlGame < Game1 > .Create(launchArguments, Window.Current.CoreWindow, swapChainPanel)
        End Sub
    End Class
End Namespace
