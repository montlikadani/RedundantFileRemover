using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RedundantFileRemover.UserSettingsData;

namespace RedundantFileRemover {

    public partial class SettingsContent : Window {

        private readonly MainWindow main;
        private readonly FileAttributesTab fileAttributesTab;

        private readonly string[] protectedDirectories = {
            Environment.GetFolderPath(Environment.SpecialFolder.Windows)
        };

        public SettingsContent() {
            InitializeComponent();

            foreach (Window window in Application.Current.Windows) {
                if (window.GetType() == typeof(MainWindow)) {
                    main = window as MainWindow;
                    break;
                }
            }

            #region Load saved data into memory
            searchInSubDirs.IsChecked = FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories;
            errorLogging.IsChecked = FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging;
            alwaysClearLogs.IsChecked = FileDataReader.ProgramSettings.SettingsWindow.AlwaysClearLogs;
            moveFilesToBin.IsChecked = FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin;

            if (errorLogging.IsChecked == false) {
                alwaysClearLogs.Visibility = Visibility.Hidden;
            }
            #endregion

            // Protect some system directories
            foreach (string p in protectedDirectories) {
                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Add(p);
            }

            errorLogging.Checked += ErrorLoggingEnabled_CheckedChanged;
            errorLogging.Unchecked += ErrorLoggingEnabled_CheckedChanged;

            fileAttributesTab = new();
            fileAttributesTab.Visibility = Visibility.Hidden;
        }

        private void Settings_Load(object sender, RoutedEventArgs e) {
            filterList.Items.Clear();

            foreach (string a in FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Distinct()) {
                if (!Directory.Exists(a)) {
                    return;
                }

                ListBoxItem item = new() {
                    Content = a
                };

                if (IsProtected(a)) {
                    item.IsEnabled = false;
                    item.ToolTip = "You can not remove this special directory";
                }

                filterList.Items.Add(item);
            }

            removeFilters.IsEnabled = filterList.Items.Count > 1;
        }

        private void Settings_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            Visibility = Visibility.Hidden;

            Settings_Closed(sender, e);

            if (main != null) {

                // ShowErrors button in main window
                if (errorLogging.IsChecked == false) {
                    main.showErrors.IsEnabled = false;
                    main.showErrors.Visibility = Visibility.Hidden;
                } else {
                    main.showErrors.IsEnabled = true;
                    main.showErrors.Visibility = Visibility.Visible;
                }
            }
        }

        private void Settings_Closed(object sender, EventArgs e) {
            FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging = (bool) errorLogging.IsChecked;
            FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories = (bool) searchInSubDirs.IsChecked;

            FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Clear();

            foreach (ListBoxItem item in filterList.Items) {
                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Add(item.Content.ToString());
            }

            FileDataReader.ProgramSettings.SettingsWindow.AlwaysClearLogs = (bool) alwaysClearLogs.IsChecked;
            FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin = (bool) moveFilesToBin.IsChecked;
        }

        private void FileAttributesTab_Click(object sender, RoutedEventArgs e) {
            Visibility = Visibility.Hidden;
            fileAttributesTab.SettingsPage = this;
            fileAttributesTab.Visibility = Visibility.Visible;
        }

        private FolderBrowserEx.FolderBrowserDialog folderBrowserDialog;

        private void BrowseFiltersButton_Click(object sender, RoutedEventArgs e) {
            if (folderBrowserDialog == null) {
                folderBrowserDialog = new() {
                    AllowMultiSelect = true
                };
            }

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                error.Content = "";

                foreach (string selectedPath in folderBrowserDialog.SelectedFolders) {
                    if (main != null) {
                        if (main.IsDirectoryInIgnoredList(selectedPath)) {
                            return;
                        }

                        if (selectedPath == main.folderPath.Text) {
                            error.Content = "Error: The path can not be the same with the selected folder(s)!";
                            return;
                        }
                    }

                    filterList.Items.Add(new ListBoxItem() {
                        Content = selectedPath
                    });
                    FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Add(selectedPath);
                }

                if (filterList.Items.Count > 1) {
                    removeFilters.IsEnabled = true;
                }
            }

