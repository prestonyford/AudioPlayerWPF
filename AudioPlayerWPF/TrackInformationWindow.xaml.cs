using AudioPlayerWPF.Classes;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using TagLib;

namespace AudioPlayerWPF {
    /// <summary>
    /// Interaction logic for TrackInformationWindow.xaml
    /// </summary>
    public partial class TrackInformationWindow : Window
    {
        private Song song;
        string fileExtension;

        public delegate void ConfirmButtonPressedEventHandler(object sender, TrackInformationConfirmEventArgs e);
        public event ConfirmButtonPressedEventHandler? ConfirmButtonPressed;

        public TrackInformationWindow(Song song) {
            InitializeComponent();
            SourceInitialized += (sender, e) => {
                if (Properties.Settings.Default.DarkMode) {
                    App.ChangeTitleBarDarkMode(true);
                }
            };

            this.song = song;

            double length = new FileInfo(song.FileUri.LocalPath).Length;
            fileSize.Text = (length / (double)(1024.0 * 1024.0)).ToString("N2") + " MB";
            audioFormat.Text = song.TagLibFile.Properties.Description;
            fileExtension = System.IO.Path.GetExtension(song.FileUri.LocalPath).ToLower();

            metadataContainer.Text = song.TagLibFile.Tag.TagTypes.ToString();
            bitrate.Text = song.TagLibFile.Properties.AudioBitrate.ToString() + " KB/s";
            channels.Text = song.TagLibFile.Properties.AudioChannels.ToString();
            sampleRate.Text = song.TagLibFile.Properties.AudioSampleRate.ToString() + " Hz";

            txtBoxTitle.Text = song.TagLibFile.Tag.Title;
            txtBoxArtist.Text = string.Join(", ", song.TagLibFile.Tag.Performers);
            txtBoxAlbum.Text = song.TagLibFile.Tag.Album;
            txtBoxAlbumArtist.Text = string.Join(", ", song.TagLibFile.Tag.AlbumArtists);
            txtBoxGenre.Text = string.Join(", ", song.TagLibFile.Tag.Genres);
            txtBoxTrackNumber.Text = song.TagLibFile.Tag.Track.ToString();
            txtBoxDiscNumber.Text = song.TagLibFile.Tag.Disc.ToString();
            txtBoxYear.Text = song.TagLibFile.Tag.Year.ToString();

            if (song.CoverArt != null) {
                albumArtImage.Source = song.CoverArt;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
            if (song == null) {
                MessageBox.Show("null song.Info", "Error Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }
            // TagLib.Id3v2.Tag id3v2Tag = (TagLib.Id3v2.Tag)song.TagLibFile.GetTag(TagTypes.Id3v2); // mp3
            // id3v2Tag.Title = txtBoxTitle.Text;

            // var file = TagLib.File.Create(si.FileUri.LocalPath);

            song.TagLibFile.Tag.Title = txtBoxTitle.Text;
            song.TagLibFile.Tag.Performers = txtBoxArtist.Text.Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries);
            song.TagLibFile.Tag.Album = txtBoxAlbum.Text;
            song.TagLibFile.Tag.AlbumArtists = txtBoxAlbumArtist.Text.Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries);
            song.TagLibFile.Tag.Genres = txtBoxGenre.Text.Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries);
            if (txtBoxTrackNumber.Text != "") {
                song.TagLibFile.Tag.Track = uint.Parse(txtBoxTrackNumber.Text);
            }
            if (txtBoxDiscNumber.Text != "") {
                song.TagLibFile.Tag.Disc = uint.Parse(txtBoxDiscNumber.Text);
            }
            if (txtBoxDiscNumber.Text != "") {
                song.TagLibFile.Tag.Year = uint.Parse(txtBoxDiscNumber.Text);
            }

            ConfirmButtonPressed?.Invoke(this, new TrackInformationConfirmEventArgs(song)); // Handled in MainWindow.xaml.cs in order to stop the music before saving
            
            Close();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            /*openFileDialog.Filter = "Image Files (*png;*jpg;*.jpeg)|*png;*jpg;*.jpeg|All files (*.*)|*.*";*/
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.tiff|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true) {
                try {
                    BitmapImage coverArt = new BitmapImage(new Uri(openFileDialog.FileName));

                    byte[] imageBytes;
                    using (MemoryStream ms = new MemoryStream()) {
                        string fileExtension = System.IO.Path.GetExtension(openFileDialog.FileName).ToLower();
                        BitmapEncoder encoder;
                        if (fileExtension == ".png") {
                            encoder = new PngBitmapEncoder();
                        }
                        else if (fileExtension == ".jpg" || fileExtension == ".jpeg") {
                            encoder = new JpegBitmapEncoder();
                        }
                        else if (fileExtension == ".bmp") {
                            encoder = new BmpBitmapEncoder();
                        }
                        else if (fileExtension == ".tiff") {
                            encoder = new TiffBitmapEncoder();
                        }
                        else {
                            throw new Exception("Unexpected file extension. Please use either PNG, JPEG, BMP, or TIFF.");
                        }

                        encoder.Frames.Add(BitmapFrame.Create(coverArt));
                        encoder.Save(ms);
                        imageBytes = ms.ToArray();
                    }
                    IPicture picture = new Picture(new ByteVector(imageBytes));
                    song.TagLibFile.Tag.Pictures = new IPicture[] { picture };

                    browseURITextBlock.Text = openFileDialog.FileName;
                    albumArtImage.Source = coverArt;
                }
                catch (Exception ex) {
                    MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception Occured!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            /*MessageBoxResult result = MessageBox.Show("Are you sure you want to close the window?", "Confirm Close", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) {
                e.Cancel = true;
                return;
            }*/
            ConfirmButtonPressed = null;
        }
    }
}
