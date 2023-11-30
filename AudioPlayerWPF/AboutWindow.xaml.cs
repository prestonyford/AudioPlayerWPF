using System;
using System.Diagnostics;
using System.Windows;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudioPlayerWPF {
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow() {
            InitializeComponent();

            SourceInitialized += (sender, e) => {
                if (Properties.Settings.Default.DarkMode) {
                    App.ChangeTitleBarDarkMode(true);
                }
            };

            Update_Changelog();
        }

        private async void Update_Changelog() {
            using (HttpClient httpClient = new HttpClient()) {
                string url = "https://raw.githubusercontent.com/prestonyford/AudioPlayerWPF/main/changelog.md";
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode) {
                    string content = await response.Content.ReadAsStringAsync();
                    ChangelogBox.Text = content;
                }
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Hyperlink_RequestNavigateTagLib(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void FileStorageHyperlink_Click(object sender, RoutedEventArgs e) {
            Process.Start("explorer.exe", App.userSubdirectory);
            e.Handled = true;
        }
    }
}
