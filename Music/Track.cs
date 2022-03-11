using System;
using System.Linq;

namespace Vibe.Music
{
    /// <summary>
    /// A song or piece of music.
    /// </summary>
    public sealed record Track
    {
        internal Track(long id, string path, string title, uint duration, long albumId, long artistId)
        {
            this.Id = id;
            this.Path = String.IsNullOrEmpty(path) ? throw new ArgumentException($"File path to a {nameof(Track)} cannot be {null} or empty", nameof(path)) : path;
            this.Title = String.IsNullOrEmpty(title) ? "Untitled track" : title;
            this.Duration = duration;
            this.albumId = albumId;
            this.artistId = artistId;
        }

        private readonly long albumId;

        private readonly long artistId;

        private Album? album;

        private Artist? artist;
        
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
                return this.album ??= Library.Albums.Single(album => (album.Id == this.albumId) && (album.Artist.Id == this.artistId));
            }
        }

        /// <summary>
        /// The <see cref="Artist"/> that released the <see cref="Track"/>.
        /// </summary>
        public Artist Artist
        {
            get
            {
                return this.artist ??= Library.Artists.Single(artist => artist.Id == this.artistId);
            }
        }
        
        public bool Equals(Track? track)
        {
            if (track is null)
            {
                return false;
            }
            return (this.Id == track.Id) && (this.albumId == track.albumId) && (this.artistId == track.artistId);
        }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }
    }
}