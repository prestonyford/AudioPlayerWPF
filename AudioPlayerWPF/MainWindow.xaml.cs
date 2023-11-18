using Microsoft.Win32;
using AudioPlayerWPF.Classes;
using AudioPlayerWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Controls.Primitives;

namespace AudioPlayerWPF {
    public partial class MainWindow : Window {
        private SongViewModel songViewModel;
        private MediaPlayer mediaPlayer;
        private DispatcherTimer timer;
        private Playlist? playlist;

        private bool userIsDraggingSlider = false;
        private bool musicIsPlaying = false;
        private bool repeat = false;
        private bool shuffle = false;
        private static readonly BitmapImage pauseImage = new BitmapImage(new Uri("/Images/icons8-pause-48.png", UriKind.Relative));
        private static readonly BitmapImage playImage = new BitmapImage(new Uri("/Images/icons8-play-48.png", UriKind.Relative));

        private Stack<Uri> history = new Stack<Uri>();

        private Dictionary<string, MenuItem> playlistMenuItemsDictionary;
        private Dictionary<TimeSpan, MenuItem> bookmarkMenuItemsDictionary;

        // song options
        private double startingPos = 0; // these are in milliseconds
        private double endingPos = 0;

        // general options
        private int tickSpeed = 500; // also in milliseconds

        // Constructors

        public MainWindow() {
            InitializeComponent();
            songViewModel = new SongViewModel();
            mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaOpened += OnMediaOpened;
            mediaPlayer.MediaEnded += OnMediaEnded;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(tickSpeed);
            timer.Tick += Timer_Tick;
            playlistMenuItemsDictionary = new Dictionary<string, MenuItem>();
            bookmarkMenuItemsDictionary = new Dictionary<TimeSpan, MenuItem>();

            DataContext = songViewModel;

            // Load playlists
            string[] playlists = Directory.GetFiles(App.playlistsDirectory);
            foreach (string playlistUri in playlists) {
                string name = System.IO.Path.GetFileNameWithoutExtension(playlistUri);
                MenuItem newMenuItem = NewPlaylistMenuItem(name, playlistUri);
                playlistMenu.Items.Add(newMenuItem);
                playlistMenuItemsDictionary.Add(name, newMenuItem);
            }

            DisablePrevTrackButton();
            DisableNextTrackButton();

            Closing += (sender, e) => { Application.Current.Shutdown(); };
        }

        // Properties
        public double WindowWidth { get { return this.Width; } }
        public double WindowHeight { get { return this.Height; } }

        // Helpers

        private MenuItem NewBookmarkMenuItem(BookmarkDTO bookmark) {
            MenuItem item = new MenuItem();
            TimeSpan ts = TimeSpan.FromMilliseconds(bookmark.Position);
            item.Header = $"({ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}) {bookmark.Name}";
            item.Icon = new Image { Source = App.bookmarkIcon };

            item.Click += async (object sender, RoutedEventArgs e) => { // WPF controls use weak events? So this shouldn't need to be unsubscribed probably?
                mediaPlayer.Position = ts;

                // Prevent song from ending if this is the bookmark set as the ending pos
                UpdateSlider();
                timer.Stop();
                await Task.Delay(tickSpeed + 1);
                UpdateSlider();
                timer.Start();
            };

            return item;
        }

