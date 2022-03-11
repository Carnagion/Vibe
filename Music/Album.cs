using System;
using System.Collections.Generic;
using System.Linq;

using Android.Graphics;
using Android.Util;

namespace Vibe.Music
{
    /// <summary>
    /// A collection of songs or pieces of music.
    /// </summary>
    public sealed record Album
    {
        internal Album(long id, string title, Bitmap? artwork, IEnumerable<Track> tracks, long artistId)
        {
            this.Id = id;
            this.Title = String.IsNullOrEmpty(title) ? "Untitled album" : title;
            this.Artwork = artwork;
            this.Tracks = tracks;
            this.artistId = artistId;
        }

        private readonly long artistId;

        private Artist? artist;
        
        /// <summary>
        /// The <see cref="Album"/>'s unique identifier.
        /// </summary>
        public long Id
        {
            get;
        }

        /// <summary>
        /// The <see cref="Album"/>'s title.
        /// </summary>t
        public string Title
        {
            get;
        }

        /// <summary>
        /// The <see cref="Album"/>'s cover artwork.
        /// </summary>
        public Bitmap? Artwork
        {
            get;
        }
        
        /// <summary>
        /// All the <see cref="Track"/>s in the <see cref="Album"/>.
        /// </summary>
        public IEnumerable<Track> Tracks
        {
            get;
        }
        
        /// <summary>
        /// The <see cref="Artist"/> that released the <see cref="Album"/>.
        /// </summary>
        public Artist Artist
        {
            get
            {
                return this.artist ??= Library.Artists.Single(artist => artist.Id == this.artistId);
            }
        }
        
        public bool Equals(Album? album)
        {
            if (album is null)
            {
                return false;
            }
            return (this.Id == album.Id) && (this.artistId == album.artistId);
        }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }
    }
}