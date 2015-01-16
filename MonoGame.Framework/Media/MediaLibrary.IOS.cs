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
            MPMediaQuery mediaQuery = new MPMediaQuery();
            var value = NSObject.FromObject(MPMediaType.Music);
            var type = MPMediaItem.MediaTypeProperty;
            var predicate = MPMediaPropertyPredicate.PredicateWithValue(value, type);
            mediaQuery.AddFilterPredicate(predicate);
            mediaQuery.GroupingType = MPMediaGrouping.Album;

            List<Song> songList = new List<Song>();
            List<Album> albumList = new List<Album>();

            for (int i = 0; i < mediaQuery.Collections.Length; i++)
            {
                MPMediaItemCollection itemCollection = mediaQuery.Collections[i];
				List<Song> albumSongs = new List<Song>((int)itemCollection.Count);

                var nsAlbumArtist = itemCollection.RepresentativeItem.ValueForProperty(MPMediaItem.AlbumArtistProperty);
                var nsAlbumName = itemCollection.RepresentativeItem.ValueForProperty(MPMediaItem.AlbumTitleProperty);
                var nsAlbumGenre = itemCollection.RepresentativeItem.ValueForProperty(MPMediaItem.GenreProperty);
                string albumArtist = nsAlbumArtist == null ? "Unknown Artist" : nsAlbumArtist.ToString();
                string albumName = nsAlbumName == null ? "Unknown Album" : nsAlbumName.ToString();
                string albumGenre = nsAlbumGenre == null ? "Unknown Genre" : nsAlbumGenre.ToString();
                MPMediaItemArtwork thumbnail = itemCollection.RepresentativeItem.ValueForProperty(MPMediaItem.ArtworkProperty) as MPMediaItemArtwork;

                var album = new Album(new SongCollection(albumSongs), albumName, new Artist(albumArtist), new Genre(albumGenre), thumbnail);
                albumList.Add(album);

                for (int j = 0; j < itemCollection.Count; j++)
                {
                    var nsArtist = itemCollection.Items[j].ValueForProperty(MPMediaItem.ArtistProperty);
                    var nsTitle = itemCollection.Items[j].ValueForProperty(MPMediaItem.TitleProperty);
                    var nsGenre = itemCollection.Items[j].ValueForProperty(MPMediaItem.GenreProperty);
                    var assetUrl = itemCollection.Items[j].ValueForProperty(MPMediaItem.AssetURLProperty) as NSUrl;

                    if (nsTitle == null || assetUrl == null) // The Asset URL check will exclude iTunes match items from the Media Library that are not downloaded, but show up in the music app
                        continue;

                    string artist = nsArtist == null ? "Unknown Artist" : nsArtist.ToString();
                    string title = nsTitle.ToString();
                    string genre = nsGenre == null ? "Unknown Genre" : nsGenre.ToString();
                    TimeSpan duration = TimeSpan.FromSeconds(((NSNumber)itemCollection.Items[j].ValueForProperty(MPMediaItem.PlaybackDurationProperty)).FloatValue);

                    var song = new Song(album, new Artist(artist), new Genre(genre), title, duration, itemCollection.Items[j], assetUrl);
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
