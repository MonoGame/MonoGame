using ___SafeGameName___.Core.Effects;
using ___SafeGameName___.Core.Inputs;
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace ___SafeGameName___.Core;

/// <summary>
/// A uniform grid of tiles with collections of gems and enemies.
/// The level owns the player and controls the game's win and lose
/// conditions as well as scoring.
/// </summary>
class Level : IDisposable
{
    // Physical structure of the level.
    private Tile[,] tiles;
    private Texture2D[] layers;
    // The layer which entities are drawn on top of.
    private const int EntityLayer = 2;

    // Entities in the level.
    public Player Player
    {
        get { return player; }
    }
    Player player;

    private List<Gem> gems = new List<Gem>();
    private List<Enemy> enemies = new List<Enemy>();

    // Key locations in the level.        
    private Vector2 start;
    private Point exit = InvalidPosition;
    private static readonly Point InvalidPosition = new Point(-1, -1);

    // Level game state.
    private Random random = new Random(354668); // Arbitrary, but constant seed

    public int Score
    {
        get { return score; }
    }
    int score;

    public bool ReachedExit
    {
        get { return reachedExit; }
    }
    bool reachedExit;

    public TimeSpan TimeTaken
    {
        get { return timeTaken; }
    }
    TimeSpan timeTaken;

    private TimeSpan maximumTimeToCompleteLevel = TimeSpan.FromMinutes(2.0);
    public TimeSpan MaximumTimeToCompleteLevel { get => maximumTimeToCompleteLevel; }

    private const int PointsPerSecond = 5;

    // Level content.        
    public ContentManager Content
    {
        get { return content; }
    }
    ContentManager content;

    private SoundEffect exitReachedSound;

    // The number of levels in the Levels directory of our content. We assume that
    // levels in our content are 0-based and that all numbers under this constant
    // have a level file present. This allows us to not need to check for the file
    // or handle exceptions, both of which can add unnecessary time to level loading.
    public const int NUMBER_OF_LEVELS = 5;
    private const int NUMBER_OF_LAYERS = 3;

    /// <summary>
    /// Constructs a new level.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider that will be used to construct a ContentManager.
    /// </param>
    /// <param name="fileStream">
    /// A stream containing the tile data.
    /// </param>
    public Level(IServiceProvider serviceProvider, string levelPath, int levelIndex)
    {
        // Create a new content manager to load content used just by this level.
        content = new ContentManager(serviceProvider, "Content");

        timeTaken = TimeSpan.Zero;

        using (Stream fileStream = TitleContainer.OpenStream(levelPath))
        {
            LoadTiles(fileStream);
        }

        // Load background layer textures. For now, all levels must
        // use the same backgrounds and only use the left-most part of them.
        layers = new Texture2D[3];
        for (int i = 0; i < layers.Length; ++i)
        {
            // Choose a random segment if each background layer for level variety.
            int segmentIndex = levelIndex % NUMBER_OF_LAYERS;
            layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
        }

        // Load sounds.
        exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
    }

