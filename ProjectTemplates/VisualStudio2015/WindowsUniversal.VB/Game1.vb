Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

''' <summary>
''' This is the main type for your game.
''' </summary>
Public Class Game1
    Inherits Game

    Private graphics As GraphicsDeviceManager
    Private spriteBatch As SpriteBatch

    Public Sub New()
        graphics = New GraphicsDeviceManager(Me)
        Content.RootDirectory = "Content"
    End Sub

    ''' <summary>
    ''' Allows the game to perform any initialization it needs to before starting to run.
    ''' This is where it can query for any required services and load any non-graphic
    ''' related content.  Calling base.Initialize will enumerate through any components
    ''' and initialize them as well.
    ''' </summary>
    Protected Overrides Sub Initialize()
        '' TODO: Add your initialization logic here

        MyBase.Initialize()
    End Sub

    ''' <summary>
    ''' LoadContent will be called once per game and is the place to load
    ''' all of your content.
    ''' </summary>
    Protected Overrides Sub LoadContent()
        '' Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = New SpriteBatch(GraphicsDevice)

        '' TODO: use this.Content to load your game content here
    End Sub

    ''' <summary>
    ''' UnloadContent will be called once per game and is the place to unload
    ''' game-specific content.
    ''' </summary>
    Protected Overrides Sub UnloadContent()
        '' TODO: Unload any non ContentManager contenthere.
    End Sub

    ''' <summary>
    ''' Allws the game to run logic such as updating the world,
    ''' checking for collisions, gathering input, and playing audio.
    ''' </summary>
    ''' <param name="gameTime">Provides the snapshot of timing values</param>
    Protected Overrides Sub Update(gameTime As GameTime)
        '' TODO: Add your update logic here

        MyBase.Update(gameTime)
    End Sub

    ''' <summary>
    ''' This is called when the game should draw itself.
    ''' </summary>
    ''' <param name="gameTime">Provides a snapshot of timing values.</param>
    Protected Overrides Sub Draw(gameTime As GameTime)
        GraphicsDevice.Clear(Color.CornflowerBlue)

        '' TODO: Add your drawing code here

        MyBase.Draw(gameTime)
    End Sub
End Class