        private MenuItem NewPlaylistMenuItem(string name, string path) {
            // Playlist menu item
            MenuItem result = new MenuItem();
            result.Header = name;
            result.Icon = new Image { Source = App.playlistIcon };

            MenuItem playMenuItem = NewPlaylistSubMenuItem("Play", new Image { Source = App.playIcon });
            MenuItem editMenuItem = NewPlaylistSubMenuItem("Edit", new Image { Source = App.editIcon });
            MenuItem deleteMenuItem = NewPlaylistSubMenuItem("Delete", new Image { Source = App.deleteIcon });
            result.Items.Add(playMenuItem);
            result.Items.Add(editMenuItem);
            result.Items.Add(deleteMenuItem);
            result.Items.Add(new Separator());

            // MenuItem click event handlers
            using JsonDocument jsonDocument = JsonDocument.Parse(System.IO.File.ReadAllText(path));
            try {
                JsonElement root = jsonDocument.RootElement;
                //string? nameFromJson = root.GetProperty("Name").GetString();
                PlaylistDTO? playlistJson = JsonSerializer.Deserialize<PlaylistDTO>(root);
                if (name == null || playlistJson == null) {
                    throw new Exception("Invalid playlist file");
                }

                // Play playlist button
                RoutedEventHandler? playLambda = null;
                playLambda = (object sender, RoutedEventArgs e) => {
                    List<Song> songs = new List<Song>();
                    foreach (PlaylistItemDTO item in playlistJson.Songs) {
                        if (SongExists(item.FilePath)) {
                            songs.Add(new Song(item.FilePath));
                        }
                    }
                    if (songs.Count == 0) {
                        MessageBox.Show("The selected playlist is empty.", "Error Occured!", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    playlist = new Playlist(name, songs);
                    OpenNewSong(playlist.CurrentSong.FileUri);
                    UpdateViewModel();
                    if (playlist.CurrentSongIdx == 0) DisablePrevTrackButton();
                    else EnablePrevTrackButton();
                    if (playlist.CurrentSongIdx == playlist.Songs.Count - 1) DisableNextTrackButton();
                    else EnableNextTrackButton();
                };
                playMenuItem.Click += playLambda;

                // Edit playlist button
                RoutedEventHandler? editLambda = null;
                editLambda = (object sender, RoutedEventArgs e) => {
                    PlaylistEditorWindow playlistEditorWindow = new PlaylistEditorWindow(name);
                    playlistEditorWindow.Show();
                    playlistEditorWindow.NewPlaylistCreated += OnNewPlaylistCreated; // Was existing before
                };
                editMenuItem.Click += editLambda;

                // Specific song buttons
                int i = 0;
                // Keep track of each song MenuItem's Click event and its handler, to be removed when the Delete button is clicked
                Dictionary<MenuItem, RoutedEventHandler> songButtonsToHandlers = new Dictionary<MenuItem, RoutedEventHandler>();
                foreach (PlaylistItemDTO item in playlistJson.Songs) {
                    string songName;
                    if (item.Title != null && item.Title != "") {
                        songName = item.Title;
                    }
                    else {
                        songName = item.FileName;
                    }
                    MenuItem songMenuItem = NewPlaylistSubMenuItem(songName, new Image { Source = App.songIcon });
                    result.Items.Add(songMenuItem);

                    int j = i;
                    RoutedEventHandler songHandler = (object sender, RoutedEventArgs e) => {
                        List<Song> songs = new List<Song>();
                        foreach (PlaylistItemDTO item in playlistJson.Songs) {
                            if (SongExists(item.FilePath)) {
                                songs.Add(new Song(item.FilePath));
                            }
                            else {
                                return;
                            }
                        }
                        if (songs.Count == 0) {
                            MessageBox.Show("The selected playlist is empty.", "Error Occured!", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        playlist = new Playlist(name, songs);
                        playlist.CurrentSongIdx = j;
                        OpenNewSong(playlist.Songs[j].FileUri);
                        UpdateViewModel();
                        if (playlist.CurrentSongIdx == 0) DisablePrevTrackButton();
                        else EnablePrevTrackButton();
                        if (playlist.CurrentSongIdx == playlist.Songs.Count - 1) DisableNextTrackButton();
                        else EnableNextTrackButton();
                    };
                    songMenuItem.Click += songHandler;
                    songButtonsToHandlers.Add(songMenuItem, songHandler);
                    ++i;
                }

                // Delete playlist button
                RoutedEventHandler? deleteLambda = null;
                deleteLambda = (object sender, RoutedEventArgs e) => {
                    MessageBoxResult messageResult = MessageBox.Show("Are you sure you want to delete this playlist?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageResult == MessageBoxResult.No) return;

                    if (File.Exists(path)) {
                        File.Delete(path);
                        playMenuItem.Click -= playLambda;
                        editMenuItem.Click -= editLambda;
                        deleteMenuItem.Click -= deleteLambda;
                        playlistMenuItemsDictionary.Remove(name);
                        playlistMenu.Items.Remove(result);

                        // Remove song click subscriber from each song
                        foreach (KeyValuePair<MenuItem, RoutedEventHandler> pair in songButtonsToHandlers) {
                            pair.Key.Click -= pair.Value;
                        }
                    }
                };
                deleteMenuItem.Click += deleteLambda;

            }
            catch (Exception ex) {
                MessageBox.Show("Error reading playlist file: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally {
                jsonDocument.Dispose();
            }


            return result;
        }

        private MenuItem NewPlaylistSubMenuItem(string name, Image img) {
            MenuItem item = new MenuItem();
            item.Header = name;
            item.Icon = img;
            return item;
        }

        private bool SongExists(string uristr) {
            if (File.Exists(uristr)) {
                return true;
            }
            else {
                MessageBox.Show("Could not find the file: " + uristr + ".\nDid you delete, move, or rename it?", "File error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void UpdateViewModel() {
            if (playlist == null) { return; }
            Song song = playlist.CurrentSong;
            if (song.Title == null) {
                songViewModel.CurrentSongTitle = System.IO.Path.GetFileNameWithoutExtension(song.FileUri.LocalPath); // Excludes path and file extension
            }
            else {
                songViewModel.CurrentSongTitle = song.Title;
            }
            if (song.CoverArt == null) {
                songViewModel.CurrentAlbumArt = App.noimgImage;
            }
            else {
                songViewModel.CurrentAlbumArt = song.CoverArt;
            }
            if (song.Album == null) {
                songViewModel.CurrentAlbum = "n/a";
            }
            else {
                songViewModel.CurrentAlbum = song.Album;
            }
            if (song.Artist == null || song.Artist == "") {
                songViewModel.CurrentArtist = "n/a";
            }
            else {
                songViewModel.CurrentArtist = song.Artist;
            }
            if (song.AlbumArtist == null || song.AlbumArtist == "") {
                songViewModel.CurrentArtist = "n/a";
            }
            else {
                songViewModel.CurrentArtist = song.AlbumArtist;
            }
            songViewModel.TotalSongDuration = song.Duration;
        }

        private void UpdateSlider() {
            if (mediaPlayer.NaturalDuration.HasTimeSpan) {
                SongSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                SongSlider.Value = mediaPlayer.Position.TotalSeconds;
                songViewModel.CurrentSongPosition = mediaPlayer.Position;
            }
        }

        private void UpdatePlayButton() {
            if (musicIsPlaying) {
                if (playButton.Content is Image image) {
                    image.Source = pauseImage;
                }
            }
            else {
                if (playButton.Content is Image image) {
                    image.Source = playImage;
                }
            }
        }

        private void PlayMusic() {
            loadingBlock.Visibility = Visibility.Hidden;
            mediaPlayer.Play();
            musicIsPlaying = true;
            timer.Start();
        }

        private void PauseMusic() {
            mediaPlayer.Pause();
            musicIsPlaying = false;
            timer.Stop();
        }

        private void TogglePausePlay() {
            if (musicIsPlaying) {
                PauseMusic();
            }
            else {
                PlayMusic();
            }
        }

        private void OpenNewSong(Uri uri) { // This should always follow updating the playlist
            if (mediaPlayer.Source != uri) { // If the source is the same, MediaOpened will never fire, and the loading block will never get hidden
                loadingBlock.Visibility = Visibility.Visible;

                LoadOptions();
            }

            string filestr = uri.OriginalString;
            if (SongExists(filestr)) {
                mediaPlayer.Open(uri);
            }
        }

        private void GoNextTrack() {
            if (playlist == null) { return; }

            if (shuffle == true && playlist.Songs.Count > 1) {
                Random random = new Random();
                // Prevent playing the same song twice in a row
                int excludedNumber = playlist.CurrentSongIdx;
                int newIndex = random.Next(0, playlist.Songs.Count - 1);
                if (newIndex >= excludedNumber) {
                    ++newIndex;
                }
                playlist.CurrentSongIdx = newIndex;
            }
            else if (playlist.Songs.Count == 1) {
                PlayMusic();
            }
            else {
                ++playlist.CurrentSongIdx;
                if (playlist.CurrentSongIdx == playlist.Songs.Count - 1) {
                    DisableNextTrackButton();
                }
                EnablePrevTrackButton();
            }
            OpenNewSong(playlist.CurrentSong.FileUri);
            UpdateViewModel();
        }

        private void GoPrevTrack() {
            if (playlist == null) {
                DisablePrevTrackButton();
                return;
            }
            OpenNewSong(playlist.Songs[--playlist.CurrentSongIdx].FileUri);
            UpdateViewModel();

            if (playlist.CurrentSongIdx == 0) {
                DisablePrevTrackButton();
            }
            EnableNextTrackButton();
        }

        private void LoadBookmarks() {
            // Load bookmarks
            string filePath = System.IO.Path.Combine(App.bookmarksDirectory, songViewModel.CurrentSongTitle + ".json");
            if (File.Exists(filePath)) {
                try {
                    using (JsonDocument jsonDocument = JsonDocument.Parse(System.IO.File.ReadAllText(filePath))) {
                        JsonElement root = jsonDocument.RootElement;
                        BookmarkListDTO? bookmarks = JsonSerializer.Deserialize<BookmarkListDTO>(root);
                        if (bookmarks != null) {
                            foreach (BookmarkDTO bookmark in bookmarks.Bookmarks) {
                                MenuItem item = NewBookmarkMenuItem(bookmark);
                                songMenu.Items.Add(item);
                                TimeSpan ts = TimeSpan.FromMilliseconds(bookmark.Position);
                                if (!bookmarkMenuItemsDictionary.ContainsKey(ts)) {
                                    bookmarkMenuItemsDictionary.Add(ts, item);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error reading bookmark file: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ClearBookmarks() {
            foreach (KeyValuePair<TimeSpan, MenuItem> pair in bookmarkMenuItemsDictionary) {
                songMenu.Items.Remove(pair.Value);
            }
            bookmarkMenuItemsDictionary.Clear();
        }

        private void LoadOptions() {
            // Load options for this song
            if (playlist != null) {
                Song song = playlist.CurrentSong;
                string name;
                if (song.Title != null) {
                    name = song.Title;
                }
                else {
                    name = songViewModel.CurrentSongTitle = System.IO.Path.GetFileNameWithoutExtension(song.FileUri.LocalPath);
                }

                string optionsPath = System.IO.Path.Combine(App.songOptionsDirectory, name + ".json");
                if (File.Exists(optionsPath)) {
                    try {
                        using (JsonDocument jsonDocument = JsonDocument.Parse(System.IO.File.ReadAllText(optionsPath))) {
                            JsonElement root = jsonDocument.RootElement;
                            SongOptionsDTO? options = JsonSerializer.Deserialize<SongOptionsDTO>(root);
                            if (options != null) {
                                startingPos = options.StartingPosMilliseconds;
                                endingPos = options.EndingPosMilliseconds;
                                mediaPlayer.Volume = options.Volume;
                            }
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show("Error reading song options file: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else {
                    startingPos = 0;
                    endingPos = 0;
                }
            }
        }

        private void DisableNextTrackButton() {
            nextTrackButton.IsEnabled = false;
            nextTrackButtonImage.Source = App.fastForwardGreyedIcon;
        }

        private void DisablePrevTrackButton() {
            prevTrackButton.IsEnabled = false;
            prevTrackButtonImage.Source = App.rewindGreyedIcon;
        }

        private void EnableNextTrackButton() {
            nextTrackButton.IsEnabled = true;
            nextTrackButtonImage.Source = App.fastForwardIcon;
        }

        private void EnablePrevTrackButton() {
            prevTrackButton.IsEnabled = true;
            prevTrackButtonImage.Source = App.rewindIcon;
        }

        // Media event handlers

        private void OnMediaEnded(object? sender, EventArgs e) {
            if (repeat) { // Repeat song
                mediaPlayer.Position = TimeSpan.FromMilliseconds(startingPos);
            }
            else if (shuffle == true || (playlist != null && playlist.CurrentSongIdx < playlist.Songs.Count - 1)) { // Go next song
                GoNextTrack();
            }
            else { // Stop song and stay put
                PauseMusic();
                UpdatePlayButton();
                mediaPlayer.Position = TimeSpan.FromMilliseconds(startingPos);
            }
        }

        private void OnMediaOpened(object? sender, EventArgs e) {
            loadingBlock.Visibility = Visibility.Hidden;
            menuTrackInformation.IsEnabled = true;
            menuEditBookmarks.IsEnabled = true;
            menuSongOptions.IsEnabled = true;
            ClearBookmarks();
            LoadBookmarks();
            mediaPlayer.Position = TimeSpan.FromMilliseconds(startingPos);
            PlayMusic();
            UpdatePlayButton();
        }

        private void Timer_Tick(object? sender, EventArgs e) {
            UpdateSlider();

            // Check if past ending pos, and end or continue accordingly
            double diff = endingPos - mediaPlayer.Position.TotalMilliseconds;
            if (endingPos != 0 && diff <= 1 && diff >= -1 * tickSpeed) {
                if (repeat) { // Repeat song
                    mediaPlayer.Position = TimeSpan.FromMilliseconds(startingPos);
                }
                else if (playlist != null && playlist.CurrentSongIdx < playlist.Songs.Count - 1) { // Go next song
                    PauseMusic();
                    GoNextTrack();
                }
                else { // Stop song and stay put
                    PauseMusic();
                    UpdatePlayButton();
                    mediaPlayer.Position = TimeSpan.FromMilliseconds(startingPos);
                }
            }
        }

        // Menu bar event handlers

        private void MenuOpen_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Audio Files (*.mp3;*.flac;*.wma;*.avi;*.mid;*.aif;*.wav;*.m4a)|*.mp3;*.flac;*.wma;*.avi;*.mid;*.midi;*.aif;*.wav;*.m4a|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true) {
                SongSlider.Value = 0;

                List<Song> songs = new List<Song>();
                foreach (string filename in openFileDialog.FileNames) {
                    songs.Add(new Song(new Uri(filename)));
                }
                playlist = new Playlist("Untitled playlist", songs);
                OpenNewSong(playlist.CurrentSong.FileUri); // FileName includes path
                //Console.WriteLine(playlist.CurrentSong.FileUri.ToString);
                // song.ViewModel.CurrentSongTitle = openFileDialog.SafeFileName; // SafeFileName excludes path
                UpdateViewModel();
                if (playlist.CurrentSongIdx == playlist.Songs.Count - 1) {
                    DisableNextTrackButton();
                }
                else {
                    EnableNextTrackButton();
                }
                DisablePrevTrackButton();
            }

        }

        private void MenuExit_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void RepeatToggleButton_Click(object sender, RoutedEventArgs e) {
            if (menuRepeat.IsChecked) {
                repeat = true;
            }
            else {
                repeat = false;
            }
        }

        private void MenuTrackInformation_Click(object sender, RoutedEventArgs e) {
            if (playlist == null) { return; }
            TrackInformationWindow trackInformationWindow = new TrackInformationWindow(playlist.CurrentSong);
            trackInformationWindow.ConfirmButtonPressed += OnTrackInformationWindowConfirmButtonPressed;
            trackInformationWindow.Show();
        }

        private void MenuEditBookmarks_Click(object sender, RoutedEventArgs e) {
            string name;
            if (playlist == null) {
                MessageBox.Show("No song is currently playing.", "Error Occured!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Song song = playlist.CurrentSong;
            if (song.Title != null) {
                name = song.Title;
            }
            else {
                name = songViewModel.CurrentSongTitle = System.IO.Path.GetFileNameWithoutExtension(song.FileUri.LocalPath);
            }
            BookmarksEditorWindow bookmarksEditorWindow = new BookmarksEditorWindow(name, ref mediaPlayer);
            bookmarksEditorWindow.ConfirmButtonPressed += OnBookmarksEditorWindowConfirmButtonPressed;
            bookmarksEditorWindow.Show();
        }

        private void MenuSongOptions_Click(object sender, RoutedEventArgs e) {
            string name;
            if (playlist == null) {
                MessageBox.Show("No song is currently playing.", "Error Occured!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Song song = playlist.CurrentSong;
            if (song.Title != null) {
                name = song.Title;
            }
            else {
                name = songViewModel.CurrentSongTitle = System.IO.Path.GetFileNameWithoutExtension(song.FileUri.LocalPath);
            }
            SongOptionsWindow songOptionsWindow = new SongOptionsWindow(name);
            songOptionsWindow.SongOptionsWindowConfirmButtonPressed += OnSongOptionsWindowConfirmButtonPressed;
            songOptionsWindow.Show();
        }

        private void ShuffleToggleButton_Click(object sender, RoutedEventArgs e) {
            shuffle = !shuffle;
            if (playlist != null) {
                if (playlist.CurrentSongIdx == playlist.Songs.Count - 1) {
                    DisableNextTrackButton();
                }
                else {
                    EnableNextTrackButton();
                }
                if (playlist.CurrentSongIdx == 0) {
                    DisablePrevTrackButton();
                }
                else {
                    EnablePrevTrackButton();
                }
            }
        }

        private void MenuEditPlaylists_Click(object sender, RoutedEventArgs e) {
            /*PlaylistWindow playlistWindow = new PlaylistWindow();
            playlistWindow.Show();*/
            PlaylistEditorWindow playlistEditorWindow = new PlaylistEditorWindow();
            playlistEditorWindow.Show();
            playlistEditorWindow.NewPlaylistCreated += OnNewPlaylistCreated;
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e) {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }


        // Button event handlers

        private void PlayButton_Click(object sender, RoutedEventArgs? e) {
            TogglePausePlay();
            UpdatePlayButton();
        }

        private void SeekBackButton_Click(object sender, RoutedEventArgs e) {
            TimeSpan t = TimeSpan.FromMilliseconds(-10000);
            if (mediaPlayer != null && mediaPlayer.NaturalDuration.HasTimeSpan) {
                mediaPlayer.Position += t;
            }
            UpdateSlider();
        }

        private void SeekForwardButton_Click(object sender, RoutedEventArgs e) {
            TimeSpan t = TimeSpan.FromMilliseconds(10000);
            if (mediaPlayer != null && mediaPlayer.NaturalDuration.HasTimeSpan) {
                mediaPlayer.Position += t;
            }
            UpdateSlider();
        }

        private void PrevTrackButton_Click(object sender, RoutedEventArgs e) {
            GoPrevTrack();
        }

        private void NextTrackButton_Click(object sender, RoutedEventArgs e) {
            GoNextTrack();
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            userIsDraggingSlider = true;
            PauseMusic();
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            userIsDraggingSlider = false;
            PlayMusic();
            if (mediaPlayer != null && mediaPlayer.NaturalDuration.HasTimeSpan) {
                TimeSpan t = TimeSpan.FromSeconds(SongSlider.Value);
                mediaPlayer.Position = t;
            }
            UpdateSlider();
        }

        private void SongSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (mediaPlayer != null && mediaPlayer.NaturalDuration.HasTimeSpan) {
                TimeSpan t = TimeSpan.FromSeconds(SongSlider.Value);
                mediaPlayer.Position = t;
            }
            UpdateSlider();
        }

        // CANNOT USE  IsMoveToPointEnabled="True"  because it absorbs click events, and does not work properly due to the slider being updated every tick
        private void SongSlider_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (playlist != null && sender is Slider s) {
                Point mousePosition = e.GetPosition(s);
                double thumbWidth = ((Track)SongSlider.Template.FindName("PART_Track", SongSlider)).Thumb.Width;

                // Adjust for the thumb's width and any potential margins or padding
                double newValPercent = (mousePosition.X - (thumbWidth / 2)) / (s.ActualWidth - thumbWidth);

                // Ensure the value is within valid range [0, 1]
                newValPercent = Math.Max(0, Math.Min(1, newValPercent));

                double newSeconds = playlist.CurrentSong.Duration.TotalSeconds * newValPercent;
                mediaPlayer.Position = TimeSpan.FromSeconds(newSeconds);
                UpdateSlider();
            }
        }

        private void ThumbButtonInfoPlayPause_Click(object sender, EventArgs e) {
            TogglePausePlay();
            UpdatePlayButton();
        }

        // Window event handlers
        // / This window

        // / Other windows
        private async void OnTrackInformationWindowConfirmButtonPressed(object sender, TrackInformationConfirmEventArgs e) {
            if (sender is TrackInformationWindow window) {
                window.ConfirmButtonPressed -= OnTrackInformationWindowConfirmButtonPressed;
            }

            Song song = e.Song;
            if (song != null) {
                Uri tempUri = song.FileUri;
                TimeSpan time = songViewModel.CurrentSongPosition;
                try {
                    mediaPlayer.Stop();
                    mediaPlayer.Close();

                    int tries = 0;
                attempt_save:
                    try {
                        e.Song.TagLibFile.Save();
                    }
                    catch (IOException) {
                        if (++tries >= 10) {
                            throw new Exception("Saving failed after 10 attempts. Please try again.");
                        }
                        await Task.Delay(100);
                        goto attempt_save;
                    }

                    // After closing the song, saving, and reopening the song, seek to the spot the user was at before closing

                    EventHandler? seekToPriorSpot = null;
                    seekToPriorSpot = (sender, _) => {
                        if (mediaPlayer != null) {
                            if (mediaPlayer.NaturalDuration.HasTimeSpan) {
                                mediaPlayer.Position = time;
                            }
                            mediaPlayer.MediaOpened -= seekToPriorSpot;
                        }
                    };
                    mediaPlayer.MediaOpened += seekToPriorSpot;
                    UpdateViewModel();
                    OpenNewSong(tempUri);
                    PlayMusic();

                }
                catch (Exception ex) {
                    MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            TrackInformationWindow? senderWindow = sender as TrackInformationWindow;
            if (senderWindow != null) {
                senderWindow.ConfirmButtonPressed -= OnTrackInformationWindowConfirmButtonPressed;
            }
        }

        private void OnNewPlaylistCreated(object sender, NewPlaylistCreatedEventArgs e) {
            MenuItem item = NewPlaylistMenuItem(e.Name, e.Path);

            if (e.OldName == "") { // New playlist
                if (playlistMenuItemsDictionary.ContainsKey(e.Name)) { // Name of new playlist conflicts
                    //Console.WriteLine(1);
                    playlistMenu.Items.Remove(playlistMenuItemsDictionary[e.Name]);
                    playlistMenu.Items.Add(item);
                    playlistMenuItemsDictionary[e.Name] = item;
                }
                else { // We're good
                    //Console.WriteLine(2);
                    playlistMenu.Items.Add(item);
                    playlistMenuItemsDictionary[e.Name] = item;
                }
            }
            else { // Existing playlist
                if (e.Name != e.OldName) { // Renamed
                    //Console.WriteLine(3);
                    if (playlistMenuItemsDictionary.ContainsKey(e.Name)) {
                        playlistMenu.Items.Remove(playlistMenuItemsDictionary[e.Name]);
                        playlistMenuItemsDictionary.Remove(e.Name);
                    }
                    playlistMenu.Items.Remove(playlistMenuItemsDictionary[e.OldName]);
                    playlistMenuItemsDictionary.Remove(e.OldName);
                    // Delete old json
                    string path = System.IO.Path.Combine(App.playlistsDirectory, e.OldName + ".json");
                    if (System.IO.File.Exists(path)) {
                        System.IO.File.Delete(path);
                    }
                    playlistMenu.Items.Add(item);
                    playlistMenuItemsDictionary[e.Name] = item;
                }
                else { // Same name
                    //Console.WriteLine(4);
                    playlistMenu.Items.Remove(playlistMenuItemsDictionary[e.OldName]);
                    playlistMenuItemsDictionary.Remove(e.OldName);
                    playlistMenu.Items.Add(item);
                    playlistMenuItemsDictionary[e.Name] = item;
                }
            }

            PlaylistEditorWindow? senderWindow = sender as PlaylistEditorWindow;
            if (senderWindow != null) {
                senderWindow.NewPlaylistCreated -= OnNewPlaylistCreated;
            }
        }

        private void OnBookmarksEditorWindowConfirmButtonPressed(object? sender, EventArgs e) {
            if (sender is BookmarksEditorWindow window) {
                window.ConfirmButtonPressed -= OnBookmarksEditorWindowConfirmButtonPressed;
            }
            ClearBookmarks();
            LoadBookmarks();
        }
        private void OnSongOptionsWindowConfirmButtonPressed(object? sender, EventArgs e) {
            if (sender is SongOptionsWindow window) {
                window.SongOptionsWindowConfirmButtonPressed -= OnSongOptionsWindowConfirmButtonPressed;
            }
            LoadOptions();
        }

        private void MainFileDragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.Copy;
            }
            else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void MainFileDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                List<Song> songs = new List<Song>();
                foreach (string file in files) {
                    songs.Add(new Song(new Uri(file)));
                }
                playlist = new Playlist("Untitled playlist", songs);
                OpenNewSong(playlist.CurrentSong.FileUri);
                UpdateViewModel();
                if (playlist.CurrentSongIdx == playlist.Songs.Count - 1) {
                    DisableNextTrackButton();
                }
                else {
                    EnableNextTrackButton();
                }
                DisablePrevTrackButton();

            }
        }


    }
}
