// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Net.Sockets;
using MonoGame.Framework.Content;

namespace MonoGame.Framework.DevTools;

/// <summary>
/// An <see cref="IContentProvider"/> used for loading content over a network.
/// </summary>
public class NetworkContentProvider : IContentProvider
{
    private const string ModifiedTimesFilename = "ModifiedTimes.yaml";
    private readonly HttpClient _client;
    private readonly Dictionary<string, long> _modifiedTimes;
    private readonly string _location;

    public NetworkContentProvider()
    {
        _client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsLinux())
        {
            _location = AppDomain.CurrentDomain.BaseDirectory;
        }
        else
        {
            _location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MonoGameCache");
        }
        _modifiedTimes = LoadModifiedTimes();
    }

    /// <summary>
    /// The location of the content server for loading assets.
    /// </summary>
    /// <value><c>http://localhost:7771/</c> by default.</value>
    public required string Address { get; init; } = "http://localhost:7771/";

    public async Task<bool> FetchContent(string relativePath)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Path", relativePath);
        if (_modifiedTimes.TryGetValue(relativePath, out var lastModifiedTime))
        {
            var absolutePath = Path.Combine(_location, relativePath);
            if (File.Exists(absolutePath)) // TODO: Maybe also cache information about the write time of the file and use that as well
            {
                _client.DefaultRequestHeaders.Add("LastModifiedTime", lastModifiedTime.ToString());
            }
        }

        try
        {
            await using var stream = await _client.GetStreamAsync(Address);
            var lastModifiedTimeBytes = new byte[sizeof(long)];
            var readBytes = await stream.ReadAsync(lastModifiedTimeBytes);
            if (readBytes != lastModifiedTimeBytes.Length)
            {
                // if we just received a 0 that means the server does not have the file
                // no error has occured while processing this request so return true
                // and let the client try to find the file in its storage.
                return readBytes == 1 && lastModifiedTimeBytes[0] == 0;
            }

            await using var fileStream = OpenWriteStream(relativePath);
            if (fileStream == null)
            {
                return false;
            }

            await stream.CopyToAsync(fileStream);

            _modifiedTimes[relativePath] = BitConverter.ToInt64(lastModifiedTimeBytes);
            SaveModifiedTimes();

            return true;
        }
        catch (HttpRequestException)
        {
            return true;
        }
        catch (SocketException)
        {
            return true;
        }
        catch { }

        return false;
    }

    public Stream? OpenReadStream(string relativePath)
    {
        var absolutePath = Path.Combine(_location, relativePath);
        return File.Exists(absolutePath) ? File.OpenRead(absolutePath) : null;
    }

    private Dictionary<string, long> LoadModifiedTimes()
    {
        using var fileStream = OpenReadStream(ModifiedTimesFilename);
        if (fileStream == null)
        {
            return [];
        }

        var ret = new Dictionary<string, long>();
        using var reader = new StreamReader(fileStream);
        foreach (var line in reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var split = line.Trim().Split(": ");
            if (split.Length == 2)
            {
                ret[split[0]] = long.Parse(split[1]);
            }
        }

        return ret;
    }

    private void SaveModifiedTimes()
    {
        using var fileStream = OpenWriteStream(ModifiedTimesFilename);
        if (fileStream == null)
        {
            return;
        }

        using var streamWriter = new StreamWriter(fileStream);
        foreach (var pair in _modifiedTimes)
        {
            streamWriter.WriteLine($"{pair.Key}: {pair.Value}");
        }
    }

    private FileStream? OpenWriteStream(string relativePath)
    {
        var absolutePath = Path.Combine(_location, relativePath);
        var dirPath = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        return File.OpenWrite(absolutePath);
    }
}
