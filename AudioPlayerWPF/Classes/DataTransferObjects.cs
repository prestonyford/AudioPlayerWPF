using System.Collections.Generic;
using System.ComponentModel;

namespace AudioPlayerWPF.Classes {
    public class PlaylistItemDTO {
        public PlaylistItemDTO(string title, string filePath, string fileName, double duration) {
            Title = title;
            FilePath = filePath;
            FileName = fileName;
            Duration = duration;
        }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public double Duration { get; set; }
    }

    public class PlaylistDTO {
        public PlaylistDTO(string name, List<PlaylistItemDTO> songs) {
            Name = name;
            Songs = songs;
        }
        public string Name { get; set; }
        public List<PlaylistItemDTO> Songs { get; set; }
    }

    public class BookmarkDTO : INotifyPropertyChanged{
        private string name;
        private double position;
        public BookmarkDTO(string name, double position) {
            this.name = name;
            this.position = position;
        }
        public string Name {
            get {
                return name;
            }
            set {
                if (name != value) {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public double Position {
            get {
                return position;
            }
            set {
                if (position != value) {
                    position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BookmarkListDTO {
        public BookmarkListDTO(List<BookmarkDTO> bookmarks) {
            Bookmarks = bookmarks;
        }
        public BookmarkListDTO() {
            Bookmarks = new List<BookmarkDTO>();
        }
        public List<BookmarkDTO> Bookmarks { get; set; }
    }

    public class SongOptionsDTO {
        public SongOptionsDTO() {
            StartingPosMilliseconds = 0;
            StartingPosBookmarkTitle = string.Empty;
            EndingPosMilliseconds = 0;
            EndingPosBookmarkTitle = string.Empty;
            Volume = 1;
        }

        public double StartingPosMilliseconds { get; set; }
        public string StartingPosBookmarkTitle { get; set; }
        public double EndingPosMilliseconds { get; set; }
        public string EndingPosBookmarkTitle { get; set; }
        public double Volume { get; set; }
    }
}
