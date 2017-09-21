' The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

Imports Microsoft.Xna.Framework
Imports MonoGame.Framework
''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class GamePage
    Inherits Page

    ReadOnly _game As Game

    Public Sub New()

        Try
            InitializeComponent()

            ' Create the game.
            _game = XamlGame(Of Game1).Create(String.Empty, Window.Current.CoreWindow, SwapChainPanel)
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
            Debug.WriteLine(ex.InnerException)
        End Try

    End Sub

End Class
