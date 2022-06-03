using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Media;
using Android.OS;

namespace Vibe.Music
{
    /// <summary>
    /// Handles the playing, pausing, skipping, etc of music.
    /// </summary>
    public static class Playback
    {
        private static MediaPlayer mediaPlayer = null!;

        private static readonly LinkedList<Track> playingQueue = new();

        private static LinkedListNode<Track>? nowPlaying;

        private static MediaPlayerState mediaPlayerState = MediaPlayerState.Idle;

        /// <summary>
        /// The state of <see cref="Playback"/>'s <see cref="MediaPlayer"/>.
        /// </summary>
        public static MediaPlayerState PlayingState
        {
            get
            {
                return Playback.mediaPlayerState;
            }
            private set
            {
                MediaPlayerState previous = Playback.mediaPlayerState;
                Playback.mediaPlayerState = value;
                Playback.MediaPlayerStateChanged?.Invoke(null, new(previous, value));
            }
        }

        /// <summary>
        /// All the <see cref="Track"/>s in the current playing queue. Note that <see cref="NowPlaying"/> is not necessarily the first <see cref="Track"/>.
        /// </summary>
        public static IEnumerable<Track> PlayingQueue
        {
            get
            {
                return Playback.playingQueue.Copy();
            }
        }

        /// <summary>
        /// The <see cref="Track"/> currently being played, if any.
        /// </summary>
        public static Track? NowPlaying
        {
            get
            {
                return Playback.nowPlaying?.Value;
            }
        }

        /// <summary>
        /// Whether <see cref="NowPlaying"/> should automatically repeat itself after completion.
        /// </summary>
        public static bool Looping
        {
            get
            {
                return Playback.mediaPlayer.Looping;
            }
            set
            {
                Playback.mediaPlayer.Looping = value;
            }
        }

        /// <summary>
        /// The currently completed duration of <see cref="NowPlaying"/>, if any.
        /// </summary>
        public static uint CurrentPosition
        {
            get
            {
                return Playback.PlayingState switch
                {
                    MediaPlayerState.Started or MediaPlayerState.Paused => (uint)Playback.mediaPlayer.CurrentPosition,
                    MediaPlayerState.Completed => Playback.NowPlaying?.Duration ?? 0,
                    _ => 0,
                };
            }
            set
            {
                if (Playback.PlayingState is MediaPlayerState.Prepared or MediaPlayerState.Started or MediaPlayerState.Paused or MediaPlayerState.Completed)
                {
                    Playback.mediaPlayer.SeekTo((int)value);
                }
            }
        }

        /// <summary>
        /// Raised whenever <see cref="Playback"/>'s <see cref="MediaPlayer"/> changes its state.
        /// </summary>
        public static event EventHandler<MediaPlayerStateChangeArgs>? MediaPlayerStateChanged;

        /// <summary>
        /// Allows <see cref="Playback"/> to set up a new <see cref="MediaPlayer"/>. This must be called before any other method is called.
        /// </summary>
        public static void Setup()
        {
            Playback.mediaPlayer = new();
            
            Playback.mediaPlayer.SetWakeMode(Application.Context, WakeLockFlags.Partial);

            Playback.mediaPlayer.Error += Playback.OnMediaPlayerError;
            Playback.mediaPlayer.Completion += Playback.OnMediaPlayerCompletion;
            Playback.mediaPlayer.Prepared += Playback.OnMediaPlayerPrepared;
        }

        /// <summary>
        /// Allows <see cref="Playback"/> to release the resources held by its <see cref="MediaPlayer"/> and stop playing music. This must be called before the application is exited.
        /// </summary>
        public static void End()
        {
            Playback.mediaPlayer.Error -= Playback.OnMediaPlayerError;
            Playback.mediaPlayer.Completion -= Playback.OnMediaPlayerCompletion;
            Playback.mediaPlayer.Prepared -= Playback.OnMediaPlayerPrepared;
            
            Playback.Stop();
            
            Playback.mediaPlayer.Release();
            Playback.PlayingState = MediaPlayerState.End;
            Playback.mediaPlayer.Dispose();
        }

