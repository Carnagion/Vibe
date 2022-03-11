using System.Collections.Generic;
using System.Linq;

namespace Vibe.Music
{
    /// <summary>
    /// An ordered list of <see cref="Track"/>s.
    /// </summary>
    public class Playlist : List<Track>
    {
        /// <summary>
        /// Initialises a new, empty <see cref="Playlist"/>.
        /// </summary>
        public Playlist()
        {
        }

        /// <summary>
        /// Initialises a new <see cref="Playlist"/> and adds all <see cref="Track"/>s in <paramref name="tracks"/> to it.
        /// </summary>
        /// <param name="tracks">The <see cref="Track"/>s to include in the <see cref="Playlist"/>.</param>
        public Playlist(IEnumerable<Track> tracks) : base(tracks)
        {
        }

        /// <summary>
        /// The <see cref="Playlist"/>'s title.
        /// </summary>
        public string Title
        {
            get;
            set;
        } = "";
    }
}