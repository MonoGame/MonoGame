Imports System

Namespace BlankApp.Desktop
    ''' <summary>
    ''' The main class.
    ''' </summary>
    Public Module Program
        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        <STAThread>
        Shared Sub Main()
            Using game As New Game1()
                game.Run()
            End Using
        End Sub
    End Module
End Namespace
