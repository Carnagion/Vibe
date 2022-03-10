using System.Collections.Generic;

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
            Playlist.currentMaxId += 1;
            this.Id = Playlist.currentMaxId;
        }

        /// <summary>
        /// Initialises a new <see cref="Playlist"/> and adds all <see cref="Track"/>s in <paramref name="tracks"/> to it.
        /// </summary>
        /// <param name="tracks">The <see cref="Track"/>s to include in the <see cref="Playlist"/>.</param>
        public Playlist(IEnumerable<Track> tracks) : base(tracks)
        {
            Playlist.currentMaxId += 1;
            this.Id = Playlist.currentMaxId;
        }

        private static long currentMaxId;

        /// <summary>
        /// The <see cref="Playlist"/>'s unique identifier.
        /// </summary>
        public long Id
        {
            get;
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