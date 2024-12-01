namespace ___SafeGameName___.Core;

/// <summary>
/// Controls the collision detection and response behavior of a tile.
/// </summary>
enum TileCollision
{
    /// <summary>
    /// A passable tile is one which does not hinder player motion at all.
    /// </summary>
    Passable = 0,

    /// <summary>
    /// An impassable tile is one which does not allow the player to move through
    /// it at all. It is completely solid.
    /// </summary>
    Impassable = 1,

    /// <summary>
    /// A platform tile is one which behaves like a passable tile except when the
    /// player is above it. A player can jump up through a platform as well as move
    /// past it to the left and right, but can not fall down through the top of it.
    /// </summary>
    Platform = 2,

    /// <summary>
    /// A breakable tile is one which behaves like a platform tile except when the
    /// player is below it, the player jumps up the tile breaks/disappears.
    /// Our version of Mario :).
    /// </summary>
    Breakable = 3,
}