using System.Windows;
using System.Windows.Controls;

namespace RedundantFileRemover {
    public partial class FindWindow : Window {

        public static MainWindow MainInstance { private get; set; }

        public FindWindow() {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void findKey_TextChanged(object sender, TextChangedEventArgs e) {
            findButton.IsEnabled = findKey.Text.Trim().Length != 0;
        }

        private void findButton_Click(object sender, RoutedEventArgs e) {
            FindKey();
        }

        private void findKey_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == System.Windows.Input.Key.Enter) {
                FindKey();
            }
        }

        private void FindKey() {
            foreach (ListBoxItem item in MainInstance.logs.Items) {
                if (item.Content.ToString().Contains(findKey.Text)) {
                    Close();
                    MainInstance.logs.SelectedItem = item;
                    MainInstance.logs.ScrollIntoView(item);
                    return;
                }
            }

            MessageBox.Show(this, "Key not found", "No key found with the specified name", MessageBoxButton.OK);
        }
    }
}
