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
        private static readonly Compilation database = new();

        private static readonly string playlistsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Playlists.xml");

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

        /// <summary>
        /// Searches the device for changes to media files, updating the stored <see cref="Artist"/>s, <see cref="Album"/>s, and <see cref="Track"/>s.
        /// </summary>
        public static void UpdateDatabase()
        {
            Library.database.Artists.Clear();
            Library.database.Playlists.Clear();
            Library.database.Compilations.Clear();
            
            new MusicDataQuery(Application.Context).ToUsableData().ForEach(artist => Library.database.Artists.Add(artist));
            
            Library.FetchStoredPlaylists().ForEach(playlist => Library.database.Playlists.Add(playlist));
        }

        private static IEnumerable<Playlist> FetchStoredPlaylists()
        {
            if (!File.Exists(Library.playlistsFilePath))
            {
                return Enumerable.Empty<Playlist>();
            }

            try
            {
                XmlDocument document = new();
                document.LoadXml(File.ReadAllText(Library.playlistsFilePath));

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
            
            await File.WriteAllTextAsync(Library.playlistsFilePath, builder.ToString());
        }

        private sealed record MusicDataQuery
        {
            internal MusicDataQuery(Context context)
            {
                string[] columns =
                {
                    MediaStore.Audio.Media.InterfaceConsts.IsMusic,
#pragma warning disable 618
                    MediaStore.Audio.Media.InterfaceConsts.Data, // Marked as obsolete but still works, and nothing else works. I have no idea why. Good job, Android
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
                    if (!loadedArtworks.TryGetValue(trackId, out Bitmap? albumArt)) // The track ID is used because it is unique, as opposed to album ID which can be duplicate for reasons I fail to understand. Why must you do this, Android
                    {
                        try
                        {
                            albumArt = context.ContentResolver!.LoadThumbnail(albumArtUri, new(256, 256), null);
                        }
                        catch
                        {
                            albumArt = null;
                        }
                        loadedArtworks[trackId] = albumArt;
                    }

                    if (!this.artistsInfo.TryGetValue(artistId, out _))
                    {
                        this.artistsInfo[artistId] = (artistName, new());
                    }
                    this.artistsInfo[artistId].albums.Add(albumId);
                    if (!this.albumsInfo.TryGetValue((artistId, albumId), out _))
                    {
                        this.albumsInfo[(artistId, albumId)] = (albumTitle, albumArt, new());
                    }
                    this.albumsInfo[(artistId, albumId)].tracks.Add(trackId);
                    this.tracksInfo[trackId] = (trackPath, trackTitle, trackDuration, trackIndex);
                }
                cursor.Close();
            }

            private readonly Dictionary<long, (string name, HashSet<long> albums)> artistsInfo = new();

            private readonly Dictionary<(long artistId, long albumId), (string title, Bitmap? artwork, HashSet<long> tracks)> albumsInfo = new(); // Both artist ID and album ID are used to uniquely identify an album since album IDs aren't unique for some reason. Excellent backend design, Android devs

            private readonly Dictionary<long, (string path, string title, uint duration, int index)> tracksInfo = new();
            
            internal IEnumerable<Artist> ToUsableData()
            {
                // Be not afraid
                return from artistEntry in this.artistsInfo
                       let albums = from albumId in artistEntry.Value.albums
                                    let albumEntry = this.albumsInfo[(artistEntry.Key, albumId)]
                                    let tracks = from trackId in albumEntry.tracks
                                                 let trackEntry = this.tracksInfo[trackId]
                                                 orderby trackEntry.index
                                                 select new Track(trackId, trackEntry.path, trackEntry.title, trackEntry.duration, albumId, artistEntry.Key)
                                    select new Album(albumId, albumEntry.title, albumEntry.artwork, tracks, artistEntry.Key)
                       select new Artist(artistEntry.Key, artistEntry.Value.name, albums);
            }
        }
    }
}