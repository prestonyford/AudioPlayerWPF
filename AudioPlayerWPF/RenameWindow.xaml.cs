using AudioPlayerWPF.Classes;
using System.Windows;

namespace AudioPlayerWPF
{
    /// <summary>
    /// Interaction logic for RenameWindow.xaml
    /// </summary>
    public partial class RenameWindow : Window
    {
        public RenameWindow(BookmarkDTO bookmark) {
            SourceInitialized += (sender, e) => {
                if (Properties.Settings.Default.DarkMode) {
                    App.ChangeWindowDarkMode(this, true);
                }
            };

            groupBox.Header = $"Rename: {bookmark.Name}";
            textBox.Text = bookmark.Name;
            textBox.Focus();
            confirmButton.Click += (sender, e) => {
                bookmark.Name = textBox.Text;
                Close();
            };
            
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
