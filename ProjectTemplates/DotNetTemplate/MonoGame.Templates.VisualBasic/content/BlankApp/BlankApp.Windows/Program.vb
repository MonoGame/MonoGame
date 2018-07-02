Imports System

Namespace BlankApp.Windows
    ''' <summary>
    ''' The main module.
    ''' </summary>
    Public Module Program
        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        <STAThread>
        Private Sub Main()
            Using game = New Game1()
                game.Run()
            End Using
        End Sub
    End Module
End Namespace
