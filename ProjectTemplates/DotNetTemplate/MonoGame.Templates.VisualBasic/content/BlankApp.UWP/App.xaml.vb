Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices.WindowsRuntime
Imports Windows.ApplicationModel
Imports Windows.ApplicationModel.Activation
Imports Windows.Foundation
Imports Windows.Foundation.Collections
Imports Windows.UI.ViewManagement
Imports Windows.UI.Xaml
Imports Windows.UI.Xaml.Controls
Imports Windows.UI.Xaml.Controls.Primitives
Imports Windows.UI.Xaml.Data
Imports Windows.UI.Xaml.Input
Imports Windows.UI.Xaml.Media
Imports Windows.UI.Xaml.Navigation

' The Blank Application template is documented at http:'go.microsoft.com/fwlink/?LinkId=402347&clcid=0x409

Namespace BlankApp.UWP
    ''' <summary>
    ''' Provides application-specific behavior to supplement the default Application class.
    ''' </summary>
    Partial NotInheritable Class App : Inherits Application

        Shared deviceFamily As String

        ''' <summary>
        ''' Initializes the singleton application object.  This is the first line of authored code
        ''' executed, and as such is the logical equivalent of main() or WinMain().
        ''' </summary>
        Public Sub New()
            Me.InitializeComponent()
            Me.Suspending += AddressOf OnSuspending

            'API check to ensure the "RequiresPointerMode" property exists, ensuring project is running on build 14393 or later
            If Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Application", "RequiresPointerMode") Then
                'If running on the Xbox, disable the default on screen pointer
                If IsXbox() Then
                    Application.Current.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested
                End If
            End If
        End Sub

        ''' <summary>
        ''' Detection code in Windows 10 to identify the platform it is being run on
        ''' This function returns true if the project is running on an XboxOne
        ''' </summary>
        Public Shared Function IsXbox() As Boolean
            If deviceFamily Is Nothing Then
                deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily
            End If

            Return deviceFamily = "Windows.Xbox"
        End Function

        ''' <summary>
        ''' Invoked when the application is launched normally by the end user.  Other entry points
        ''' will be used such as when the application is launched to open a specific file.
        ''' </summary>
        ''' <param name="e">Details about the launch request and process.</param>
        Protected Overrides Sub OnLaunched(ByVal e As LaunchActivatedEventArgs)
            ' By default we want to fill the entire core window.
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow)

#If DEBUG Then
            If System.Diagnostics.Debugger.IsAttached Then
                Me.DebugSettings.EnableFrameRateCounter = true
            End If
#End If

            Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

            ' Do not repeat app initialization when the Window already has content,
            ' just ensure that the window is active
            If rootFrame Is Nothing Then
                ' Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = New Frame()

                rootFrame.NavigationFailed += AddressOf OnNavigationFailed

                If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                    'TODO: Load state from previously suspended application
                End If

                ' Place the frame in the current Window
                Window.Current.Content = rootFrame
            End If

            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(GamePage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End Sub

        ''' <summary>
        ''' Invoked when Navigation to a certain page fails
        ''' </summary>
        ''' <param name="sender">The Frame which failed navigation</param>
        ''' <param name="e">Details about the navigation failure</param>
        Private Sub OnNavigationFailed(ByVal sender As Object, ByVal e As NavigationFailedEventArgs)
            Throw New Exception("Failed to load Page " & e.SourcePageType.FullName)
        End Sub

        ''' <summary>
        ''' Invoked when application execution is being suspended.  Application state is saved
        ''' without knowing whether the application will be terminated or resumed with the contents
        ''' of memory still intact.
        ''' </summary>
        ''' <param name="sender">The source of the suspend request.</param>
        ''' <param name="e">Details about the suspend request.</param>
        Private Sub OnSuspending(ByVal sender As Object, ByVal e As SuspendingEventArgs)
            Dim deferral = e.SuspendingOperation.GetDeferral()
            'TODO: Save application state and stop any background activity
            deferral.Complete()
        End Sub
    End Class
End Namespace
