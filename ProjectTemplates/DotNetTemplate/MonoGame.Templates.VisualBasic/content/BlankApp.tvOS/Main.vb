#Region "Imports Statements"
Imports System
Imports System.Collections.Generic
Imports System.Linq
#If MONOMAC Then
Imports MonoMac.AppKit
Imports MonoMac.Foundation
#ElseIf __IOS__ OrElse __TVOS__ Then
Imports Foundation
Imports UIKit
#End If
#End Region

Namespace BlankApp.tvOS
#If __IOS__ OrElse __TVOS__ Then
    <Register("AppDelegate")>
    Class Program : Inherits UIApplicationDelegate
#Else
    Module Program
#End If
        Private Shared game As Game1

        Friend Shared Sub RunGame()
            game = New Game1()
            game.Run()
#If Not __IOS__ AndAlso Not __TVOS__ Then
            game.Dispose()
#End If
        End Sub

        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
#If Not MONOMAC AndAlso Not __IOS__ AndAlso Not __TVOS__ Then
        <STAThread>
        Private Shared Sub Main(ByVal args As String())
#Else
        Private Shared Sub Main(ByVal args As String())
#End If
#If MONOMAC Then
            NSApplication.Init ()

            MonoNSRelease()
            
#ElseIf __IOS__ OrElse __TVOS__ Then
            UIApplication.Main(args, Nothing, "AppDelegate")
#Else
            RunGame()
#End If
        End Sub

        Private Sub MonoNSRelease(ByVal Optional p As var = New NSAutoreleasePool())
            NSApplication.SharedApplication.[Delegate] = New AppDelegate()
            NSApplication.Main(args)
        End Sub

#If __IOS__ OrElse __TVOS__ Then
        Public Overrides Sub FinishedLaunching(ByVal app As UIApplication)
            RunGame()
        End Sub
#End If
    End Module

#If MONOMAC Then
    Class AppDelegate : Inherits NSApplicationDelegate
        Public Overrides Sub FinishedLaunching(ByVal notification As MonoMac.Foundation.NSObject)
            AppDomain.CurrentDomain.AssemblyResolve += Function(ByVal sender As Object, ByVal a As ResolveEventArgs)
                If a.Name.StartsWith("MonoMac") Then
                    Return GetType(MonoMac.AppKit.AppKitFramework).Assembly
                End If
                Return Nothing
            End Function
            Program.RunGame()
        End Sub

        Public Overrides Function ApplicationShouldTerminateAfterLastWindowClosed(ByVal sender As NSApplication) As Boolean
            Return True
        End Function
    End Class
#End If
End Namespace
