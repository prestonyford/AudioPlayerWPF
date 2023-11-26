using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AudioPlayerWPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        // Static stuff
        public static readonly BitmapImage windowCloseIcon = new BitmapImage(new Uri("/Images/icons8-close-gray-48.png", UriKind.Relative));
        public static readonly BitmapImage windowMinimizeIcon = new BitmapImage(new Uri("/Images/icons8-minimize-gray-48.png", UriKind.Relative));
        public static readonly BitmapImage noimgImage = new BitmapImage(new Uri("/Images/icons8-audio-wave.png", UriKind.Relative));
        public static readonly string userSubdirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AudioPlayerWPF");
        public static readonly string playlistsDirectory = Path.Combine(userSubdirectory, "playlists");
        public static readonly string bookmarksDirectory = Path.Combine(userSubdirectory, "bookmarks");
        public static readonly string songOptionsDirectory = Path.Combine(userSubdirectory, "song_options");
        public static readonly BitmapImage playlistIcon = new BitmapImage(new Uri("/Images/icons8-playlist-48.png", UriKind.Relative));
        public static readonly BitmapImage playIcon = new BitmapImage(new Uri("/Images/icons8-play-48.png", UriKind.Relative));
        public static readonly BitmapImage editIcon = new BitmapImage(new Uri("/Images/icons8-edit-48.png", UriKind.Relative));
        public static readonly BitmapImage deleteIcon = new BitmapImage(new Uri("/Images/icons8-delete-48.png", UriKind.Relative));
        public static readonly BitmapImage songIcon = new BitmapImage(new Uri("/Images/icons8-music-48.png", UriKind.Relative));
        public static readonly BitmapImage bookmarkIcon = new BitmapImage(new Uri("/Images/icons8-bookmark-48.png", UriKind.Relative));
        public static readonly BitmapImage fastForwardIcon = new BitmapImage(new Uri("/Images/icons8-fast-forward-48.png", UriKind.Relative));
        public static readonly BitmapImage rewindIcon = new BitmapImage(new Uri("/Images/icons8-rewind-48.png", UriKind.Relative));
        public static readonly BitmapImage fastForwardGreyedIcon = new BitmapImage(new Uri("/Images/icons8-fast-forward-greyed-48.png", UriKind.Relative));
        public static readonly BitmapImage rewindGreyedIcon = new BitmapImage(new Uri("/Images/icons8-rewind-greyed-48.png", UriKind.Relative));

        public App() {
            if (!Directory.Exists(userSubdirectory)) {
                Directory.CreateDirectory(userSubdirectory);
            }
            if (!Directory.Exists(userSubdirectory + "\\playlists")) {
                Directory.CreateDirectory(userSubdirectory + "\\playlists");
            }
            if (!Directory.Exists(userSubdirectory + "\\bookmarks")) {
                Directory.CreateDirectory(userSubdirectory + "\\bookmarks");
            }
            if (!Directory.Exists(userSubdirectory + "\\song_options")) {
                Directory.CreateDirectory(userSubdirectory + "\\song_options");
            }
            SetDropDownMenuToBeRightAligned();
        }

        public static void SetDropDownMenuToBeRightAligned() {
            var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            Action setAlignmentValue = () => {
                if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null) menuDropAlignmentField.SetValue(null, false);
            };

            setAlignmentValue();

            SystemParameters.StaticPropertyChanged += (sender, e) => {
                setAlignmentValue();
            };
        }

        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref bool attrValue, int attrSize);
        public static void ChangeWindowDarkMode(Window window, bool darkMode) {
            DwmSetWindowAttribute(new System.Windows.Interop.WindowInteropHelper(window).Handle, 20, ref darkMode, System.Runtime.InteropServices.Marshal.SizeOf(darkMode));
        }
    }
}
