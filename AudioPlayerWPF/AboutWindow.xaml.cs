using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;

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
                    App.ChangeWindowDarkMode(this, true);
                }
            };
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
