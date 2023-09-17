using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using TagLib;

namespace AudioPlayerWPF.Classes
{
    public class Song {
        private TagLib.File audioFile;
        private Uri fileUri;

        // FOR JSON USE
        //private int lengthSeconds;

        public Song(Uri uri) {
            fileUri = uri;
            audioFile = TagLib.File.Create(uri.LocalPath.ToString());
        }
        public Song(string uri) {
            fileUri = new Uri(uri);
            audioFile = TagLib.File.Create(fileUri.LocalPath.ToString());
        }
        ~Song() {
            Console.WriteLine($"Disposing tags for: {FileName}");
            audioFile.Dispose();
        }

        // Properties

        public Uri FileUri { get { return fileUri; } }
        public TagLib.File TagLibFile { get { return audioFile; } }
        public string? Title { get { return audioFile.Tag.Title; } }

        public BitmapImage? CoverArt {
            get {
                IPicture? tagAlbumArt = audioFile.Tag.Pictures.FirstOrDefault();
                if (tagAlbumArt != null) {
                    MemoryStream ms = new MemoryStream(tagAlbumArt.Data.Data);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
                return null;
            }
        }
        public string? Album { get { return audioFile.Tag.Album; } }
        public string? Artist { get { return string.Join(", ", audioFile.Tag.Performers); } }

        public string? AlbumArtist { get { return string.Join(", ", audioFile.Tag.AlbumArtists); } }
        public TimeSpan Duration { get { return audioFile.Properties.Duration; } }

        public string FileUriString { get { return fileUri.LocalPath.ToString(); } }
        public string FileName { get { return System.IO.Path.GetFileName(fileUri.LocalPath); } }

        // 

        public override string ToString() {
            return "test!";
        }
    }
}
