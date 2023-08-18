using AudioPlayerWPF.Classes;
using System.ComponentModel;

namespace AudioPlayerWPF.ViewModels
{
    class PlaylistEditorViewModel : INotifyPropertyChanged {

        // Not used

        public PlaylistEditorViewModel() {
            songCollection = new CustomLinkedList<Song>();
        }
        CustomLinkedList<Song> songCollection;

        // Properties
        public CustomLinkedList<Song> SongCollection {
            get { return songCollection; }
            set {
                // if (songCollection != value) {
                    songCollection = value;
                    OnPropertyChanged(nameof(SongCollection));
                // }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
