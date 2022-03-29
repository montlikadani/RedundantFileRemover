using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

using Microsoft.VisualBasic.FileIO;

using RedundantFileRemover.UserSettingsData;
using RedundantFileRemover.Utils;

namespace RedundantFileRemover {
    public partial class MainWindow : Window {

        private static FileDataReader fdr;
        public static FileDataReader ConfigFile => fdr;

        private static readonly string nl = Environment.NewLine;

        public static bool TerminateRequested { get; set; } = false;

        public static ErrorViewer ErrorViewer { get; } = new ErrorViewer();

        private readonly SettingsContent settings;
        private readonly FindWindow findWindow;

        private readonly List<DirectoryInfo> emptyDirectories = new();
        private readonly List<FileInfo> emptyFilesList = new();

        private bool searchStarted = false;

        public MainWindow() {
            fdr = new FileDataReader();

            InitializeComponent();

            #region Load data from settings cache
            folderPath.Text = FileDataReader.ProgramSettings.MainWindow.FolderPath;
            searchEmptyFolders.IsChecked = FileDataReader.ProgramSettings.MainWindow.SearchEmptyFolders;
            searchEmptyFiles.IsChecked = FileDataReader.ProgramSettings.MainWindow.SearchEmptyFiles;
            patternFileTypes.Text = FileDataReader.ProgramSettings.MainWindow.PatternFileTypes;
            autoScroll.IsChecked = FileDataReader.ProgramSettings.MainWindow.AutoScroll;
            allFileType.IsChecked = FileDataReader.ProgramSettings.MainWindow.LookThroughAllFileType;

            if (!(showErrors.IsEnabled = FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging)) {
                showErrors.Visibility = Visibility.Hidden;
            }

            if ((searchEmptyFiles.IsChecked == false || patternFileTypes.Text == "") && searchEmptyFolders.IsChecked == false) {
                searchButton.IsEnabled = false;
            }

            if (searchEmptyFiles.IsChecked == false) {
                allFileType.Visibility = patternFileTypes.Visibility = Visibility.Hidden;
            }
            #endregion

            patternFileTypes.TextChanged += PatternFileTypes_TextChanged;

            searchEmptyFiles.Checked += SearchFiles_CheckedChanged;
            searchEmptyFolders.Checked += SearchFiles_CheckedChanged;
            searchEmptyFiles.Unchecked += SearchFiles_CheckedChanged;
            searchEmptyFolders.Unchecked += SearchFiles_CheckedChanged;
            allFileType.Checked += allFileType_CheckedChanged;
            allFileType.Unchecked += allFileType_CheckedChanged;

            settings = new();
            findWindow = new();
        }

        public bool IsDirectoryInIgnoredList(string dir) {
            foreach (string ig in FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories) {
                if (ig.Equals(dir)) {
                    return true;
                }
            }

            return false;
        }

        void Window_Closing(object sender, CancelEventArgs e) {
            FileDataReader.ProgramSettings.MainWindow.SearchEmptyFolders = (bool) searchEmptyFolders.IsChecked;
            FileDataReader.ProgramSettings.MainWindow.SearchEmptyFiles = (bool) searchEmptyFiles.IsChecked;
            FileDataReader.ProgramSettings.MainWindow.PatternFileTypes = patternFileTypes.Text;
            FileDataReader.ProgramSettings.MainWindow.FolderPath = folderPath.Text;
            FileDataReader.ProgramSettings.MainWindow.AutoScroll = (bool) autoScroll.IsChecked;
            FileDataReader.ProgramSettings.MainWindow.LookThroughAllFileType = (bool) allFileType.IsChecked;
        }

        void Window_Closed(object sender, EventArgs e) {
            ConfigFile.SaveAllSettings();
            System.Windows.Application.Current.Shutdown();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e) {
            var openFolder = new FolderBrowserDialog() {
                ShowNewFolderButton = false
            };

            if (openFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                folderPath.Text = openFolder.SelectedPath;
            }

            openFolder.Dispose();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(folderPath.Text)) {
                return;
            }

