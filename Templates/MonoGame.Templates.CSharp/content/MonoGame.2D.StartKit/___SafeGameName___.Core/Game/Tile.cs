using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ___SafeGameName___.Core;

/// <summary>
/// Stores the appearance and collision behavior of a tile.
/// </summary>
struct Tile
{
    public Texture2D Texture;
    public TileCollision Collision;

    public const int Width = 40;
    public const int Height = 32;

    public static readonly Vector2 Size = new Vector2(Width, Height);

    /// <summary>
    /// Constructs a new tile.
    /// </summary>
    public Tile(Texture2D texture, TileCollision collision)
    {
        Texture = texture;
        Collision = collision;
    }
}