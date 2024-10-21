using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ___SafeGameName___.Core;

/// <summary>
/// A valuable item the player can collect.
/// </summary>
partial class Gem
{
    private Texture2D texture;
    private Vector2 origin;
    private SoundEffect collectedSound;

    public int Value = 10;
    public readonly Color Color = Color.Green;

    // The gem is animated from a base position along the Y axis.
    private Vector2 basePosition;
    private Vector2 levelDimensions;
    private float bounce;

    // Bounce control constants
    const float BounceHeight = 0.18f;
    const float BounceRate = 3.0f;
    const float BounceSync = -0.75f;

    public Level Level
    {
        get { return level; }
    }
    Level level;

    /// <summary>
    /// Gets the current position of this gem in world space.
    /// </summary>
    public Vector2 Position
    {
        get
        {
            return basePosition + new Vector2(0.0f, bounce);
        }
    }

    /// <summary>
    /// Gets a circle which bounds this gem in world space.
    /// </summary>
    public Circle BoundingCircle
    {
        get
        {
            return new Circle(Position, Tile.Width / 3.0f);
        }
    }

    Vector2 scale;
    public Vector2 Scale { get => scale; set => scale = value; }

    Vector2 collectedPosition;

    public GemState State { get; set; } = GemState.Waiting;

    /// <summary>
    /// Constructs a new gem.
    /// </summary>
    public Gem(Level level, Vector2 position, char gemType, Vector2 levelDimensions)
    {
        this.level = level;
        this.basePosition = position;
        this.levelDimensions = levelDimensions;

        switch (gemType)
        {
            case '1':
                Value = 10;
                Color = Color.Green;
                break;

            case '2':
                Value = 30;
                Color = Color.Yellow;
                break;

            case '3':
                Value = 50;
                Color = Color.Red;
                break;
        }

        collectedPosition = new Vector2(levelDimensions.X / 2, 20.0f);

        LoadContent();
    }

    /// <summary>
    /// Loads the gem texture and collected sound.
    /// </summary>
    public void LoadContent()
    {
        texture = Level.Content.Load<Texture2D>("Sprites/Gem");
        origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
    }

    /// <summary>
    /// Bounces up and down in the air to entice players to collect them.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        switch (State)
        {
            case GemState.Collected:
                break;

            case GemState.Collecting:
                if (basePosition.Y > collectedPosition.Y)
                {
                    // Move towards top centre of the screen.
                    Vector2 direction = collectedPosition - basePosition;
                    direction.Normalize();
                    basePosition += direction * 256 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    scale /= 1.010f;
                }

                if (basePosition.Y <= collectedPosition.Y)
                {
                    State = GemState.Collected;
                }
                break;

            case GemState.Waiting:
                // Bounce along a sine curve over time.
                // Include the X coordinate so that neighboring gems bounce in a nice wave pattern.
                double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
                bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
                scale = new Vector2(1.0f, 1.0f);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Called when this gem has been collected by a player and removed from the level.
    /// </summary>
    /// <param name="collectedBy">
    /// The player who collected this gem. Although currently not used, this parameter would be
    /// useful for creating special power-up gems. For example, a gem could make the player invincible.
    /// </param>
    public void OnCollected(Player collectedBy)
    {
        collectedSound.Play();
    }

    /// <summary>
    /// Draws a gem in the appropriate color.
    /// </summary>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, scale, SpriteEffects.None, 0.0f);
    }
}