    /// <summary>
    /// Iterates over every tile in the structure file and loads its
    /// appearance and behavior. This method also validates that the
    /// file is well-formed with a player start point, exit, etc.
    /// </summary>
    /// <param name="fileStream">
    /// A stream containing the tile data.
    /// </param>
    private void LoadTiles(Stream fileStream)
    {
        // Load the level and ensure all of the lines are the same length.
        int width;
        List<string> lines = new List<string>();
        using (StreamReader reader = new StreamReader(fileStream))
        {
            string line = reader.ReadLine();
            width = line.Length;
            while (line != null)
            {
                lines.Add(line);
                if (line.Length != width)
                    throw new Exception(String.Format(Resources.ErrorLevelLineLength, lines.Count));
                line = reader.ReadLine();
            }
        }

        // Allocate the tile grid.
        tiles = new Tile[width, lines.Count];

        // Loop over every tile position,
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                // to load each tile.
                char tileType = lines[y][x];
                tiles[x, y] = LoadTile(tileType, x, y);
            }
        }

        // Verify that the level has a beginning and an end.
        if (Player == null)
            throw new NotSupportedException(Resources.ErrorLevelStartingPoint);
        if (exit == InvalidPosition)
            throw new NotSupportedException(Resources.ErrorLevelExit);

    }

    /// <summary>
    /// Loads an individual tile's appearance and behavior.
    /// </summary>
    /// <param name="tileType">
    /// The character loaded from the structure file which
    /// indicates what should be loaded.
    /// </param>
    /// <param name="x">
    /// The X location of this tile in tile space.
    /// </param>
    /// <param name="y">
    /// The Y location of this tile in tile space.
    /// </param>
    /// <returns>The loaded tile.</returns>
    private Tile LoadTile(char tileType, int x, int y)
    {
        switch (tileType)
        {
            // Blank space
            case '.':
                return new Tile(null, TileCollision.Passable);

            // Exit
            case 'X':
                return LoadExitTile(x, y);

            // Minimal value Gem
            case '1':
                return LoadGemTile(x, y, tileType);
            // Mediuam value Gem
            case '2':
                return LoadGemTile(x, y, tileType);
            // Maximum value Gem
            case '3':
                return LoadGemTile(x, y, tileType);

            // Floating platform
            case '-':
                return LoadTile("Platform", TileCollision.Platform);

            // Various enemy types
            case 'A':
            case 'B':
            case 'C':
            case 'D':
                return LoadEnemyTile(x, y, tileType);

            // Platform block
            case '~':
                return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

            // Passable block
            case ':':
                return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

            // Impassable block
            case '#':
                return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

            // Breakable block
            case ';':
                return LoadVarietyTile("BlockB", 2, TileCollision.Breakable);

            // Player 1 start point
            case 'P':
                return LoadStartTile(x, y);

            // Unknown tile type character
            default:
                throw new NotSupportedException(String.Format(Resources.ErrorUnsupportedTileType, tileType, x, y));
        }
    }

    /// <summary>
    /// Creates a new tile. The other tile loading methods typically chain to this
    /// method after performing their special logic.
    /// </summary>
    /// <param name="name">
    /// Path to a tile texture relative to the Content/Tiles directory.
    /// </param>
    /// <param name="collision">
    /// The tile collision type for the new tile.
    /// </param>
    /// <returns>The new tile.</returns>
    private Tile LoadTile(string name, TileCollision collision)
    {
        return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
    }

    /// <summary>
    /// Loads a tile with a random appearance.
    /// </summary>
    /// <param name="baseName">
    /// The content name prefix for this group of tile variations. Tile groups are
    /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
    /// </param>
    /// <param name="variationCount">
    /// The number of variations in this group.
    /// </param>
    private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
    {
        int index = random.Next(variationCount);
        return LoadTile(baseName + index, collision);
    }

    /// <summary>
    /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
    /// </summary>
    private Tile LoadStartTile(int x, int y)
    {
        if (Player != null)
            throw new NotSupportedException(Resources.ErrorLevelOneStartingPoint);

        start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
        player = new Player(this, start);

        return new Tile(null, TileCollision.Passable);
    }

    /// <summary>
    /// Remembers the location of the level's exit.
    /// </summary>
    private Tile LoadExitTile(int x, int y)
    {
        if (exit != InvalidPosition)
            throw new NotSupportedException(Resources.ErrorLevelOneExit);

        exit = GetBounds(x, y).Center;

        return LoadTile("Exit", TileCollision.Passable);
    }

    /// <summary>
    /// Instantiates an enemy and puts him in the level.
    /// </summary>
    private Tile LoadEnemyTile(int x, int y, char monsterType)
    {
        Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
        enemies.Add(new Enemy(this, position, "Monster" + monsterType));

        return new Tile(null, TileCollision.Passable);
    }

    /// <summary>
    /// Instantiates a gem and puts it in the level.
    /// </summary>
    private Tile LoadGemTile(int x, int y, char gemType)
    {
        Point position = GetBounds(x, y).Center;
        gems.Add(new Gem(this, new Vector2(position.X, position.Y), gemType, new Vector2(Width * Tile.Width, Height * Tile.Height)));

        return new Tile(null, TileCollision.Passable);
    }

    /// <summary>
    /// Unloads the level content.
    /// </summary>
    public void Dispose()
    {
        Content.Unload();
    }

    /// <summary>
    /// Gets the collision mode of the tile at a particular location.
    /// This method handles tiles outside of the levels boundries by making it
    /// impossible to escape past the left or right edges, but allowing things
    /// to jump beyond the top of the level and fall off the bottom.
    /// </summary>
    public TileCollision GetCollision(int x, int y)
    {
        // Prevent escaping past the level ends.
        if (x < 0 || x >= Width)
            return TileCollision.Impassable;
        // Allow jumping past the level top and falling through the bottom.
        if (y < 0 || y >= Height)
            return TileCollision.Passable;

        return tiles[x, y].Collision;
    }

    /// <summary>
    /// Gets the bounding rectangle of a tile in world space.
    /// </summary>
    public Rectangle GetBounds(int x, int y)
    {
        return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
    }

    /// <summary>
    /// Width of level measured in tiles.
    /// </summary>
    public int Width
    {
        get { return tiles.GetLength(0); }
    }

    /// <summary>
    /// Height of the level measured in tiles.
    /// </summary>
    public int Height
    {
        get { return tiles.GetLength(1); }
    }

    private ParticleManager particleManager;
    private bool particlesExploding;
    public ParticleManager ParticleManager { get => particleManager; set => particleManager = value; }

    /// <summary>
    /// Updates all objects in the world, performs collision between them,
    /// and handles the time limit with scoring.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="keyboardState">Provides a snapshot of timing values.</param>
    /// <param name="gamePadState">Provides a snapshot of timing values.</param>
    /// <param name="accelerometerState">Provides a snapshot of timing values.</param>
    /// <param name="displayOrientation">Provides a snapshot of timing values.</param>
    public void Update(
        GameTime gameTime,
        KeyboardState keyboardState,
        GamePadState gamePadState,
        AccelerometerState accelerometerState,
        DisplayOrientation displayOrientation,
        bool readyToPlay = true)
    {
        particleManager.Update(gameTime);

        if (ReachedExit
            && !particlesExploding)
        {
            particleManager.Position = Player.Position;
            particleManager.Emit(100, SettingsScreen.CurrentParticleEffect);
            particlesExploding = true;
        }

        // Pause while the player is dead or we've reached maximum time allowed.
        if (!Player.IsAlive || TimeTaken == MaximumTimeToCompleteLevel)
        {
            // Still want to perform physics on the player.
            Player.ApplyPhysics(gameTime);
        }
        else if (ReachedExit)
        {
            // Animate the time being converted into points.
            int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
            seconds = Math.Min(seconds, (int)Math.Ceiling(TimeTaken.TotalSeconds));
            timeTaken += TimeSpan.FromSeconds(seconds);
            score += seconds * PointsPerSecond;
        }
        else
        {
            if (readyToPlay)
            {
                timeTaken += gameTime.ElapsedGameTime;

                Player.Update(gameTime, keyboardState, gamePadState, accelerometerState, displayOrientation);

                UpdateGems(gameTime);

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }
        }

        if (timeTaken > maximumTimeToCompleteLevel)
        {
            timeTaken = maximumTimeToCompleteLevel;
        }
    }

    /// <summary>
    /// Animates each gem and checks to allows the player to collect them.
    /// </summary>
    private void UpdateGems(GameTime gameTime)
    {
        for (int i = 0; i < gems.Count; ++i)
        {
            Gem gem = gems[i];

            gem.Update(gameTime);

            switch (gem.State)
            {
                case GemState.Collected:
                    gems.RemoveAt(i--);
                    break;

                case GemState.Collecting:
                    break;

                case GemState.Waiting:
                    if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                    {
                        gem.Scale = new Vector2(1.5f, 1.5f);
                        gem.State = GemState.Collecting;
                        OnGemCollected(gem, Player);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Animates each enemy and allow them to kill the player.
    /// </summary>
    private void UpdateEnemies(GameTime gameTime)
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.Update(gameTime);

            // Touching an enemy instantly kills the player
            if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
            {
                OnPlayerKilled(enemy);
            }
        }
    }

    /// <summary>
    /// Called when a gem is collected.
    /// </summary>
    /// <param name="gem">The gem that was collected.</param>
    /// <param name="collectedBy">The player who collected this gem.</param>
    private void OnGemCollected(Gem gem, Player collectedBy)
    {
        score += gem.Value;

        gem.OnCollected(collectedBy);
    }

    /// <summary>
    /// Called when the player is killed.
    /// </summary>
    /// <param name="killedBy">
    /// The enemy who killed the player. This is null if the player was not killed by an
    /// enemy, such as when a player falls into a hole.
    /// </param>
    private void OnPlayerKilled(Enemy killedBy)
    {
        Player.OnKilled(killedBy);
    }

    /// <summary>
    /// Called when the player reaches the level's exit.
    /// </summary>
    private void OnExitReached()
    {
        Player.OnReachedExit();
        exitReachedSound.Play();
        reachedExit = true;
    }

    /// <summary>
    /// Restores the player to the starting point to try the level again.
    /// </summary>
    public void StartNewLife()
    {
        Player.Reset(start);
    }

    /// <summary>
    /// Draw everything in the level from background to foreground.
    /// </summary>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        for (int i = 0; i <= EntityLayer; ++i)
            spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

        DrawTiles(spriteBatch);

        foreach (Gem gem in gems)
            gem.Draw(gameTime, spriteBatch);

        Player.Draw(gameTime, spriteBatch);

        foreach (Enemy enemy in enemies)
            enemy.Draw(gameTime, spriteBatch);

        for (int i = EntityLayer + 1; i < layers.Length; ++i)
            spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

        particleManager.Draw(spriteBatch);
    }

    /// <summary>
    /// Draws each tile in the level.
    /// </summary>
    private void DrawTiles(SpriteBatch spriteBatch)
    {
        // For each tile position
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                // If there is a visible tile in that position
                Texture2D texture = tiles[x, y].Texture;
                if (texture != null)
                {
                    // Draw it in screen space.
                    Vector2 position = new Vector2(x, y) * Tile.Size;
                    spriteBatch.Draw(texture, position, Color.White);
                }
            }
        }
    }

    // BreakTile method should handle triggering its destruction animation
    internal void BreakTile(int x, int y)
    {
        RemoveTile(x, y);

        // Use Particle effect to explode the removed tile
        particleManager.Position = new Vector2(Player.Position.X, Player.Position.Y - 20);
        particleManager.Emit(50, ParticleEffectType.Confetti, Color.SandyBrown);
    }

    internal void RemoveTile(int x, int y)
    {
        // By making the tile passable with no nexture, it no longer "exists" in the game world
        // Thus making the level layout dynamic
        tiles[x, y] = new Tile(null, TileCollision.Passable);
    }
}
