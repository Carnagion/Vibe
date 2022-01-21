using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Provider;

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

        public static IEnumerable<Playlist> Playlists
        {
            get
            {
                return Library.database.Playlists.Copy();
            }
        }

        public static IEnumerable<Compilation> Compilations
        {
            get
            {
                return Library.database.Compilations.Copy();
            }
        }

        /// <summary>
        /// Clears any stored <see cref="Artist"/>s, <see cref="Album"/>s, and <see cref="Track"/>s, then scans for any media files in the device and adds them to the database.
        /// </summary>
        public static void BuildDatabase()
        {
            Library.database.Artists.Clear();
            Library.database.Playlists.Clear();
            Library.database.Compilations.Clear();
            
            Library.cache = new(Application.Context);
            Library.cache.ConvertToUsableData().Execute(artist => Library.database.Artists.Add(artist));
        }

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
            (MusicDataQuery removed, MusicDataQuery changed, MusicDataQuery missing) = MusicDataQuery.DifferenceBetween(Library.cache, updated);
            
            removed.ConvertToUsableData().Execute(artist => Library.database.Artists.Add(artist));
            changed.ConvertToUsableData().Execute(artist =>
            {
                Library.database.Artists.RemoveWhere(existing => existing.Id == artist.Id);
                Library.database.Artists.Add(artist);
            });
            missing.ConvertToUsableData().Execute(artist => Library.database.Artists.Add(artist));

            Library.cache = updated;
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
                
                ICursor? cursor = context.ContentResolver?.Query(MediaStore.Audio.Media.ExternalContentUri!, columns, null, null, null);
                if (cursor is null)
                {
                    return;
                }
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

                    (long id, string name) artist = (artistId, artistName);
                    (long id, string title) album = (albumId, albumTitle);
                    (long id, string path, string title, uint duration, int index) track = (trackId, trackPath, trackTitle, trackDuration, trackIndex);

                    Dictionary<(long id, string name), Dictionary<(long id, string title), List<(long id, string path, string title, uint duration, int index)>>> artists = this.data;
                    if (!artists.ContainsKey(artist))
                    {
                        artists.Add(artist, new());
                    }
                    Dictionary<(long id, string title), List<(long id, string path, string title, uint duration, int index)>> albums = artists[artist];
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

            private readonly Dictionary<(long id, string name), Dictionary<(long id, string title), List<(long id, string path, string title, uint duration, int index)>>> data = new();
            
            internal static (MusicDataQuery removed, MusicDataQuery changed, MusicDataQuery missing) DifferenceBetween(MusicDataQuery before, MusicDataQuery after)
            {
                (MusicDataQuery removed, MusicDataQuery changed, MusicDataQuery missing) difference = new(new(), new(), new());
                
                (from entry in before.data
                 where !after.data.ContainsKey(entry.Key)
                 select entry).Execute(entry => difference.removed.data.Add(entry.Key, entry.Value));
                
                (from entry in after.data
                 where !before.data.ContainsKey(entry.Key)
                 select entry).Execute(entry => difference.missing.data.Add(entry.Key, entry.Value));
                
                (from entry in after.data
                 where before.data.ContainsKey(entry.Key) && !before.data[entry.Key].Equals(entry.Value)
                 select entry).Execute(entry => difference.changed.data.Add(entry.Key, entry.Value));
                 
                return difference;
            }
            
            internal IEnumerable<Artist> ConvertToUsableData()
            {
                return from artistEntry in this.data
                       let albums = from albumEntry in artistEntry.Value
                                    let tracks = from trackEntry in albumEntry.Value
                                                 orderby trackEntry.index
                                                 select new Track(trackEntry.id, trackEntry.path, trackEntry.title, trackEntry.duration)
                                    select new Album(albumEntry.Key.id, albumEntry.Key.title, tracks)
                       select new Artist(artistEntry.Key.id, artistEntry.Key.name, albums);
            }
        }
    }
}