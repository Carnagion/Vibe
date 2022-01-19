using System;
using System.Collections.Generic;
using System.Linq;

namespace Vibe.Music
{
    /// <summary>
    /// An individual or group that creates music.
    /// </summary>
    public record Artist
    {
        /// <summary>
        /// Initialises a new instance of <see cref="Artist"/> with the provided values.
        /// </summary>
        /// <param name="id">A unique identifier.</param>
        /// <param name="name">The name of the <see cref="Artist"/>.</param>
        /// <param name="albums">The <see cref="Album"/>s released by the <see cref="Artist"/>.</param>
        public Artist(long id, string name, IEnumerable<Album> albums)
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
    }
}