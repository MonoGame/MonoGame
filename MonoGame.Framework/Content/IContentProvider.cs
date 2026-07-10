using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework.Content;

/// <summary>
/// A content provider used for loading of remote content by <see cref="TitleContainer"/>.
/// </summary>
public interface IContentProvider
{
    /// <summary>
    /// Sends a request for the content to be acquired from the remote content server.
    /// </summary>
    /// <param name="relativePath">Relative path to the content.</param>
    /// <returns><c>true</c> if no unexpected error has occured, <c>false</c> otherwise.</returns>
    Task<bool> FetchContent(string relativePath);

    /// <summary>
    /// Opens the file stream that was acquired by <see cref="FetchContent"/> if its available.
    /// </summary>
    /// <param name="relativePath">Relative path to the content.</param>
    /// <returns>A valid <see cref="Stream"/> if the content was found, null otherwise.</returns>
    Stream OpenReadStream(string relativePath);
}