            var directoryInfo = new DirectoryInfo(folderPath.Text);
            if (!directoryInfo.Exists) {
                System.Windows.MessageBox.Show(this, "File/directory does not exists");
                return;
            }

            bool isSearchEmptyFiles = searchEmptyFiles.IsChecked == true;
            bool isSearchEmptyFolders = searchEmptyFolders.IsChecked == true;

            #region Disabling and clearing some options
            logs.Items.Clear();

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            progressBarProcess.Visibility = statusLabel.Visibility = Visibility.Visible;
            progressBarProcess.Value = 0;

            TerminateRequested = allFileType.IsEnabled = findKeyButton.IsEnabled = searchButton.IsEnabled = browseButton.IsEnabled = removeAll.IsEnabled = false;
            searchEmptyFolders.IsEnabled = searchEmptyFiles.IsEnabled = patternFileTypes.IsEnabled = settingsMenu.IsEnabled = clearButton.IsEnabled = false;
            searchStarted = stopTask.IsEnabled = true;

            if (settings.alwaysClearLogs.IsChecked == true) {
                ErrorViewer.errorLogs.Clear();
            }

            removedFilesList.Clear();
            emptyFilesList.Clear();
            emptyDirectories.Clear();

            statusLabel.Content = removedAmount.Content = "";

            if (isSearchEmptyFolders) {
                removedAmount.Content = "0 empty folders";
            }

            if (isSearchEmptyFiles) {
                removedAmount.Content += " 0 empty files";
            }
            #endregion

            bool lookThroughAllFileType = allFileType.IsChecked == true;

            string[] split = null;
            if (!lookThroughAllFileType && isSearchEmptyFiles) {
                split = patternFileTypes.Text.Replace(" ", "").Split(',');
            }

            await Task.Factory.StartNew(() => {
                if (isSearchEmptyFiles) {
                    if (lookThroughAllFileType) {
                        CountFiles(directoryInfo, isSearchEmptyFolders, true);
                        CollectFiles(directoryInfo, "", isSearchEmptyFolders, true);
                    } else if (split.Length != 0) {
                        for (int a = 0; a < split.Length; a++) {
                            if (TerminateRequested) {
                                return;
                            }

                            CountFiles(directoryInfo, isSearchEmptyFolders, true);
                        }

                        foreach (string sp in split) {
                            if (TerminateRequested) {
                                return;
                            }

                            CollectFiles(directoryInfo, sp, isSearchEmptyFolders, true);
                        }
                    } else {
                        CountFiles(directoryInfo, isSearchEmptyFolders, true);
                        CollectFiles(directoryInfo, "", isSearchEmptyFolders, true);
                    }
                } else if (isSearchEmptyFolders) {
                    CountFiles(directoryInfo, true, false);
                    CollectFiles(directoryInfo, null, true, false);
                }
            });

            if (!TerminateRequested) {
                if (emptyFilesList.Count == 0 && emptyDirectories.Count == 0) {
                    progressBarProcess.Value = 0;
                    statusLabel.Content = "Status: Done";

                    MessageBoxResult result = System.Windows.MessageBox.Show(this, "There is no any empty " + (isSearchEmptyFiles ? "file" : "")
                        + (isSearchEmptyFolders ? " and folder" : "") + ".", "Empty directory", MessageBoxButton.OK);

                    if (result != MessageBoxResult.OK) {
                        return;
                    }
                } else {
                    progressBarProcess.Value = progressBarProcess.Maximum;
                    statusLabel.Content = "Status: Done";
                }
            } else {
                progressBarProcess.Value = progressBarProcess.Maximum;
                statusLabel.Content = "Status: Cancelled";
            }

            #region Re-enabling options
            if (emptyDirectories.Count != 0 || emptyFilesList.Count != 0) {
                removeAll.IsEnabled = true;
            }

            if (logs.Items.Count > 40) {
                findKeyButton.IsEnabled = true;
            }

            if (logs.Items.Count != 0) {
                clearButton.IsEnabled = true;
            }

            Mouse.OverrideCursor = null;
            collectedFilesAmount = 0;

