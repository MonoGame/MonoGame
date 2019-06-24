// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;

namespace Microsoft.Xna.Framework.Media
{
    public partial class MediaLibrary
    {
        private const string CacheFile = "MediaLibrary.cache";

        private static StorageFolder musicFolder;
        private static AlbumCollection albumCollection;
        private static SongCollection songCollection;

        private void PlatformLoad(Action<int> progressCallback)
        {
            Task.Run(async () =>
            {
                if (musicFolder == null)
                {
                    try
                    {
                        musicFolder = KnownFolders.MusicLibrary;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Failed to access Music Library: " + e.Message);
                        albumCollection = new AlbumCollection(new List<Album>());
                        songCollection = new SongCollection(new List<Song>());
                        return;
                    }
                }
                    
            
                var files = new List<StorageFile>();
                await this.GetAllFiles(musicFolder, files);

                var songList = new List<Song>();
                var albumList = new List<Album>();

                var artists = new Dictionary<string, Artist>();
                var albums = new Dictionary<string, Album>();
                var genres = new Dictionary<string, Genre>();

                var cache = new Dictionary<string, MusicProperties>();

                // Read cache
                var cacheFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(CacheFile, CreationCollisionOption.OpenIfExists);
                using (var baseStream = await cacheFile.OpenStreamForReadAsync())
                using (var stream = new BinaryReader(baseStream))
                    try
                    {
                        for (; baseStream.Position < baseStream.Length; )
                        {
                            var entry = MusicProperties.Deserialize(stream);
                            cache.Add(entry.Path, entry);
                        }
                    }
                    catch { }

                // Write cache
                cacheFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(CacheFile, CreationCollisionOption.ReplaceExisting);
                using (var stream = new BinaryWriter(await cacheFile.OpenStreamForWriteAsync()))
                {
                    int prevProgress = 0;

                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        try
                        {
                            MusicProperties properties;
                            if (!(cache.TryGetValue(file.Path, out properties) && properties.TryMatch(file)))
                                properties = new MusicProperties(file);
                            properties.Serialize(stream);

                            if (string.IsNullOrWhiteSpace(properties.Title))
                                continue;

                            Artist artist;
                            if (!artists.TryGetValue(properties.Artist, out artist))
                            {
                                artist = new Artist(properties.Artist);
                                artists.Add(artist.Name, artist);
                            }

                            Artist albumArtist;
                            if (!artists.TryGetValue(properties.AlbumArtist, out albumArtist))
                            {
                                albumArtist = new Artist(properties.AlbumArtist);
                                artists.Add(albumArtist.Name, albumArtist);
                            }

                            Genre genre;
                            if (!genres.TryGetValue(properties.Genre, out genre))
                            {
                                genre = new Genre(properties.Genre);
                                genres.Add(genre.Name, genre);
                            }

                            Album album;
                            if (!albums.TryGetValue(properties.Album, out album))
                            {
                                var thumbnail = Task.Run(async () => await properties.File.GetThumbnailAsync(ThumbnailMode.MusicView, 300, ThumbnailOptions.ResizeThumbnail)).Result;
                                album = new Album(new SongCollection(), properties.Album, albumArtist, genre, thumbnail.Type == ThumbnailType.Image ? thumbnail : null);
                                albums.Add(album.Name, album);
                                albumList.Add(album);
                            }

                            var song = new Song(album, artist, genre, properties);
                            song.Album.Songs.Add(song);
                            songList.Add(song);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("MediaLibrary exception: " + e.Message);
                        }

                        int progress = 100 * i / files.Count;
                        if (progress > prevProgress)
                        {
                            prevProgress = progress;
                            if (progressCallback != null)
                                progressCallback.Invoke(progress);
                        }
                    }
                }

                if (progressCallback != null)
                    progressCallback.Invoke(100);

                albumCollection = new AlbumCollection(albumList);
                songCollection = new SongCollection(songList);
            }).Wait();
        }

        private async Task GetAllFiles(StorageFolder storageFolder, List<StorageFile> musicFiles)
        {
            foreach (var item in await storageFolder.GetItemsAsync())
                if (item is StorageFile)
                {
                    var file = item as StorageFile;
                    if (file.ContentType.StartsWith("audio") && !file.ContentType.EndsWith("url"))
                        musicFiles.Add(file);
                }
                else
                {
                    var folder = item as StorageFolder;
                    await this.GetAllFiles(folder, musicFiles);
                }
        }

        private AlbumCollection PlatformGetAlbums()
        {
            return albumCollection;
        }

        private SongCollection PlatformGetSongs()
        {
            return songCollection;
        }

        private void PlatformDispose()
        {
            
        }
    }
}