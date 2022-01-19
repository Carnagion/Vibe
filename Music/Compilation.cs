using System;
using System.Collections.Generic;
using System.Linq;

namespace Vibe.Music
{
    /// <summary>
    /// A collection of <see cref="Artist"/>s, <see cref="Album"/>s, <see cref="Track"/>s, <see cref="Playlist"/>s, and other <see cref="Compilation"/>s themselves.
    /// </summary>
    public class Compilation
    {
        private string title = "Untitled compilation";

        public HashSet<Compilation> Compilations
        {
            get;
        } = new();

        public HashSet<Playlist> Playlists
        {
            get;
        } = new();

        public HashSet<Artist> Artists
        {
            get;
        } = new();

        public HashSet<Album> Albums
        {
            get;
        } = new();

        public HashSet<Track> Tracks
        {
            get;
        } = new();

        /// <summary>
        /// The <see cref="Compilation"/>'s title.
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    this.title = value;
                }
            }
        }

        /// <summary>
        /// All the <see cref="Compilation"/>s contained in the <see cref="Compilation"/>, including sub-<see cref="Compilation"/>s of <see cref="Compilation"/>s.
        /// </summary>
        public IEnumerable<Compilation> AllCompilations
        {
            get
            {
                foreach (Compilation compilation in this.Compilations)
                {
                    yield return compilation;
                    foreach (Compilation subCompilation in compilation.AllCompilations)
                    {
                        yield return subCompilation;
                    }
                }
            }
        }

        /// <summary>
        /// All the <see cref="Playlist"/>s contained in the <see cref="Compilation"/>, including <see cref="Playlist"/>s in sub-<see cref="Compilation"/>s.
        /// </summary>
        public IEnumerable<Playlist> AllPlaylists
        {
            get
            {
                foreach (Playlist playlist in this.Playlists)
                {
                    yield return playlist;
                }
                foreach (Playlist playlist in from Compilation compilation in this.AllCompilations
                                              from Playlist playlist in compilation.Playlists
                                              select playlist)
                {
                    yield return playlist;
                }
            }
        }

        /// <summary>
        /// All the <see cref="Artist"/>s contained in the <see cref="Compilation"/>, including <see cref="Artist"/>s in sub-<see cref="Compilation"/>s.
        /// </summary>
        public IEnumerable<Artist> AllArtists
        {
            get
            {
                foreach (Artist artist in this.Artists)
                {
                    yield return artist;
                }
                foreach (Artist artist in from Compilation compilation in this.AllCompilations
                                          from Artist artist in compilation.Artists
                                          select artist)
                {
                    yield return artist;
                }
            }
        }

        /// <summary>
        /// All the <see cref="Album"/>s contained in the <see cref="Compilation"/>, including <see cref="Album"/>s in sub-<see cref="Compilation"/>s and their <see cref="Artist"/>s.
        /// </summary>
        public IEnumerable<Album> AllAlbums
        {
            get
            {
                foreach (Album album in this.Albums)
                {
                    yield return album;
                }
                foreach (Album album in from Compilation compilation in this.AllCompilations
                                        from Album album in compilation.Albums
                                        select album)
                {
                    yield return album;
                }
                foreach (Album album in from Artist artist in this.AllArtists
                                        from Album album in artist.Albums
                                        select album)
                {
                    yield return album;
                }
            }
        }

        /// <summary>
        /// All the <see cref="Track"/>s contained in the <see cref="Compilation"/>, including <see cref="Track"/>s in sub-<see cref="Compilation"/>s, their <see cref="Playlist"/>s, their <see cref="Artist"/>s, and their <see cref="Album"/>s.
        /// </summary>
        public IEnumerable<Track> AllTracks
        {
            get
            {
                foreach (Track track in this.Tracks)
                {
                    yield return track;
                }
                foreach (Track track in from Playlist playlist in this.AllPlaylists
                                        from Track track in playlist
                                        select track)
                {
                    yield return track;
                }
                foreach (Track track in from Album album in this.AllAlbums
                                        from Track track in album.Tracks
                                        select track)
                {
                    yield return track;
                }
            }
        }
    }
}