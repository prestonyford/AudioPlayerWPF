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
            InitializeComponent();
            DataContext = this;
            SourceInitialized += (sender, e) => {
                if (Properties.Settings.Default.DarkMode) {
                    App.ChangeTitleBarDarkMode(true);
                }
            };
            groupBox.Header = $"Rename: {bookmark.Name}";
            textBox.Text = bookmark.Name;
            textBox.Focus();
            confirmButton.Click += (sender, e) => {
                bookmark.Name = textBox.Text;
                Close();
            };
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
