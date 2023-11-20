using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace AudioPlayerWPF.ViewModels
{
    public class SongViewModel : INotifyPropertyChanged {

        private string currentSongTitle;
        private string currentAlbum;
        private string currentArtist;
        private string currentAlbumArtist;
        private BitmapImage currentAlbumArt;
        private TimeSpan totalSongDuration;
        private TimeSpan currentSongPosition;

        public SongViewModel() {
            currentSongTitle = "No song loaded";
            currentAlbum = "n/a";
            currentArtist = "n/a";
            currentAlbumArtist = "n/a";
            totalSongDuration = TimeSpan.Zero;
            currentSongPosition = TimeSpan.Zero;
            currentAlbumArt = App.noimgImage;
        }

        public string CurrentSongTitle {
            get { return currentSongTitle; }
            set {
                if (currentSongTitle != value) {
                    currentSongTitle = value;
                    OnPropertyChanged(nameof(CurrentSongTitle));
                }
            }
        }
        public string CurrentAlbum {
            get { return currentAlbum; }
            set {
                if (currentAlbum != value) {
                    currentAlbum = value;
                    OnPropertyChanged(nameof(CurrentAlbum));
                }
            }
        }

        public string CurrentArtist {
            get { return currentArtist; }
            set {
                if (currentArtist != value) {
                    currentArtist = value;
                    OnPropertyChanged(nameof(CurrentArtist));
                }
            }
        }

        public string CurrentAlbumArtist {
            get { return currentAlbumArtist; }
            set {
                if (currentAlbumArtist != value) {
                    currentAlbumArtist = value;
                    OnPropertyChanged(nameof(CurrentAlbumArtist));
                }
            }
        }

        public TimeSpan TotalSongDuration {
            get { return totalSongDuration; }
            set {
                if (totalSongDuration != value) {
                    totalSongDuration = value;
                    OnPropertyChanged(nameof(TotalSongDuration));
                }
            }
        }

        public TimeSpan CurrentSongPosition {
            get { return currentSongPosition; }
            set {
                if (currentSongPosition != value) {
                    currentSongPosition = value;
                    OnPropertyChanged(nameof(CurrentSongPosition));
                }
            }
        }

        public BitmapImage CurrentAlbumArt {
            get { return currentAlbumArt; }
            set {
                if (currentAlbumArt != value) {
                    currentAlbumArt = value;
                    OnPropertyChanged(nameof(CurrentAlbumArt));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
