using System;
using System.Collections.Generic;
using Android.Content;
using Android.Provider;
using Android.Text.Format;
using Uri = Android.Net.Uri;

namespace Microsoft.Xna.Framework.Media
{
    public partial class MediaLibrary
    {
        internal static Context Context { get; set; }

        private static AlbumCollection albumCollection;
        private static SongCollection songCollection;

        private void PlatformLoad(Action<int> progressCallback)
        {
            List<Song> songList = new List<Song>();
            List<Album> albumList = new List<Album>();

            using (var musicCursor = Context.ContentResolver.Query(MediaStore.Audio.Media.ExternalContentUri, null, null, null, null))
            {
                if (musicCursor != null && musicCursor.MoveToFirst())
                {
                    Dictionary<string, Artist> artists = new Dictionary<string, Artist>();
                    Dictionary<string, Album> albums = new Dictionary<string, Album>();
                    Dictionary<string, Genre> genres = new Dictionary<string, Genre>();

                    int albumNameColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AlbumColumns.Album);
                    int albumArtistColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AlbumColumns.Artist);
                    int albumArtColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AlbumColumns.AlbumArt);
                    int genreColumn = musicCursor.GetColumnIndex(MediaStore.Audio.GenresColumns.Name);

                    int artistColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Artist);
                    int titleColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Title);
                    int durationColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Duration);
                    int assetIdColumn = musicCursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Id);

                    do
                    {
                        string albumNameProperty = musicCursor.GetString(albumNameColumn);
                        string albumArtistProperty = musicCursor.GetString(albumArtistColumn);
                        string albumArtProperty = musicCursor.GetString(albumArtColumn);
                        string genreProperty = musicCursor.GetString(genreColumn);

                        string artistProperty = musicCursor.GetString(artistColumn);
                        string titleProperty = musicCursor.GetString(titleColumn);
                        long durationProperty = musicCursor.GetLong(durationColumn);
                        TimeSpan duration = TimeSpan.FromMilliseconds(durationProperty);
                        long assetId = musicCursor.GetLong(assetIdColumn);
                        var assetUri = ContentUris.WithAppendedId(MediaStore.Audio.Media.ExternalContentUri, assetId);
                        
                        Artist artist;
                        if (!artists.TryGetValue(artistProperty, out artist))
                        {
                            artist = new Artist(artistProperty);
                            artists.Add(artist.Name, artist);
                        }

                        Artist albumArtist;
                        if (!artists.TryGetValue(albumArtistProperty, out albumArtist))
                        {
                            albumArtist = new Artist(albumArtistProperty);
                            artists.Add(albumArtist.Name, albumArtist);
                        }

                        Genre genre;
                        if (!genres.TryGetValue(genreProperty, out genre))
                        {
                            genre = new Genre(genreProperty);
                            genres.Add(genre.Name, genre);
                        }

                        Album album;
                        if (!albums.TryGetValue(albumNameProperty, out album))
                        {
                            album = new Album(new SongCollection(), albumNameProperty, albumArtist, genre, Uri.Parse(albumArtProperty));
                            albums.Add(album.Name, album);
                            albumList.Add(album);
                        }

                        var song = new Song(album, artist, genre, titleProperty, duration, assetUri);
                        song.Album.Songs.Add(song);
                        songList.Add(song);

                    } while (musicCursor.MoveToNext()); 
                }
            }

            albumCollection = new AlbumCollection(albumList);
            songCollection = new SongCollection(songList);
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
