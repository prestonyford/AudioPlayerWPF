using AudioPlayerWPF.Classes;
using System.Collections.Generic;
using System.ComponentModel;

namespace AudioPlayerWPF.ViewModels {
    public class BookmarksEditorViewModel : INotifyPropertyChanged {
        private List<BookmarkDTO> bookmarksList;

        public BookmarksEditorViewModel() {
            bookmarksList = new List<BookmarkDTO>();
        }

        public List<BookmarkDTO> BookmarksList {
            get { return bookmarksList; }
            set {
                if (bookmarksList != value) {
                    bookmarksList = value;
                    OnPropertyChanged(nameof(BookmarksList));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