            // Bring this settings window to front foreground
            // There is a glitch or idk what when another dialog opens the main window pops up instead of keeping
            // the original one
            Activate();
        }

        private void RemoveFilters_Click(object sender, RoutedEventArgs e) {
            foreach (ListBoxItem item in new System.Collections.ArrayList(filterList.SelectedItems.Count == 0 ? filterList.Items : filterList.SelectedItems)) {
                string content = item.Content.ToString();

                if (IsProtected(content)) {
                    continue;
                }

                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Remove(content);
                filterList.Items.Remove(item);
            }

            if (filterList.Items.Count == 1) {
                removeFilters.IsEnabled = false;
            }
        }

        private bool IsProtected(string ob) {
            foreach (string s in protectedDirectories) {
                if (ob.Equals(s)) {
                    return true;
                }
            }

            return false;
        }

        private void FilterList_selected(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count != 0) {
                removeFilters.Content = "Remove selected";

                // Remove selection from protected directories
                foreach (ListBoxItem item in e.AddedItems) {
                    if (IsProtected(item.Content.ToString())) {
                        filterList.SelectedItems.Remove(item);
                    }
                }
            } else {
                removeFilters.Content = "Remove all";
            }
        }

        private void FilterList_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            filterList.ContextMenu = null;

            if (filterList.SelectedItems.Count == 0) {
                return;
            }

            if (e.ChangedButton != System.Windows.Input.MouseButton.Right && e.ChangedButton != System.Windows.Input.MouseButton.Left) {
                filterList.SelectedItems.Clear();
                return;
            }

            bool hoveringItem = false;

            // Checks if the mouse is over the list item (hovering) to append the menu items on right click
            for (int i = 0; i < filterList.Items.Count; i++) {
                if (filterList.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem lbi
                        && System.Windows.Media.VisualTreeHelper.GetDescendantBounds(lbi).Contains(e.GetPosition(lbi))) {
                    hoveringItem = true;
                    break;
                }
            }

            if (!hoveringItem) {
                filterList.SelectedItems.Clear();
                return;
            }

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left) {
                return;
            }

            ContextMenu cm = new();

            MenuItem item = new();
            item.Header = "Remove from list";
            item.Click += OnMenuItemClicked;
            cm.Items.Add(item);

            item = new();
            item.Header = "Open in file explorer";
            item.Click += OnMenuItemClicked;
            cm.Items.Add(item);

            cm.IsOpen = true;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            filterList.ContextMenu = cm;
        }

        private void OnMenuItemClicked(object sender, RoutedEventArgs e) {
            if (filterList.SelectedItems.Count == 0 || sender is not MenuItem menuItem) {
                return;
            }

            switch (menuItem.Header.ToString()) {
                case "Open in file explorer":
                    try {
                        System.Diagnostics.Process.Start("explorer.exe", "/select," + (filterList.SelectedItem as ListBoxItem).Content.ToString());
                    } catch (Exception ex) {
                        MainWindow.LogException(ex.Message + " " + ex.StackTrace);
                        MessageBox.Show(ex.Message, "Error while opening the file explorer", MessageBoxButton.OK);
                    }

                    break;
                case "Remove from list":
                    ListBoxItem selectedItem = filterList.SelectedItem as ListBoxItem;
                    string content = selectedItem.Content.ToString();

                    if (!IsProtected(content)) {
                        FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Remove(content);
                        filterList.Items.Remove(selectedItem);

                        if (filterList.Items.Count == 1) {
                            removeFilters.IsEnabled = false;
                        }
                    }

                    break;
                default:
                    break;
            }

            if (menuItem.ContextMenu != null) {
                menuItem.ContextMenu.IsOpen = false;
            }

            filterList.SelectedItems.Clear();
        }

        private void ErrorLoggingEnabled_CheckedChanged(object sender, RoutedEventArgs e) {
            alwaysClearLogs.Visibility = errorLogging.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
        }

        // Remove contextMenu when clicking outside of listbox
        private void WindowMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.Source is not ListBoxItem) {
                filterList.SelectedItems.Clear();

                if (filterList.ContextMenu != null) {
                    filterList.ContextMenu.IsOpen = false;
                    filterList.ContextMenu = null;
                }
            }
        }
    }
}
