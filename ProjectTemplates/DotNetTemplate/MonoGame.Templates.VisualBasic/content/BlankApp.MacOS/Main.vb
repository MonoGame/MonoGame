#Region "Imports Statements"
Imports System
Imports System.Collections.Generic
Imports System.Linq

Imports AppKit
Imports Foundation
#End Region

Namespace BlankApp.MacOS
    Module Program
        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        Private Sub Main(ByVal args As String())
            NSApplication.Init()

            Using game = New Game1()
                game.Run()
            End Using
        End Sub
    End Module
End Namespace
