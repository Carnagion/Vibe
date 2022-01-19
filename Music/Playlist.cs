using System;
using System.Collections.Generic;

namespace Vibe.Music
{
    /// <summary>
    /// An ordered list of <see cref="Track"/>s.
    /// </summary>
    public class Playlist : List<Track>
    {
        private string title = "Untitled playlist";

        /// <summary>
        /// The <see cref="Playlist"/>'s title.
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
    }
}