using System;
using System.Collections.Generic;

namespace AudioPlayerWPF.Classes
{
    public class Playlist {
        private string name;
        private List<Song> songs;
        private int currentSongIdx;
        private bool repeat = false;
        private bool shuffle = false;
        
        // Stack history by index
        private Stack<int> history = new Stack<int>();

        public Playlist() {
            name = "Unnamed empty playlist";
            songs = new List<Song>();
        }
        public Playlist(Song s) {
            name = "internal single-use playlist";
            songs = new List<Song> { s };
            currentSongIdx = 0;
        }
        public Playlist(string name, List<Song> songs) {
            this.name = name;
            this.songs = songs;
            currentSongIdx = 0;
        }

        // Methods
        public void ResetPlaylist(string name, List<Song> songs) {
            this.name = name;
            this.songs = songs;
            currentSongIdx = 0;
            history.Clear();
        }
        public bool NextSongExists(bool ignoreRepeat = false) {
            if (shuffle || (repeat && !ignoreRepeat)) { return true; }
            else {
                return currentSongIdx != songs.Count - 1;
            }
        }
        public bool PreviousSongExists() {
            // return currentSongIdx != 0;
            return (shuffle && history.Count > 0) || (!shuffle && currentSongIdx > 0);
        }
        public Song? GoNextSong(bool ignoreRepeat = false) {
            // Save current to history
            this.history.Push(currentSongIdx);

            if (!ignoreRepeat && repeat) {
                return songs[currentSongIdx];
            }
            else if (shuffle) {
                return Randomize(true);
            }
            else if (currentSongIdx == songs.Count - 1) {
                return null;
            }
            else {
                ++currentSongIdx;
            }
            return songs[currentSongIdx];
        }
        public Song GoPrevSong() {
            if (shuffle) {
                if (history.Count > 0) {
                    int prevSongIdx = history.Pop();
                    currentSongIdx = prevSongIdx;
                }
            }
            else {
                if (currentSongIdx > 0) {
                    --currentSongIdx;
                }
            }
            return songs[currentSongIdx];
        }
        public Song Randomize(bool excludeCurrent = false) {
            Random random = new Random();
            int newIndex;

            // Prevent playing the same song twice in a row
            if (excludeCurrent == true && songs.Count > 1) {
                int excludedNumber = currentSongIdx;
                newIndex = random.Next(0, songs.Count - 1);
                if (newIndex >= excludedNumber) {
                    ++newIndex;
                }
            }
            // Or purely randomize (possibly the same song again)
            else {
                newIndex = random.Next(0, songs.Count);
            }
            currentSongIdx = newIndex;

            return songs[currentSongIdx];
        }

        // Properties
        public string Name { get { return name; } set { name = value; } }
        public List<Song> Songs { get { return songs; } set { songs = value; } }
        public int CurrentSongIdx { get {  return currentSongIdx; } set { currentSongIdx = value; } }
        public Song CurrentSong {
            get { return songs[currentSongIdx]; }
        }
        public bool Repeat { get { return repeat; } set { repeat = value; } }
        public bool Shuffle { get { return shuffle; } set {  shuffle = value; } }
    }
}
