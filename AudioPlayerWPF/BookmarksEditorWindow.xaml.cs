using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using AudioPlayerWPF.Classes;
using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;

namespace AudioPlayerWPF {
    /// <summary>
    /// Interaction logic for BookmarksEditorWindow.xaml
    /// </summary>
    public partial class BookmarksEditorWindow : Window{

        private ObservableCollection<BookmarkDTO> bookmarksCollection;
        private HashSet<double> bookmarksNoDuplicates;
        private MediaPlayer mainWindowPlayer;
        private string name;
        public event EventHandler? ConfirmButtonPressed;

        public BookmarksEditorWindow(string name, ref MediaPlayer mainWindowPlayer) {
            this.mainWindowPlayer = mainWindowPlayer;
            this.name = name;
            DataContext = this;
            bookmarksCollection = new ObservableCollection<BookmarkDTO>();
            bookmarksNoDuplicates = new HashSet<double>();
            string filePath = System.IO.Path.Combine(App.bookmarksDirectory, name + ".json");
            if (File.Exists(filePath)) {
                try {
                    using (JsonDocument jsonDocument = JsonDocument.Parse(System.IO.File.ReadAllText(filePath))) {
                        JsonElement root = jsonDocument.RootElement;
                        BookmarkListDTO? bookmarks = JsonSerializer.Deserialize<BookmarkListDTO>(root);
                        if (bookmarks != null) {
                            foreach (BookmarkDTO bookmark in bookmarks.Bookmarks) {
                                bookmarksCollection.Add(bookmark);
                                bookmarksNoDuplicates.Add(bookmark.Position);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error reading bookmark file: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            InitializeComponent();
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Properties
        public ObservableCollection<BookmarkDTO> BookmarksCollection { get { return bookmarksCollection; } }

        // Buttons

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            try {
                int min = (minTextBox.Text == "") ? 0 : int.Parse(minTextBox.Text);
                int sec = (secTextBox.Text == "") ? 0 : int.Parse(secTextBox.Text);
                int ms = (msTextBox.Text == "") ? 0 : int.Parse(msTextBox.Text);

                TimeSpan ts = TimeSpan.FromMinutes(min) + TimeSpan.FromSeconds(sec) + TimeSpan.FromMilliseconds(ms);
                BookmarkDTO bookmark = new BookmarkDTO("Untitled bookmark", ts.TotalMilliseconds);
                if (!bookmarksNoDuplicates.Contains(bookmark.Position)) {
                    bookmarksCollection.Add(bookmark);
                    bookmarksNoDuplicates.Add(bookmark.Position);
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Error creating bookmark from the given values: " + ex.Message, "Error Occured!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            if (mainListView.SelectedItem is BookmarkDTO item) {
                mainListView.SelectedIndex++;
                bookmarksCollection.Remove(item);
                bookmarksNoDuplicates.Add(item.Position);
            }
            if (mainListView.SelectedItem == null) {
                deleteButton.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            //BindingOperations.ClearAllBindings(mainListView);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
            List<BookmarkDTO> bookmarksList = bookmarksCollection.ToList();
            bookmarksList.Sort((a,b) => a.Position.CompareTo(b.Position));

            BookmarkListDTO data = new BookmarkListDTO(bookmarksList);
            string newJson = JsonSerializer.Serialize(data, new JsonSerializerOptions {
                WriteIndented = true
            });

            string path = System.IO.Path.Combine(App.bookmarksDirectory, name + ".json");

            System.IO.File.WriteAllText(path, newJson);
            ConfirmButtonPressed?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private void RenameButton_Click(object sender, RoutedEventArgs e) {
            if (mainListView.SelectedItem != null && mainListView.SelectedItem is BookmarkDTO bookmark) {
                RenameWindow renameWindow = new RenameWindow(bookmark);
                renameWindow.Left = Left + Width/2 - renameWindow.MinWidth;
                renameWindow.Top = Top + Height/2 - renameWindow.MinHeight;
                renameWindow.Show();
            }
        }

        private void CurrentTimeButton_Click(object sender, RoutedEventArgs e) {
            BookmarkDTO bookmark = new BookmarkDTO("Untitled bookmark", mainWindowPlayer.Position.TotalMilliseconds);
            bookmarksCollection.Add(bookmark);
        }

        private void mainListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            deleteButton.IsEnabled = true;
            renameButton.IsEnabled = true;
        }

        
    }
}
