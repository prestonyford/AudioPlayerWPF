using System;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Windows.Media.Imaging;
using TagLib;

namespace AudioPlayerWPF.Classes
{
    public class Song {
        private TagLib.File? tagFile;
        private Uri fileUri;

        public Song(Uri uri) {
            fileUri = uri;
            
        }
        ~Song() {
            // Console.WriteLine($"Disposing tags for: {FileName}");
            if (tagFile != null ) {
                tagFile.Dispose();
            }
        }

        // Helpers
        private TagLib.File createTag() {
            tagFile = TagLib.File.Create(fileUri.LocalPath.ToString());
            return tagFile;
        }

        // Properties
        public Uri FileUri { get { return fileUri; } }
        public TagLib.File TagLibFile {
            get {
                if (tagFile == null) {
                    return createTag();
                }
                return tagFile;
            }
        }

        public BitmapImage? CoverArt {
            get {
                if (tagFile == null) {
                    createTag();
                }
                if (tagFile == null) {
                    return null;
                }
                IPicture? tagAlbumArt = tagFile.Tag.Pictures.FirstOrDefault();
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

        public string FileUriString { get { return fileUri.LocalPath.ToString(); } }
        public string FileName { get { return System.IO.Path.GetFileName(fileUri.LocalPath); } }

    }
}
