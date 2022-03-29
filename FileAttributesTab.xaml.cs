using RedundantFileRemover.UserSettingsData;

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RedundantFileRemover {

    public partial class FileAttributesTab : Window {

        public SettingsContent SettingsPage { private get; set; }

        public FileAttributesTab() {
            InitializeComponent();

            foreach (FileAttributes fileAttribute in Enum.GetValues(typeof(FileAttributes))) {
                attributeList.Items.Add(fileAttribute);
                directoryAttributeList.Items.Add(fileAttribute);
            }

            foreach (FileAttributes attr in FileDataReader.ProgramSettings.SettingsWindow.IgnoredFileAttributes) {
                ignoredFileAttributes.Items.Add(attr);
            }

            foreach (FileAttributes attr in FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectoryAttributes) {
                ignoredDirectoryAttributes.Items.Add(attr);
            }
        }

        private void GeneralTab_Click(object sender, RoutedEventArgs e) {
            Visibility = Visibility.Hidden;
            SettingsPage.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            Visibility = Visibility.Hidden;

            FileDataReader.ProgramSettings.SettingsWindow.IgnoredFileAttributes = ignoredFileAttributes.Items.OfType<FileAttributes>().ToArray();
            FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectoryAttributes = ignoredDirectoryAttributes.Items.OfType<FileAttributes>().ToArray();
        }

        // TODO Merge these 2 type of IgnoredAttribute option somehow to only have 1 method for each type

        // File attributes start

        private void attributeList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!ignoredFileAttributes.Items.Contains(attributeList.SelectedItem)) {
                ignoredFileAttributes.Items.Add(attributeList.SelectedItem);
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e) {
            if (attributeList.SelectedItem != null) {
                ignoredFileAttributes.Items.Remove(attributeList.SelectedItem);
                removeButton.IsEnabled = false;
            }
        }

        private void ignoredFileAttributes_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            ignoredFileAttributes.ContextMenu = null;

            if (ignoredFileAttributes.SelectedItem == null) {
                removeButton.IsEnabled = false;
                return;
            }

            if (e.ChangedButton != System.Windows.Input.MouseButton.Right && e.ChangedButton != System.Windows.Input.MouseButton.Left) {
                ignoredFileAttributes.SelectedItem = null;
                removeButton.IsEnabled = false;
                return;
            }

            bool hoveringItem = false;

            // Checks if the mouse is over the list item (hovering) to append the menu items on right click
            for (int i = 0; i < ignoredFileAttributes.Items.Count; i++) {
                if (ignoredFileAttributes.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem lbi
                        && System.Windows.Media.VisualTreeHelper.GetDescendantBounds(lbi).Contains(e.GetPosition(lbi))) {
                    hoveringItem = true;
                    break;
                }
            }

            if (!hoveringItem) {
                ignoredFileAttributes.SelectedItem = null;
                removeButton.IsEnabled = false;
                return;
            }

            removeButton.IsEnabled = true;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left) {
                return;
            }

            ContextMenu cm = new();

            MenuItem item = new();
            item.Header = "Remove";
            item.Click += OnMenuItemClicked;
            cm.Items.Add(item);

            cm.IsOpen = true;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            ignoredFileAttributes.ContextMenu = cm;
        }

        private void OnMenuItemClicked(object sender, RoutedEventArgs e) {
            if (ignoredFileAttributes.SelectedItem == null || sender is not MenuItem menuItem) {
                return;
            }

            switch (menuItem.Header.ToString()) {
                case "Remove":
                    ignoredFileAttributes.Items.Remove(ignoredFileAttributes.SelectedItem);
                    removeButton.IsEnabled = false;
                    break;
                default:
                    break;
            }

            if (menuItem.ContextMenu != null) {
                menuItem.ContextMenu.IsOpen = false;
            }
        }

        // File attributes end

        // Directory attributes start

        private void directoryAttributeList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!ignoredDirectoryAttributes.Items.Contains(directoryAttributeList.SelectedItem)) {
                ignoredDirectoryAttributes.Items.Add(directoryAttributeList.SelectedItem);
            }
        }

        private void directoryRemoveButton_Click(object sender, RoutedEventArgs e) {
            if (directoryAttributeList.SelectedItem != null) {
                ignoredDirectoryAttributes.Items.Remove(directoryAttributeList.SelectedItem);
                directoryRemoveButton.IsEnabled = false;
            }
        }

        private void ignoredDirectoryAttributes_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            ignoredDirectoryAttributes.ContextMenu = null;

            if (ignoredDirectoryAttributes.SelectedItem == null) {
                directoryRemoveButton.IsEnabled = false;
                return;
            }

            if (e.ChangedButton != System.Windows.Input.MouseButton.Right && e.ChangedButton != System.Windows.Input.MouseButton.Left) {
                ignoredDirectoryAttributes.SelectedItem = null;
                directoryRemoveButton.IsEnabled = false;
                return;
            }

            bool hoveringItem = false;

            // Checks if the mouse is over the list item (hovering) to append the menu items on right click
            for (int i = 0; i < ignoredDirectoryAttributes.Items.Count; i++) {
                if (ignoredDirectoryAttributes.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem lbi
                        && System.Windows.Media.VisualTreeHelper.GetDescendantBounds(lbi).Contains(e.GetPosition(lbi))) {
                    hoveringItem = true;
                    break;
                }
            }

            if (!hoveringItem) {
                ignoredDirectoryAttributes.SelectedItem = null;
                directoryRemoveButton.IsEnabled = false;
                return;
            }

            directoryRemoveButton.IsEnabled = true;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Left) {
                return;
            }

            ContextMenu cm = new();

            MenuItem item = new();
            item.Header = "Remove";
            item.Click += OnDirMenuItemClicked;
            cm.Items.Add(item);

            cm.IsOpen = true;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            ignoredDirectoryAttributes.ContextMenu = cm;
        }

        private void OnDirMenuItemClicked(object sender, RoutedEventArgs e) {
            if (ignoredDirectoryAttributes.SelectedItem == null || sender is not MenuItem menuItem) {
                return;
            }

            switch (menuItem.Header.ToString()) {
                case "Remove":
                    ignoredDirectoryAttributes.Items.Remove(ignoredDirectoryAttributes.SelectedItem);
                    directoryRemoveButton.IsEnabled = false;
                    break;
                default:
                    break;
            }

            if (menuItem.ContextMenu != null) {
                menuItem.ContextMenu.IsOpen = false;
            }
        }

        // Directory attributes end
    }
}
