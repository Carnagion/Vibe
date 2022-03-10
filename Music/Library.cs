using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Provider;

using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace Vibe.Music
{
    /// <summary>
    /// Gathers and stores information about all music on the device.
    /// </summary>
    public static class Library
    {
        private static MusicDataQuery? cache;

        private static readonly Compilation database = new();

        /// <summary>
        /// All the <see cref="Artist"/>s in <see cref="Library"/>'s database.
        /// </summary>
        public static IEnumerable<Artist> Artists
        {
            get
            {
                return Library.database.Artists.Copy();
            }
        }

        /// <summary>
        /// All the <see cref="Album"/>s in <see cref="Library"/>'s database.
        /// </summary>
        public static IEnumerable<Album> Albums
        {
            get
            {
                return from artist in Library.database.Artists
                       from album in artist.Albums
                       select album;
            }
        }

        /// <summary>
        /// All the <see cref="Track"/>s in <see cref="Library"/>'s database.
        /// </summary>
        public static IEnumerable<Track> Tracks
        {
            get
            {
                return from artist in Library.database.Artists
                       from album in artist.Albums
                       from track in album.Tracks
                       select track;
            }
        }

        /// <summary>
        /// All the <see cref="Playlist"/>s in <see cref="Library"/>'s database.
        /// </summary>
        public static HashSet<Playlist> Playlists
        {
            get
            {
                return Library.database.Playlists;
            }
        }

        /// <summary>
        /// All the <see cref="Compilation"/>s in <see cref="Library"/>'s database.
        /// </summary>
        public static HashSet<Compilation> Compilations
        {
            get
            {
                return Library.database.Compilations;
            }
        }

        private static string PlaylistsFilePath
        {
            get;
        } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Playlists.xml");

        /// <summary>
        /// Searches the device for changes to media files, updating the stored <see cref="Artist"/>s, <see cref="Album"/>s, and <see cref="Track"/>s as necessary.
        /// </summary>
        public static void UpdateDatabase()
        {
            if (Library.cache is null)
            {
                Library.BuildDatabase();
                return;
            }
            
            MusicDataQuery updated = new(Application.Context);
            (MusicDataQuery removed, MusicDataQuery changed, MusicDataQuery added) = MusicDataQuery.DifferenceBetween(Library.cache, updated);
            Library.cache = updated;
            
            removed.ConvertToUsableData().ForEach(artist => Library.database.Artists.RemoveWhere(existing => existing.Id == artist.Id));
            changed.ConvertToUsableData().ForEach(artist =>
            {
                Library.database.Artists.RemoveWhere(existing => existing.Id == artist.Id);
                Library.database.Artists.Add(artist);
            });
            added.ConvertToUsableData().ForEach(artist => Library.database.Artists.Add(artist));

            Library.FetchStoredPlaylists().ForEach(playlist => Library.database.Playlists.Add(playlist));
        }

        internal static void SavePersistentData()
        {
            Playlist playlist = new(Library.Tracks.Random().Yield().Append(Library.Tracks.Random()))
            {
                Title = "Test playlist",
            };
            Library.database.Playlists.Add(playlist);
            Library.StorePlaylists();
        }

        private static void BuildDatabase()
        {
            Library.database.Artists.Clear();
            Library.database.Playlists.Clear();
            Library.database.Compilations.Clear();
            
            Library.cache = new(Application.Context);
            Library.cache.ConvertToUsableData().ForEach(artist => Library.database.Artists.Add(artist));
            
            Library.FetchStoredPlaylists().ForEach(playlist => Library.database.Playlists.Add(playlist));
        }

        private static IEnumerable<Playlist> FetchStoredPlaylists()
        {
            if (!File.Exists(Library.PlaylistsFilePath))
            {
                return Enumerable.Empty<Playlist>();
            }

            try
            {
                XmlDocument document = new();
                document.LoadXml(File.ReadAllText(Library.PlaylistsFilePath));

                XmlNode root = document.SelectSingleNode("Playlists")!;
                XmlNodeList? playlistNodes = root.SelectNodes("Playlist");
                if (playlistNodes is null)
                {
                    return Enumerable.Empty<Playlist>();
                }

                return from XmlNode playlistNode in playlistNodes
                       select new Playlist(from trackNode in playlistNode.SelectNodes("Track")!.Cast<XmlNode>()
                                           let id = Int64.Parse(trackNode.InnerText)
                                           let track = (from existing in Library.Tracks
                                                        where existing.Id == id
                                                        select existing).FirstOrDefault()
                                           where track is not null
                                           select track)
                       {
                           Title = playlistNode.Attributes!["Title"].Value,
                       };
            }
            catch
            {
                return Enumerable.Empty<Playlist>();
            }
        }

        private static async void StorePlaylists()
        {
            StringBuilder builder = new();
            builder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n\r\n");
            builder.Append("<Playlists>\r\n");
            foreach (Playlist playlist in Library.database.Playlists)
            {
                builder.Append($"\t<Playlist Title=\"{playlist.Title}\">\r\n");
                foreach (Track track in playlist)
                {
                    builder.Append($"\t\t<Track>{track.Id}</Track>\r\n");
                }
                builder.Append("\t</Playlist>\r\n");
            }
            builder.Append("</Playlists>");
            
            await File.WriteAllTextAsync(Library.PlaylistsFilePath, builder.ToString());
        }

        private sealed record MusicDataQuery
        {
            internal MusicDataQuery(Context context)
            {
                string[] columns = {
                    MediaStore.Audio.Media.InterfaceConsts.IsMusic,
#pragma warning disable 618
                    MediaStore.Audio.Media.InterfaceConsts.Data,
#pragma warning restore 618
                    MediaStore.Audio.Media.InterfaceConsts.Title,
                    MediaStore.Audio.Media.InterfaceConsts.CdTrackNumber,
                    MediaStore.Audio.Media.InterfaceConsts.Duration,
                    MediaStore.Audio.Media.InterfaceConsts.Id,
                    MediaStore.Audio.Media.InterfaceConsts.Album,
                    MediaStore.Audio.Media.InterfaceConsts.AlbumId,
                    MediaStore.Audio.Media.InterfaceConsts.Artist,
                    MediaStore.Audio.Media.InterfaceConsts.ArtistId,
                };
                
                using ICursor? cursor = context.ContentResolver?.Query(MediaStore.Audio.Media.ExternalContentUri!, columns, null, null, null);
                if (cursor is null)
                {
                    return;
                }
                Dictionary<long, Bitmap?> loadedArtworks = new();
                while (cursor.MoveToNext())
                {
                    if (cursor.GetString(0) is not "1")
                    {
                        continue;
                    }

                    string trackPath = cursor.GetString(1)!;
                    string trackTitle = cursor.GetString(2)!;
                    int trackIndex = cursor.GetInt(3);
                    uint trackDuration = (uint)(cursor.GetFloat(4) / 1000);
                    long trackId = cursor.GetLong(5);
                    string albumTitle = cursor.GetString(6)!;
                    long albumId = cursor.GetLong(7);
                    string artistName = cursor.GetString(8)!;
                    long artistId = cursor.GetLong(9);

                    using Uri albumArtUri = ContentUris.WithAppendedId(MediaStore.Audio.Media.ExternalContentUri!, trackId);
                    if (!loadedArtworks.TryGetValue(albumId, out Bitmap? albumArt))
                    {
                        try
                        {
                            albumArt = context.ContentResolver!.LoadThumbnail(albumArtUri, new(256, 256), null);
                        }
                        catch
                        {
                            albumArt = null;
                        }
                        loadedArtworks[albumId] = albumArt;
                    }

                    (long id, string name) artist = (artistId, artistName);
                    (long id, string title, Bitmap? cover) album = (albumId, albumTitle, albumArt);
                    (long id, string path, string title, uint duration, int index) track = (trackId, trackPath, trackTitle, trackDuration, trackIndex);

                    Dictionary<(long id, string name), Dictionary<(long id, string title, Bitmap? cover), List<(long id, string path, string title, uint duration, int index)>>> artists = this.data;
                    if (!artists.ContainsKey(artist))
                    {
                        artists.Add(artist, new());
                    }
                    Dictionary<(long id, string title, Bitmap? cover), List<(long id, string path, string title, uint duration, int index)>> albums = artists[artist];
                    if (!albums.ContainsKey(album))
                    {
                        albums.Add(album, new());
                    }
                    List<(long id, string path, string title, uint duration, int index)> tracks = albums[album];
                    tracks.Add(track);
                }
                cursor.Close();
            }
            
            private MusicDataQuery()
            {
            }

            private readonly Dictionary<(long id, string name), Dictionary<(long id, string title, Bitmap? cover), List<(long id, string path, string title, uint duration, int index)>>> data = new();
            
            internal static (MusicDataQuery removed, MusicDataQuery changed, MusicDataQuery added) DifferenceBetween(MusicDataQuery before, MusicDataQuery after)
            {
                (MusicDataQuery removed, MusicDataQuery changed, MusicDataQuery added) difference = new(new(), new(), new());
                
                (from entry in before.data
                 where !after.data.ContainsKey(entry.Key)
                 select entry).ForEach(entry => difference.removed.data.Add(entry.Key, entry.Value));
                
                (from entry in after.data
                 where !before.data.ContainsKey(entry.Key)
                 select entry).ForEach(entry => difference.added.data.Add(entry.Key, entry.Value));
                
                (from entry in after.data
                 where before.data.ContainsKey(entry.Key) && !before.data[entry.Key].Equals(entry.Value)
                 select entry).ForEach(entry => difference.changed.data.Add(entry.Key, entry.Value));
                 
                return difference;
            }
            
            internal IEnumerable<Artist> ConvertToUsableData()
            {
                return from artistEntry in this.data
                       let albums = from albumEntry in artistEntry.Value
                                    let tracks = from trackEntry in albumEntry.Value
                                                 orderby trackEntry.index
                                                 select new Track(trackEntry.id, trackEntry.path, trackEntry.title, trackEntry.duration)
                                    select new Album(albumEntry.Key.id, albumEntry.Key.title, albumEntry.Key.cover, tracks)
                       select new Artist(artistEntry.Key.id, artistEntry.Key.name, albums);
            }
        }
    }
}