        /// <summary>
        /// Starts (or continues) playing the current <see cref="Track"/>. Does nothing if there are no <see cref="Track"/>s in <see cref="PlayingQueue"/>.
        /// </summary>
        public static void Start()
        {
            if (!Playback.playingQueue.Any())
            {
                return;
            }
            Playback.nowPlaying ??= Playback.playingQueue.First;

            switch (Playback.PlayingState)
            {
                case MediaPlayerState.Idle:
                    Playback.mediaPlayer.SetDataSource(Playback.NowPlaying!.Path);
                    Playback.mediaPlayer.PrepareAsync();
                    Playback.PlayingState = MediaPlayerState.Preparing;
                    break;
                case MediaPlayerState.Initialized or MediaPlayerState.Stopped:
                    Playback.mediaPlayer.PrepareAsync();
                    Playback.PlayingState = MediaPlayerState.Preparing;
                    break;
                case MediaPlayerState.Prepared or MediaPlayerState.Paused or MediaPlayerState.Completed:
                    Playback.StartPlaying();
                    break;
            }
        }

        /// <summary>
        /// Replaces <see cref="PlayingQueue"/> with <paramref name="tracks"/> and starts playing the current <see cref="Track"/>, stopping the previous <see cref="Track"/> (if any).
        /// </summary>
        /// <param name="tracks">The <see cref="Track"/>s to put in <see cref="PlayingQueue"/>.</param>
        public static void Start(IEnumerable<Track> tracks)
        {
            Playback.Stop();
            tracks.ForEach(track => Playback.playingQueue.AddLast(track));
            Playback.Start();
        }

        /// <summary>
        /// Pauses the current <see cref="Track"/>. Does nothing if <see cref="NowPlaying"/> is <see langword="null"/> or if <see cref="PlayingState"/> is not <see cref="MediaPlayerState.Started"/>.
        /// </summary>
        public static void Pause()
        {
            if (!Playback.playingQueue.Any() || Playback.PlayingState is not MediaPlayerState.Started)
            {
                return;
            }
            Playback.mediaPlayer.Pause();
            Playback.PlayingState = MediaPlayerState.Paused;
        }

        /// <summary>
        /// Stops playing music and clears all <see cref="Track"/>s in <see cref="PlayingQueue"/>.
        /// </summary>
        public static void Stop()
        {
            Playback.Reset();
            Playback.playingQueue.Clear();
            Playback.nowPlaying = null;
        }

        /// <summary>
        /// Skips to the next <see cref="Track"/> in <see cref="PlayingQueue"/>, if any.
        /// </summary>
        public static void SkipNext()
        {
            if (Playback.nowPlaying?.Next is null)
            {
                return;
            }
            Playback.nowPlaying = Playback.nowPlaying.Next;
            
            Playback.mediaPlayer.Reset();
            Playback.mediaPlayer.SetDataSource(Playback.NowPlaying!.Path);
            Playback.mediaPlayer.PrepareAsync();
            Playback.PlayingState = MediaPlayerState.Preparing;
        }

        /// <summary>
        /// Skips to the previous <see cref="Track"/> in <see cref="PlayingQueue"/>, if any.
        /// </summary>
        public static void SkipPrevious()
        {
            if (Playback.nowPlaying?.Previous is null)
            {
                return;
            }
            Playback.nowPlaying = Playback.nowPlaying.Previous;
            
            Playback.mediaPlayer.Reset();
            Playback.mediaPlayer.SetDataSource(Playback.NowPlaying!.Path);
            Playback.mediaPlayer.PrepareAsync();
            Playback.PlayingState = MediaPlayerState.Preparing;
        }
        
        /// <summary>
        /// Adds <paramref name="track"/> to the end of <see cref="PlayingQueue"/>.
        /// </summary>
        /// <param name="track">The <see cref="Track"/> to add.</param>
        public static void AddToQueue(Track track)
        {
            Playback.playingQueue.AddLast(track);
        }

        /// <summary>
        /// Adds all <see cref="Track"/>s in <paramref name="tracks"/> to the end of <see cref="PlayingQueue"/>.
        /// </summary>
        /// <param name="tracks">The <see cref="Track"/>s to add.</param>
        public static void AddToQueue(IEnumerable<Track> tracks)
        {
            tracks.ForEach(Playback.AddToQueue);
        }

