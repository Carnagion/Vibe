using System;
using System.Linq;

namespace Vibe.Music
{
    /// <summary>
    /// A song or piece of music.
    /// </summary>
    public record Track
    {
        /// <summary>
        /// Initialises a new instance of <see cref="Track"/> with the provided values.
        /// </summary>
        /// <param name="id">A unique identifier.</param>
        /// <param name="path">The absolute file path to the track.</param>
        /// <param name="title">The title of the track.</param>
        /// <param name="duration">The duration of the track.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="path"/> is not a valid file path.</exception>
        public Track(long id, string path, string title, uint duration)
        {
            this.Id = id;
            this.Path = String.IsNullOrEmpty(path) ? throw new ArgumentException($"File path to a {nameof(Track)} cannot be {null} or empty", nameof(path)) : path;
            this.Title = String.IsNullOrEmpty(title) ? "Untitled track" : title;
            this.Duration = duration;
        }

        private Album? album;
        
        /// <summary>
        /// The <see cref="Track"/>'s unique identifier.
        /// </summary>
        public long Id
        {
            get;
        }

        /// <summary>
        /// The <see cref="Track"/>'s title.
        /// </summary>
        public string Title
        {
            get;
        }

        /// <summary>
        /// The file path that points to the location of the <see cref="Track"/>.
        /// </summary>
        public string Path
        {
            get;
        }

        /// <summary>
        /// The duration of the <see cref="Track"/>, in seconds.
        /// </summary>
        public uint Duration
        {
            get;
        }
        
        /// <summary>
        /// The <see cref="Album"/> that contains the <see cref="Track"/>.
        /// </summary>
        public Album Album
        {
            get
            {
                return this.album ??= Library.Albums.First(album => album.Tracks.Any(track => track.Id == this.Id)); //Single() and Contains(this) cause issues for some reason
            }
        }

        /// <summary>
        /// The <see cref="Artist"/> that released the <see cref="Track"/>.
        /// </summary>
        public Artist Artist
        {
            get
            {
                return this.Album.Artist;
            }
        }
    }
}