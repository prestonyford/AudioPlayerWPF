using System.Collections.Generic;

namespace AudioPlayerWPF.Classes
{
    public class Playlist {
        private string name;
        private List<Song> songs;
        private int currentSongIdx;

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

        // Properties

        public string Name { get { return name; } }
        public List<Song> Songs { get { return songs; } set { songs = value; } }
        public int CurrentSongIdx { get {  return currentSongIdx; } set { currentSongIdx = value; } }
        public Song CurrentSong {
            get { return songs[currentSongIdx]; }
        }
    }
}
