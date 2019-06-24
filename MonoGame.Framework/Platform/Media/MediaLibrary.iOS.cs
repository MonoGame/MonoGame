// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Foundation;
using MediaPlayer;

namespace Microsoft.Xna.Framework.Media
{
    public partial class MediaLibrary
    {
        private static AlbumCollection albumCollection;
        private static SongCollection songCollection;

        private void PlatformLoad(Action<int> progressCallback)
        {
            var songList = new List<Song>();
            var albumList = new List<Album>();

            foreach (var collection in MPMediaQuery.AlbumsQuery.Collections)
            {
                var nsAlbumArtist = collection.RepresentativeItem.ValueForProperty(MPMediaItem.AlbumArtistProperty);
                var nsAlbumName = collection.RepresentativeItem.ValueForProperty(MPMediaItem.AlbumTitleProperty);
                var nsAlbumGenre = collection.RepresentativeItem.ValueForProperty(MPMediaItem.GenreProperty);
                string albumArtist = nsAlbumArtist == null ? "Unknown Artist" : nsAlbumArtist.ToString();
                string albumName = nsAlbumName == null ? "Unknown Album" : nsAlbumName.ToString();
                string albumGenre = nsAlbumGenre == null ? "Unknown Genre" : nsAlbumGenre.ToString();
                MPMediaItemArtwork thumbnail = collection.RepresentativeItem.ValueForProperty(MPMediaItem.ArtworkProperty) as MPMediaItemArtwork;

                var albumSongs = new List<Song>((int)collection.Count);
                var album = new Album(new SongCollection(albumSongs), albumName, new Artist(albumArtist), new Genre(albumGenre), thumbnail);
                albumList.Add(album);

                foreach (var item in collection.Items)
                {
                    var nsArtist = item.ValueForProperty(MPMediaItem.ArtistProperty);
                    var nsTitle = item.ValueForProperty(MPMediaItem.TitleProperty);
                    var nsGenre = item.ValueForProperty(MPMediaItem.GenreProperty);
                    var assetUrl = item.ValueForProperty(MPMediaItem.AssetURLProperty) as NSUrl;

                    if (nsTitle == null || assetUrl == null) // The Asset URL check will exclude iTunes match items from the Media Library that are not downloaded, but show up in the music app
                        continue;

                    string artist = nsArtist == null ? "Unknown Artist" : nsArtist.ToString();
                    string title = nsTitle.ToString();
                    string genre = nsGenre == null ? "Unknown Genre" : nsGenre.ToString();
                    TimeSpan duration = TimeSpan.FromSeconds(((NSNumber)item.ValueForProperty(MPMediaItem.PlaybackDurationProperty)).FloatValue);

                    var song = new Song(album, new Artist(artist), new Genre(genre), title, duration, item, assetUrl);
                    albumSongs.Add(song);
                    songList.Add(song);
                }
            }

            albumCollection = new AlbumCollection(albumList);
            songCollection = new SongCollection(songList);

            /*_playLists = new PlaylistCollection();
					
			MPMediaQuery playlists = new MPMediaQuery();
			playlists.GroupingType = MPMediaGrouping.Playlist;
            for (int i = 0; i < playlists.Collections.Length; i++)
            {
                MPMediaItemCollection item = playlists.Collections[i];
                Playlist list = new Playlist();
                list.Name = playlists.Items[i].ValueForProperty(MPMediaPlaylistPropertyName).ToString();
                for (int k = 0; k < item.Items.Length; k++)
                {
                    TimeSpan time = TimeSpan.Parse(item.Items[k].ValueForProperty(MPMediaItem.PlaybackDurationProperty).ToString());
                    list.Duration += time;
                }
                _playLists.Add(list);
            }*/
        }

        //private static readonly NSString MPMediaPlaylistPropertyName = new NSString(MPMediaPlaylistProperty.Name);

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
