using System;
using System.Collections.Generic;
using System.Linq;

namespace Vibe.Music
{
    /// <summary>
    /// An individual or group that creates music.
    /// </summary>
    public sealed record Artist
    {
        internal Artist(long id, string name, IEnumerable<Album> albums)
        {
            this.Id = id;
            this.Name = String.IsNullOrEmpty(name) ? "Unnamed artist" : name;
            this.Albums = albums;
            this.Tracks = from album in this.Albums
                          from track in album.Tracks
                          select track;
        }

        /// <summary>
        /// The <see cref="Artist"/>'s unique identifier.
        /// </summary>
        public long Id
        {
            get;
        }

        /// <summary>
        /// The <see cref="Artist"/>'s name.
        /// </summary>
        public string Name
        {
            get;
        }
        
        /// <summary>
        /// All the <see cref="Album"/>s of the <see cref="Artist"/>.
        /// </summary>
        public IEnumerable<Album> Albums
        {
            get;
        }
        
        /// <summary>
        /// All the <see cref="Track"/>s of the <see cref="Artist"/>.
        /// </summary>
        public IEnumerable<Track> Tracks
        {
            get;
        }
        
        public bool Equals(Artist? artist)
        {
            if (artist is null)
            {
                return false;
            }
            return this.Id == artist.Id;
        }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }
    }
}