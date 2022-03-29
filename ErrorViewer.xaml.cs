using System.Windows;

namespace RedundantFileRemover {

    public partial class ErrorViewer : Window {

        public ErrorViewer() {
            InitializeComponent();
        }

        private void ErrorLogs_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            if (errorLogs.SelectionLength + 50 >= errorLogs.MaxLength) {
                errorLogs.Clear(); // Auto clean up
            }
        }

        private void ClearErrorLog_Click(object sender, RoutedEventArgs e) {
            errorLogs.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
    }
}