            allFileType.IsEnabled = searchButton.IsEnabled = settingsMenu.IsEnabled = browseButton.IsEnabled = searchEmptyFolders.IsEnabled = searchEmptyFiles.IsEnabled
                = patternFileTypes.IsEnabled = true;
            searchStarted = stopTask.IsEnabled = false;
            #endregion
        }

        private int collectedFilesAmount = 0;

        private void CountFiles(DirectoryInfo directoryInfo, bool isSearchEmptyFolders, bool isSearchEmptyFiles) {
            if (TerminateRequested) {
                return;
            }

            IEnumerable<DirectoryInfo> directories = GetTopDirectories(directoryInfo);

            if (directories == null) {
                return;
            }

            if (isSearchEmptyFolders) {
                collectedFilesAmount += directories.Count();
            }

            foreach (DirectoryInfo dirInfo in directories) {
                if (TerminateRequested) {
                    return;
                }

                if (isSearchEmptyFiles) {
                    try {
                        collectedFilesAmount += dirInfo.EnumerateFiles("*", System.IO.SearchOption.TopDirectoryOnly).Count();
                    } catch (Exception ex) {
                        LogException(ex.StackTrace + " " + ex.Message);
                    }
                }

                if (FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories) {
                    CountFiles(dirInfo, isSearchEmptyFolders, isSearchEmptyFiles);
                }
            }
        }

        private void CollectFiles(DirectoryInfo directoryInfo, string searchPattern, bool isSearchEmptyFolders, bool isSearchEmptyFiles) {
            if (TerminateRequested) {
                return;
            }

            IEnumerable<DirectoryInfo> directories = GetTopDirectories(directoryInfo);

            if (directories == null) {
                return;
            }

            foreach (DirectoryInfo dirInfo in directories) {
                if (TerminateRequested) {
                    return;
                }

                try {
                    Dispatcher.Invoke(() => statusLabel.Content = "Status: " + dirInfo.FullName);

                    if (isSearchEmptyFolders) {
                        if (!Directory.EnumerateFileSystemEntries(dirInfo.FullName).Any()) {
                            emptyDirectories.Add(dirInfo);

                            Dispatcher.Invoke(() => {
                                removedAmount.Content = emptyDirectories.Count + " empty folders" + (searchEmptyFiles.IsChecked == true ?
                                    " " + emptyFilesList.Count + " empty files" : "");

                                logs.Items.Add(new ListBoxItem() {
                                    Content = dirInfo.FullName
                                });

                                if (autoScroll.IsChecked == true) {
                                    logs.Items.MoveCurrentToLast();
                                    logs.ScrollIntoView(logs.Items.CurrentItem);
                                }

                                if (progressBarProcess.Maximum != collectedFilesAmount) {
                                    progressBarProcess.Maximum = collectedFilesAmount;
                                }
                            });
                        }

                        // Increase progress bar value on each iteration
                        Dispatcher.Invoke(() => progressBarProcess.Value++);
                    }

                    if (isSearchEmptyFiles) {
                        var f = dirInfo.EnumerateFiles("*", System.IO.SearchOption.TopDirectoryOnly);

                        // Increase progress bar value with all files found
                        Dispatcher.Invoke(() => progressBarProcess.Value += f.Count());

                        IEnumerable<FileInfo> files;

                        if (searchPattern.Length != 0) {
                            files = f.Where(file => file.Length == 0 && file.Extension.Equals(searchPattern)
                                && !FileUtils.IsFileHasAttribute(file, FileDataReader.ProgramSettings.SettingsWindow.IgnoredFileAttributes) && !FileUtils.IsFileLocked(file.FullName));
                        } else {
                            files = f.Where(file => file.Length == 0 && !FileUtils.IsFileHasAttribute(file, FileDataReader.ProgramSettings.SettingsWindow.IgnoredFileAttributes)
                                && !FileUtils.IsFileLocked(file.FullName));
                        }

                        Dispatcher.Invoke(() => {
                            if (progressBarProcess.Maximum != collectedFilesAmount) {
                                progressBarProcess.Maximum = collectedFilesAmount;
                            }

                            foreach (FileInfo fileInfo in files) {
                                if (TerminateRequested) {
                                    return;
                                }

                                emptyFilesList.Add(fileInfo);

                                removedAmount.Content = (searchEmptyFolders.IsChecked == true ? emptyDirectories.Count + " empty folders " : "")
                                    + emptyFilesList.Count + " empty files";

                                logs.Items.Add(new ListBoxItem() {
                                    Content = fileInfo.FullName
                                });

                                if (autoScroll.IsChecked == true) {
                                    logs.Items.MoveCurrentToLast();
                                    logs.ScrollIntoView(logs.Items.CurrentItem);
                                }
                            }
                        });
                    }

                    if (FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories) {
                        CollectFiles(dirInfo, searchPattern, isSearchEmptyFolders, isSearchEmptyFiles);
                    }
                } catch (Exception ex) {
                    LogException(ex.StackTrace + " " + ex.Message);
                }
            }
        }

