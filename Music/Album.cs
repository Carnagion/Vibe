using System;
using System.Collections.Generic;
using System.Linq;

using Android.Graphics;
using Android.Media;

namespace Vibe.Music
{
    /// <summary>
    /// A collection of songs or pieces of music.
    /// </summary>
    public record Album
    {
        /// <summary>
        /// Initialises a new instance of <see cref="Album"/> with the provided values.
        /// </summary>
        /// <param name="id">A unique identifier.</param>
        /// <param name="title">The title of the <see cref="Album"/>.</param>
        /// <param name="artwork">The <see cref="Album"/>'s cover artwork.</param>
        /// <param name="tracks">The <see cref="Track"/>s contained in the <see cref="Album"/>.</param>
        public Album(long id, string title, Bitmap? artwork, IEnumerable<Track> tracks)
        {
            this.Id = id;
            this.Title = String.IsNullOrEmpty(title) ? "Untitled album" : title;
            this.Artwork = artwork;
            this.Tracks = tracks;
        }

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
                return this.artist ??= Library.Artists.First(artist => artist.Albums.Any(album => album.Id == this.Id)); //Single() and Contains(this) cause issues
            }
        }
    }
}