        /// <summary>
        /// Adds <paramref name="track"/> directly after <see cref="NowPlaying"/> in <see cref="PlayingQueue"/>.
        /// </summary>
        /// <param name="track">The <see cref="Track"/> to add.</param>
        public static void InsertNextInQueue(Track track)
        {
            switch (Playback.nowPlaying)
            {
                case null:
                    Playback.playingQueue.AddLast(track);
                    break;
                default:
                    Playback.playingQueue.AddAfter(Playback.nowPlaying, track);
                    break;
            }
        }

        /// <summary>
        /// Adds all <see cref="Track"/>s in <paramref name="tracks"/> directly after <see cref="NowPlaying"/> in <see cref="PlayingQueue"/>.
        /// </summary>
        /// <param name="tracks">The <see cref="Track"/>s to add.</param>
        public static void InsertNextInQueue(IEnumerable<Track> tracks)
        {
            switch (Playback.nowPlaying)
            {
                case null:
                    tracks.ForEach(track => Playback.playingQueue.AddLast(track));
                    break;
                default:
                    tracks.Reverse()
                        .ForEach(track => Playback.playingQueue.AddAfter(Playback.nowPlaying, track));
                    break;
            }
        }

        private static void StartPlaying()
        {
            Playback.mediaPlayer.Start();
            Playback.PlayingState = MediaPlayerState.Started;
        }

        private static void Reset()
        {
            Playback.mediaPlayer.Reset();
            Playback.PlayingState = MediaPlayerState.Idle;
        }

        private static void OnMediaPlayerPrepared(object source, EventArgs eventArgs)
        {
            Playback.PlayingState = MediaPlayerState.Prepared;
            Playback.StartPlaying();
        }

        private static void OnMediaPlayerCompletion(object source, EventArgs eventArgs)
        {
            Playback.PlayingState = MediaPlayerState.Completed;
            Playback.SkipNext();
        }

        private static void OnMediaPlayerError(object source, MediaPlayer.ErrorEventArgs eventArgs)
        {
            Playback.Reset();
            Playback.Start();
        }

        /// <summary>
        /// The possible states of a <see cref="MediaPlayer"/>.
        /// </summary>
        public enum MediaPlayerState
        {
            /// <summary>
            /// The <see cref="MediaPlayer"/>'s starting state.
            /// </summary>
            Idle,
            /// <summary>
            /// The <see cref="MediaPlayer"/> is initialised.
            /// </summary>
            Initialized,
            /// <summary>
            /// The <see cref="MediaPlayer"/> is preparing to play media.
            /// </summary>
            Preparing,
            /// <summary>
            /// The <see cref="MediaPlayer"/> has finished preparing.
            /// </summary>
            Prepared,
            /// <summary>
            /// The <see cref="MediaPlayer"/> is playing media.
            /// </summary>
            Started,
            /// <summary>
            /// The <see cref="MediaPlayer"/> has been paused.
            /// </summary>
            Paused,
            /// <summary>
            /// The <see cref="MediaPlayer"/> has currently playing media has finished.
            /// </summary>
            Completed,
            /// <summary>
            /// The <see cref="MediaPlayer"/> has stopped playing media.
            /// </summary>
            Stopped,
            /// <summary>
            /// The <see cref="MediaPlayer"/>'s final state.
            /// </summary>
            End,
        }

        /// <summary>
        /// Stores data to be passed when <see cref="Playback.MediaPlayerStateChanged"/> is raised.
        /// </summary>
        public sealed class MediaPlayerStateChangeArgs : EventArgs
        {
            /// <summary>
            /// Initialises a new <see cref="MediaPlayerStateChangeArgs"/> with the specified parameters.
            /// </summary>
            /// <param name="previousState">The previous state of <see cref="Playback"/>'s <see cref="MediaPlayer"/>.</param>
            /// <param name="changedTo">The current state of <see cref="Playback"/>'s <see cref="MediaPlayer"/>.</param>
            public MediaPlayerStateChangeArgs(MediaPlayerState previousState, MediaPlayerState changedTo)
            {
                this.PreviousState = previousState;
                this.ChangedTo = changedTo;
            }
            
            /// <summary>
            /// The previous state of <see cref="Playback"/>'s <see cref="MediaPlayer"/>.
            /// </summary>
            public MediaPlayerState PreviousState
            {
                get;
            }

            /// <summary>
            /// The current state of <see cref="Playback"/>'s <see cref="MediaPlayer"/>.
            /// </summary>
            public MediaPlayerState ChangedTo
            {
                get;
            }
        }
    }
}