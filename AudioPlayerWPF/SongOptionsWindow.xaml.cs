using AudioPlayerWPF.Classes;
using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioPlayerWPF {
    /// <summary>
    /// Interaction logic for SongOptionsWindow.xaml
    /// </summary>
    public partial class SongOptionsWindow : Window {
        private BookmarkDTO[]? bookmarksList;
        string name;
        string optionsPath;

        public event EventHandler? SongOptionsWindowConfirmButtonPressed;

        public SongOptionsWindow(string name) {
            InitializeComponent();
            SourceInitialized += (sender, e) => {
                if (Properties.Settings.Default.DarkMode) {
                    App.ChangeTitleBarDarkMode(true);
                }
            };

            this.name = name;
            string bookmarksPath = System.IO.Path.Combine(App.bookmarksDirectory, name + ".json");
            optionsPath = System.IO.Path.Combine(App.songOptionsDirectory, name + ".json");

            // If theres a bookmark file for this song, add them to the ComboBoxes:
            if (File.Exists(bookmarksPath)) {
                try {
                    using (JsonDocument jsonDocument = JsonDocument.Parse(System.IO.File.ReadAllText(bookmarksPath))) {
                        JsonElement root = jsonDocument.RootElement;
                        BookmarkListDTO? bookmarks = JsonSerializer.Deserialize<BookmarkListDTO>(root);
                        if (bookmarks != null) {
                            bookmarksList = new BookmarkDTO[bookmarks.Bookmarks.Count];
                            for (int i = 0; i < bookmarks.Bookmarks.Count; ++i) { // Starting pos combo box
                                bookmarksList[i] = bookmarks.Bookmarks[i];
                                startingBookmarkComboBox.Items.Add(NewComboBoxBookmark(bookmarksList[i]));
                            }
                            foreach (BookmarkDTO bookmark in bookmarks.Bookmarks) { // Ending pos combo box
                                endingBookmarkComboBox.Items.Add(NewComboBoxBookmark(bookmark));
                            }

                            if (bookmarks.Bookmarks.Count == 0) {
                                startingPosBookmarkButton.IsEnabled = false;
                                endingPosBookmarkButton.IsEnabled = false;
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error reading bookmark file: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else {
                startingPosBookmarkButton.IsEnabled = false;
                endingPosBookmarkButton.IsEnabled = false;
            }
            
            if (File.Exists(optionsPath)) {
                try {
                    using (JsonDocument jsonDocument = JsonDocument.Parse(System.IO.File.ReadAllText(optionsPath))) {
                        
                        JsonElement root = jsonDocument.RootElement;
                        SongOptionsDTO? options = JsonSerializer.Deserialize<SongOptionsDTO>(root);
                        if (options != null) {
                            volumeSlider.Value = options.Volume;
                            if (options.StartingPosMilliseconds != 0 && options.StartingPosBookmarkTitle != "" && bookmarksList != null) {
                                int index = BinarySearchPosition(options.StartingPosMilliseconds);
                                if (index >= 0 && index < bookmarksList.Length) {
                                    startingBookmarkComboBox.SelectedIndex = index;
                                }
                                else {
                                    MessageBox.Show("The starting position for this song was set to a bookmark that was deleted. Please update the starting position setting.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            if (options.EndingPosMilliseconds != 0 && options.EndingPosBookmarkTitle != "" && bookmarksList != null) {
                                int index = BinarySearchPosition(options.EndingPosMilliseconds);
                                if (index >= 0 && index < bookmarksList.Length) {
                                    endingBookmarkComboBox.SelectedIndex = index;
                                }
                                else {
                                    MessageBox.Show("The ending position for this song was set to a bookmark that was deleted. Please update your ending position setting.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error reading song options file: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        // Helpers

        private ComboBoxItem NewComboBoxBookmark(BookmarkDTO bookmark) {
            StackPanel sp = new StackPanel() {
                Orientation = Orientation.Horizontal
            };
            TimeSpan ts = TimeSpan.FromMilliseconds(bookmark.Position);
            sp.Children.Add(new Image() {
                Source = App.bookmarkIcon,
                MaxHeight = 16
            });
            sp.Children.Add(new TextBlock() {
                Text = $"({ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}.{ts.Milliseconds.ToString().PadLeft(3, '0')}) {bookmark.Name}"
            });

            return new ComboBoxItem() {
                Content = sp
            };
        }

        private int BinarySearchPosition(double targetValue) {
            if (bookmarksList == null) return -1;
            int left = 0;
            int right = bookmarksList.Length - 1;
            while (left <= right) {
                int middle = left + (right - left) / 2;
                if (bookmarksList[middle].Position == targetValue) {
                    return middle;
                }
                else if (bookmarksList[middle].Position < targetValue) {
                    left = middle + 1;
                }
                else {
                    right = middle - 1;
                }
            }
            return -1;
        }
        
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Handlers

        private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
            SongOptionsDTO options = new SongOptionsDTO();
            if (bookmarksList != null) {
                if (endingPosBookmarkButton.IsChecked == true && endingBookmarkComboBox.SelectedItem != null) {
                    if (startingPosBookmarkButton.IsChecked == true && startingBookmarkComboBox.SelectedIndex >= endingBookmarkComboBox.SelectedIndex) {
                        MessageBox.Show("The ending position must come after the starting position.", "Notice", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    options.EndingPosMilliseconds = bookmarksList[endingBookmarkComboBox.SelectedIndex].Position;
                    options.EndingPosBookmarkTitle = bookmarksList[endingBookmarkComboBox.SelectedIndex].Name;
                }
                if (startingPosBookmarkButton.IsChecked == true && startingBookmarkComboBox.SelectedItem != null) {
                    options.StartingPosMilliseconds = bookmarksList[startingBookmarkComboBox.SelectedIndex].Position;
                    options.StartingPosBookmarkTitle = bookmarksList[startingBookmarkComboBox.SelectedIndex].Name;
                }
            }
            options.Volume = volumeSlider.Value;

            string newJson = JsonSerializer.Serialize(options, new JsonSerializerOptions {
                WriteIndented = true
            });
            System.IO.File.WriteAllText(optionsPath, newJson);
            SongOptionsWindowConfirmButtonPressed?.Invoke(this, EventArgs.Empty);
            Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            //BindingOperations.ClearAllBindings(mainListView);
        }

        private void startingBookmarkComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            startingPosBookmarkButton.IsChecked = true;
        }

        private void endingBookmarkComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            endingPosBookmarkButton.IsChecked = true;
        }
    }
}
