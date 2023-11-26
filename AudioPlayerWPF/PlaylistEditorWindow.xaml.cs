using AudioPlayerWPF.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AudioPlayerWPF {
    /// <summary>
    /// Interaction logic for PlaylistEditor.xaml
    /// </summary>
    /// 

    public partial class PlaylistEditorWindow : Window {
        private string filePath;
        private CustomLinkedList<PlaylistItemDTO> songCollection;
        private HashSet<string> songNoDuplicates;
        private string? name;
        //private bool editingExistingPlaylist;

        public delegate void NewPlaylistCreatedEventHandler(object sender, NewPlaylistCreatedEventArgs e);
        public event NewPlaylistCreatedEventHandler? NewPlaylistCreated;
        //public event NewPlaylistCreatedEventHandler? ExistingPlaylistEdited;

        public PlaylistEditorWindow(string name) {
            InitializeComponent();
            SourceInitialized += (sender, e) => {
                if (Properties.Settings.Default.DarkMode) {
                    App.ChangeWindowDarkMode(this, true);
                }
            };

            this.name = name;
            //editingExistingPlaylist = true;
            songCollection = new CustomLinkedList<PlaylistItemDTO>();
            songNoDuplicates = new HashSet<string>();
            filePath = System.IO.Path.Combine(App.playlistsDirectory, name + ".json");

            DataContext = this;

            using (JsonDocument jsonDocument = JsonDocument.Parse(System.IO.File.ReadAllText(filePath))) {
                try {
                    JsonElement root = jsonDocument.RootElement;
                    //this.name = root.GetProperty("Name").GetString();
                    if (name == null) {
                        throw new Exception("null playlist name");
                    }
                    playlistNameTextBox.Text = name;
                    PlaylistDTO? playlist = JsonSerializer.Deserialize<PlaylistDTO>(root);
                    if (playlist == null) { throw new Exception("Error while deserializing"); }

                    for (int i = 0; i < playlist.Songs.Count; ++i) {
                        songCollection.AddLast(playlist.Songs[i]);
                        songNoDuplicates.Add(playlist.Songs[i].FilePath);
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error reading playlist file: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally {
                    jsonDocument.Dispose();
                }
            }
        }

        public PlaylistEditorWindow() {
            InitializeComponent();
            SourceInitialized += (sender, e) => {
                if (Properties.Settings.Default.DarkMode) {
                    App.ChangeWindowDarkMode(this, true);
                }
            };

            name = "";
            //editingExistingPlaylist = false;
            playlistNameTextBox.Text = "Untitled Playlist";
            songCollection = new CustomLinkedList<PlaylistItemDTO>();
            songNoDuplicates = new HashSet<string>();
            filePath = System.IO.Path.Combine(App.playlistsDirectory, name + ".json");
            DataContext = this;
        }

        // Properties
        public CustomLinkedList<PlaylistItemDTO> SongCollection { // Referenced in XAML
            get { return songCollection; }
        }

        // Helpers

        private void AddFile(string filePath) {
            Song? song = new Song(new Uri(filePath));
            string title;
            if (song.TagLibFile.Tag.Title != null) {
                title = song.TagLibFile.Tag.Title;
            }
            else {
                title = "";
            }

            PlaylistItemDTO item = new PlaylistItemDTO(title, song.FileUriString, song.FileName, song.TagLibFile.Properties.Duration.TotalSeconds); // Passing these by reference may keep the song object alive (bad)
            if (!songNoDuplicates.Contains(filePath)) {
                songCollection.AddLast(item);
                songNoDuplicates.Add(filePath);
            }
            // song.Dispose();
            // song = null;
        }

        // Event handlers

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Audio Files (*.mp3;*.flac;*.wma;*.avi;*.mid;*.aif;*.wav;*.m4a)|*.mp3;*.flac;*.wma;*.avi;*.mid;*.midi;*.aif;*.wav;*.m4a|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true) {
                foreach (string fileName in openFileDialog.FileNames) {
                    AddFile(fileName);
                }
            }
            BindingExpression bindingExpression = mainListView.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            bindingExpression.UpdateSource();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            int n = mainListView.SelectedItems.Count;
            for (int i = 0; i < n; ++i) { // Make this more efficient
                if (mainListView.SelectedItems[0] is Node<PlaylistItemDTO> selectedSong) {
                    songCollection.Delete(selectedSong, mainListView.SelectedIndex);
                    if (selectedSong.Data != null) {
                        songNoDuplicates.Remove(selectedSong.Data.FilePath);
                    }
                }
            }
            deleteButton.IsEnabled = false;
        }

        private void mainListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            deleteButton.IsEnabled = true;
            moveUpButton.IsEnabled = true;
            moveDownButton.IsEnabled = true;
            if (mainListView.SelectedItems.Contains(songCollection.Head)) { // if first item, disable moving up
                moveUpButton.IsEnabled = false;
            }

            if (mainListView.SelectedItems.Contains(songCollection.Tail)) { // if first item, disable moving up
                moveDownButton.IsEnabled = false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
            if (name == null) { return; }
            if (playlistNameTextBox.Text.Length == 0) {
                MessageBox.Show("Please enter a name for the playlist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            PlaylistDTO data = new PlaylistDTO(name, songCollection.GetList());
            string newJson = JsonSerializer.Serialize(data, new JsonSerializerOptions {
                WriteIndented = true
            });

            string path = System.IO.Path.Combine(App.playlistsDirectory, playlistNameTextBox.Text + ".json");

            if (name == "" || (name != "" && name != playlistNameTextBox.Text)) {
                string[] playlists = Directory.GetFiles(App.playlistsDirectory);
                foreach (string playlist in playlists) {
                    if (playlistNameTextBox.Text == System.IO.Path.GetFileNameWithoutExtension(playlist)) {
                        MessageBoxResult result = MessageBox.Show("A playlist with this name already exists. Would you like to overwrite?", "Naming conflict!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.No) {
                            return;
                        }
                        break;
                    }
                }
            }

            System.IO.File.WriteAllText(path, newJson);
            NewPlaylistCreated?.Invoke(this, new NewPlaylistCreatedEventArgs(playlistNameTextBox.Text, path, name));

            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            NewPlaylistCreated = null;
            //ExistingPlaylistEdited = null;
            songCollection.Dispose();
            BindingOperations.ClearAllBindings(mainListView);
        }

        private void mainListView_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.Copy;
            }
            else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void mainListView_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files) {
                    AddFile(file);
                }
            }
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e) {
            Node<PlaylistItemDTO>? node = mainListView.SelectedItem as Node<PlaylistItemDTO>;
            if (node == null) return;
            songCollection.MoveNodeUp(node, mainListView.SelectedIndex);

            if (mainListView.SelectedItems.Contains(songCollection.Head)) moveUpButton.IsEnabled = false;
            else moveUpButton.IsEnabled = true;
            if (mainListView.SelectedItems.Contains(songCollection.Tail)) moveDownButton.IsEnabled = false;
            else moveDownButton.IsEnabled = true;
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e) {
            Node<PlaylistItemDTO>? node = mainListView.SelectedItem as Node<PlaylistItemDTO>;
            if (node == null) return;
            songCollection.MoveNodeDown(node, mainListView.SelectedIndex);

            if (mainListView.SelectedItems.Contains(songCollection.Head)) moveUpButton.IsEnabled = false;
            else moveUpButton.IsEnabled = true;
            if (mainListView.SelectedItems.Contains(songCollection.Tail)) moveDownButton.IsEnabled = false;
            else moveDownButton.IsEnabled = true;
        }
    }
}
