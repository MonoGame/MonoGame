Imports System
Imports Foundation
Imports UIKit

Namespace BlankApp.iOS
    <Register("AppDelegate")>
    Class Program : Inherits UIApplicationDelegate
        Private Shared game As Game1

        Friend Shared Sub RunGame()
            game = New Game1()
            game.Run()
        End Sub

        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        Private Shared Sub Main(ByVal args As String())
            UIApplication.Main(args, Nothing, "AppDelegate")
        End Sub

        Public Overrides Sub FinishedLaunching(ByVal app As UIApplication)
            RunGame()
        End Sub
    End Class
End Namespace