        private void StopTask_Click(object sender, RoutedEventArgs e) {
            TerminateRequested = true;
            stopTask.IsEnabled = false;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) {
            progressBarProcess.Visibility = statusLabel.Visibility = Visibility.Hidden;

            logs.Items.Clear();
            removedFilesList.Clear();

            if (!(findKeyButton.IsEnabled = clearButton.IsEnabled = removeAll.IsEnabled = false)) {
                removedFilesList.Visibility = Visibility.Hidden;
            }

            removedAmount.Content = "";
        }

        private void RemoveAll_Click(object sender, RoutedEventArgs e) {
            bool moveFilesToRecycleBin = FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin;

            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure that you want to remove all files?" + nl + nl
                + "Selected action: " + (moveFilesToRecycleBin ? "Move to recycle bin" : "Remove entirely"),
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) {
                return;
            }

            removedFilesList.Visibility = Visibility.Visible;

            emptyFilesList.ForEach(file => {
                if (File.Exists(file.FullName) && file.Length == 0 && !FileUtils.IsFileLocked(file.FullName)) {
                    try {
                        if (moveFilesToRecycleBin) {
                            FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
                        } else {
                            file.Delete();
                        }
                    } catch (Exception ex) {
                        if (ex is OperationCanceledException) {
                            return;
                        }

                        LogException(ex.Message + " " + ex.StackTrace);
                    }

                    removedFilesList.AppendText("File " + (moveFilesToRecycleBin ? "moved to recycle bin" : "removed") + ": " + file.FullName);
                    removedFilesList.AppendText(nl);
                }
            });

            emptyDirectories.ForEach(dir => {
                if (!Directory.Exists(dir.FullName) || dir.EnumerateDirectories().Count() != 0 || dir.EnumerateFiles().Count() != 0) {
                    return;
                }

                try {
                    if (moveFilesToRecycleBin) {
                        FileSystem.DeleteDirectory(dir.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
                    } else {
                        dir.Delete(false);
                    }
                } catch (Exception ex) {
                    if (ex is OperationCanceledException) {
                        return;
                    }

                    LogException(ex.Message + " " + ex.StackTrace);
                }

                removedFilesList.AppendText("Directory " + (moveFilesToRecycleBin ? "moved to recycle bin" : "removed") + ": " + dir.FullName);
                removedFilesList.AppendText(nl);
            });

            emptyFilesList.Clear();
            emptyDirectories.Clear();

            removedAmount.Content = "";
            clearButton.IsEnabled = findKeyButton.IsEnabled = removeAll.IsEnabled = false;
        }

        private void findKeyButton_Click(object sender, RoutedEventArgs e) {
            FindWindow.MainInstance = this;
            findWindow.ShowDialog();
        }

        // Both empty folders and files are called
        private void SearchFiles_CheckedChanged(object sender, RoutedEventArgs e) {
            allFileType.Visibility = patternFileTypes.Visibility = searchEmptyFiles.IsChecked == false ? Visibility.Hidden : Visibility.Visible;

            if (searchEmptyFolders.IsChecked == true) {
                if (searchEmptyFiles.IsChecked == true) {
                    searchButton.IsEnabled = allFileType.IsChecked == true || patternFileTypes.Text.Trim().Length != 0;
                } else {
                    searchButton.IsEnabled = true;
                }
            } else if (searchEmptyFiles.IsChecked == true) {
                searchButton.IsEnabled = allFileType.IsChecked == true || patternFileTypes.Text.Trim().Length != 0;
            } else {
                searchButton.IsEnabled = false;
            }
        }

        private void allFileType_CheckedChanged(object sender, RoutedEventArgs e) {
            if (searchEmptyFiles.IsChecked == true) {
                searchButton.IsEnabled = allFileType.IsChecked == true || patternFileTypes.Text.Trim().Length != 0;
            }
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }

        private void OpenSettingsFile_Click(object sender, RoutedEventArgs e) {
            string settingsFile = "userConfigData.yml";

            if (File.Exists(settingsFile)) {
                System.Diagnostics.Process.Start(new FileInfo(settingsFile).FullName);
            }
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e) {
            settings.ShowDialog();
        }

        private void PatternFileTypes_TextChanged(object sender, TextChangedEventArgs e) {
            searchButton.IsEnabled = allFileType.IsChecked == true || patternFileTypes.Text.Trim().Length != 0;
        }

        private void ShowErrors_Click(object sender, RoutedEventArgs e) {
            ErrorViewer.ShowDialog();
        }

        private void Logs_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (searchStarted) {
                logs.SelectedItems.Clear();
            }
        }

        private void Logs_MouseUp(object sender, MouseButtonEventArgs e) {
            logs.ContextMenu = null;

            if (logs.SelectedItems.Count == 0) {
                return;
            }

            if (e.ChangedButton != MouseButton.Right && e.ChangedButton != MouseButton.Left) {
                logs.SelectedItems.Clear();
                return;
            }

            bool hoveringItem = false;

            // Checks if the mouse is over the list item (hovering) to append the menu items on right click
            for (int i = 0; i < logs.Items.Count; i++) {
                if (logs.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem lbi
                        && System.Windows.Media.VisualTreeHelper.GetDescendantBounds(lbi).Contains(e.GetPosition(lbi))) {
                    hoveringItem = true;
                    break;
                }
            }

            if (!hoveringItem) {
                logs.SelectedItems.Clear();
                return;
            }

            if (e.ChangedButton == MouseButton.Left) {
                return;
            }

            bool selectedOne = logs.SelectedItems.Count == 1;
            System.Windows.Controls.ContextMenu cm = new();

            System.Windows.Controls.MenuItem item = new();
            item.Header = (selectedOne ? "Move" : "Move all") + " to recycle bin";
            item.Click += OnMenuItemClicked;
            cm.Items.Add(item);

            item = new();
            item.Header = selectedOne ? "Remove" : "Remove all selected ones";
            item.Click += OnMenuItemClicked;
            cm.Items.Add(item);

            if (selectedOne) {
                item = new();
                item.Header = "Add directory to ignore list";
                item.Click += OnMenuItemClicked;
                cm.Items.Add(item);

                item = new();
                item.Header = "Open in file explorer";
                item.Click += OnMenuItemClicked;
                cm.Items.Add(item);

                item = new();
                item.Header = "Hide from list";
                item.Click += OnMenuItemClicked;
                cm.Items.Add(item);
            }

            cm.IsOpen = true;
            cm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            logs.ContextMenu = cm;
        }

        private void OnMenuItemClicked(object sender, RoutedEventArgs e) {
            if (logs.SelectedItems.Count == 0 || sender is not System.Windows.Controls.MenuItem menuItem) {
                return;
            }

            string itemHeader = menuItem.Header.ToString();

            switch (itemHeader) {
                case "Open in file explorer":
                    try {
                        string si = (logs.SelectedItem as ListBoxItem).Content.ToString();
                        System.Diagnostics.Process.Start("explorer.exe", "/select," + (File.Exists(si) ? new FileInfo(si).DirectoryName : si));
                    } catch (Exception ex) {
                        LogException(ex.Message + " " + ex.StackTrace);
                        System.Windows.MessageBox.Show("Error", "Error occurred during opening the file: " + ex.Message, MessageBoxButton.OK);
                    }

                    break;
                case "Remove":
                case "Remove all selected ones":
                case "Move to recycle bin":
                case "Move all to recycle bin":
                    RecycleOption recycleOption = (itemHeader == "Move to recycle bin" || itemHeader == "Move all to recycle bin")
                        ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently;

                    if (logs.SelectedItems.Count > 1) {
                        foreach (ListBoxItem item in new System.Collections.ArrayList(logs.SelectedItems)) {
                            string si = item.Content.ToString();

                            try {
                                if (Directory.Exists(si)) {
                                    FileSystem.DeleteDirectory(si, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
                                } else if (File.Exists(si)) {
                                    FileSystem.DeleteFile(si, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
                                } else {
                                    continue;
                                }
                            } catch (Exception ex) {
                                if (ex is OperationCanceledException) {
                                    continue;
                                }

                                LogException(ex.Message + " " + ex.StackTrace);
                            }

                            logs.Items.Remove(item);
                        }
                    } else {
                        ListBoxItem selItem = logs.SelectedItem as ListBoxItem;
                        string si = selItem.Content.ToString();

                        try {
                            if (Directory.Exists(si)) {
                                FileSystem.DeleteDirectory(si, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
                            } else if (File.Exists(si)) {
                                FileSystem.DeleteFile(si, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
                            } else {
                                return;
                            }
                        } catch (Exception ex) {
                            if (ex is OperationCanceledException) {
                                return;
                            }

                            LogException(ex.Message + " " + ex.StackTrace);
                        }

                        logs.Items.Remove(selItem);
                    }

                    if (logs.Items.Count == 0) {
                        emptyFilesList.Clear();
                        emptyDirectories.Clear();

                        removeAll.IsEnabled = clearButton.IsEnabled = false;
                        removedAmount.Content = "";
                    }

                    break;
                case "Add directory to ignore list":
                    string directory = (logs.SelectedItem as ListBoxItem).Content.ToString();

                    if (File.Exists(directory)) {
                        directory = new FileInfo(directory).DirectoryName;
                    }

                    if (Directory.Exists(directory)) {
                        FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Add(directory);
                    }

                    break;
                case "Hide from list":
                    ListBoxItem selectedItem = logs.SelectedItem as ListBoxItem;
                    string name = selectedItem.Content.ToString();

                    int amount;
                    if ((amount = emptyFilesList.RemoveAll(fInfo => fInfo.FullName.Equals(name))) == 0) {
                        amount = emptyDirectories.RemoveAll(dInfo => dInfo.FullName.Equals(name));
                    }

                    if (amount != 0) {
                        logs.Items.Remove(selectedItem);

                        if (logs.Items.Count == 0) {
                            removeAll.IsEnabled = clearButton.IsEnabled = false;
                            removedAmount.Content = "";
                        }
                    }

                    break;
                default:
                    break;
            }

            if (menuItem.ContextMenu != null) {
                menuItem.ContextMenu.IsOpen = false;
            }

            logs.SelectedItems.Clear();
        }

        private IEnumerable<DirectoryInfo> GetTopDirectories(DirectoryInfo directoryInfo) {
            try {
                return directoryInfo.EnumerateDirectories("*", System.IO.SearchOption.TopDirectoryOnly)
                    .Where(d => !IsDirectoryInIgnoredList(d.FullName))
                    .Where(d => !FileUtils.IsFileHasAttribute(d, FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectoryAttributes));
            } catch (Exception ex) {
                LogException(ex.StackTrace + " " + ex.Message);
            }

            return null;
        }

        public static void LogException(string s) {
            if (!FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging || string.IsNullOrWhiteSpace(s)) {
                return;
            }

            ErrorViewer.Dispatcher.Invoke(() => ErrorViewer.errorLogs.AppendText(s + nl + nl));
        }
    }
}
