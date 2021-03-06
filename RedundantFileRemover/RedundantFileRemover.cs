﻿using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Sys­tem.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using RedundantFileRemover.UserSettingsData;
using Microsoft.VisualBasic.FileIO;

namespace RedundantFileRemover {
    public partial class RedundantFileRemover : Form {

        private readonly SettingsForm settings = new();

        public static ErrorViewer ErrorViewer { get; } = new ErrorViewer();
        public static bool TerminateRequested { get; set; } = false;

        private readonly List<DirectoryInfo> emptyDirectories = new();
        private readonly List<FileInfo> emptyFilesList = new();

        private static readonly string nl = Environment.NewLine;

        public RedundantFileRemover() {
            InitializeComponent();

            FormClosing += new FormClosingEventHandler(Form_Closing);
            Application.ApplicationExit += new EventHandler(Form_Exit);

            #region Load data from cache
            folderPath.Text = FileDataReader.ProgramSettings.MainWindow.FolderPath;
            searchEmptyFolders.Checked = FileDataReader.ProgramSettings.MainWindow.SearchEmptyFolders;
            searchEmptyFiles.Checked = FileDataReader.ProgramSettings.MainWindow.SearchEmptyFiles;
            patternFileTypes.Text = FileDataReader.ProgramSettings.MainWindow.PatternFileTypes;
            showErrors.Enabled = showErrors.Visible = FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging;
            autoScroll.Checked = FileDataReader.ProgramSettings.MainWindow.AutoScroll;

            if ((!searchEmptyFiles.Checked || patternFileTypes.Text == "") && !searchEmptyFolders.Checked) {
                searchButton.Enabled = false;
            }
            #endregion
        }

        void Form_Closing(object sender, FormClosingEventArgs e) {
            FileDataReader.ProgramSettings.MainWindow.SearchEmptyFolders = searchEmptyFolders.Checked;
            FileDataReader.ProgramSettings.MainWindow.SearchEmptyFiles = searchEmptyFiles.Checked;
            FileDataReader.ProgramSettings.MainWindow.PatternFileTypes = patternFileTypes.Text;
            FileDataReader.ProgramSettings.MainWindow.FolderPath = folderPath.Text;
            FileDataReader.ProgramSettings.MainWindow.AutoScroll = autoScroll.Checked;
        }

        void Form_Exit(object sender, EventArgs e) {
            Program.ConfigFile.SaveAllSettings();
        }

        private void browseButton_Click(object sender, EventArgs e) {
            var openFolder = new FolderBrowserDialog() {
                ShowNewFolderButton = false
            };

            if (openFolder.ShowDialog().HasFlag(DialogResult.OK)) {
                // Note: do not check for file attributes as on some systems, the C:\ drive have hidden attribute by default, (why?)

                folderPath.Text = openFolder.SelectedPath;
            }

            openFolder.Dispose();
        }

        private async void searchButton_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(folderPath.Text)) {
                return;
            }

            var directoryInfo = new DirectoryInfo(folderPath.Text);
            if (!directoryInfo.Exists) {
                MessageBox.Show(this, "File/directory not exists");
                return;
            }

            logs.Items.Clear();

            Form waitingWindow = new WaitingWindow(this);
            waitingWindow.Show();
            Enabled = false; // Disable main window while the waiting window is open

            #region Disabling and clearing some options
            TerminateRequested = false;
            UseWaitCursor = true;
            searchButton.Enabled = browseButton.Enabled = removeAll.Enabled = false;
            stopTask.Enabled = true;
            searchEmptyFolders.Enabled = searchEmptyFiles.Enabled = patternFileTypes.Enabled = settingsMenu.Enabled = clearButton.Enabled = false;
            logs.SelectionMode = SelectionMode.None;
            if (settings.alwaysClearLogs.Checked) {
                ErrorViewer.errorLogs.Clear();
            }
            removedFilesList.Clear();
            emptyFilesList.Clear();
            emptyDirectories.Clear();
            removedAmount.Text = "";
            if (searchEmptyFolders.Checked) {
                removedAmount.Text = "0 empty folders";
            }
            if (searchEmptyFiles.Checked) {
                removedAmount.Text += " 0 empty files";
            }
            #endregion

            // Wait and cache all empty files and directories into memory
            List<FileSystemInfo> collectedFiles = searchEmptyFiles.Checked ? new List<FileSystemInfo>(await FileUtils.GetFiles(directoryInfo, patternFileTypes.Text, false))
                : new List<FileSystemInfo>();
            List<FileSystemInfo> collectedDirs = searchEmptyFolders.Checked ? await FileUtils.GetFiles(directoryInfo, "*", true) : new List<FileSystemInfo>();

            if (!waitingWindow.IsDisposed) {
                waitingWindow.Dispose();
                waitingWindow.Close();
                Focus(); // Get focus on main window
            }

            Enabled = true;
            TerminateRequested = false;

            Graphics g = logs.CreateGraphics();
            int lastStringWidth = 0;

            if (searchEmptyFiles.Checked) {
                foreach (FileSystemInfo fileSystemInfo in collectedFiles) {
                    if (TerminateRequested) {
                        break;
                    }

                    if (fileSystemInfo is not FileInfo fileInfo || FileUtils.IsFileHasAttribute(fileInfo, FileAttributes.Hidden, FileAttributes.System)
                        || settings.IsDirectoryInIgnoredList(fileInfo.FullName)
                        || FileUtils.IsFileLocked(fileInfo.FullName)) {
                        continue; // There are chances when files attributes are changed during search
                    }

                    if (File.Exists(fileInfo.FullName) && fileInfo.Length <= 0) {
                        emptyFilesList.Add(fileInfo);
                        removedAmount.Text = (searchEmptyFolders.Checked ? emptyDirectories.Count + " empty folders, " : "") + emptyFilesList.Count + " empty files";

                        logs.Items.Add(fileInfo.FullName);

                        // Make visible horizontal scroll bar if path is too long
                        int sWidth = (int) g.MeasureString(logs.Items[logs.Items.Count - 1].ToString(), logs.Font).Width;
                        if (sWidth > logs.Size.Width && lastStringWidth < sWidth) {
                            logs.HorizontalExtent = sWidth;
                            lastStringWidth = sWidth;
                        }

                        // Auto-scroll calculating
                        if (autoScroll.Checked) {
                            logs.TopIndex = logs.Items.Count - (logs.Height / logs.ItemHeight + 2); // + 2 to avoid "fast flickering"
                        }
                    }

                    await DoAsync(5);
                }
            }

            if (searchEmptyFolders.Checked) {
                foreach (FileSystemInfo fileSystemInfo in collectedDirs) {
                    if (TerminateRequested) {
                        break;
                    }

                    if (fileSystemInfo is not DirectoryInfo dInfo
                        || FileUtils.IsFileHasAttribute(dInfo, FileAttributes.System, FileAttributes.Hidden) || settings.IsDirectoryInIgnoredList(dInfo.FullName)) {
                        continue; // There are chances when files attributes are changed during search
                    }

                    try {
                        if (Directory.Exists(dInfo.FullName) && !Directory.EnumerateFileSystemEntries(dInfo.FullName).Any()
                                && dInfo.GetFiles().Length == 0 && dInfo.GetDirectories().Length == 0) {
                            emptyDirectories.Add(dInfo);
                            removedAmount.Text = emptyDirectories.Count + " empty folders" + (searchEmptyFiles.Checked ? ", " + emptyFilesList.Count + " empty files" : "");

                            logs.Items.Add(dInfo.FullName);

                            // Make horizontal scroll bar if path is too long
                            int sWidth = (int) g.MeasureString(logs.Items[logs.Items.Count - 1].ToString(), logs.Font).Width;
                            if (sWidth > logs.Size.Width && lastStringWidth < sWidth) {
                                logs.HorizontalExtent = sWidth;
                                lastStringWidth = sWidth;
                            }

                            // Auto-scroll calculating
                            if (autoScroll.Checked) {
                                logs.TopIndex = logs.Items.Count - (logs.Height / logs.ItemHeight + 2); // + 2 to avoid "fast flickering"
                            }
                        }
                    } catch (Exception ex) {
                        LogException(ex.Message + " " + ex.StackTrace);
                    }

                    await DoAsync(5);
                }
            }

            if (!TerminateRequested && emptyFilesList.Count == 0 && emptyDirectories.Count == 0) {
                DialogResult result = MessageBox.Show(this, "There is no any empty " + (searchEmptyFiles.Checked ? "file " : "")
                    + (searchEmptyFolders.Checked ? "and folder " : "") + ".", "Empty directory", MessageBoxButtons.OK);
                if (!result.HasFlag(DialogResult.OK)) {
                    return;
                }
            }

            #region Re-enabling options
            if (emptyDirectories.Count > 0 || emptyFilesList.Count > 0) {
                removeAll.Enabled = true;
            }

            if (logs.Items.Count > 0) {
                clearButton.Enabled = true;
            }

            logs.SelectionMode = SelectionMode.MultiExtended;
            UseWaitCursor = false;
            searchButton.Enabled = settingsMenu.Enabled = browseButton.Enabled = searchEmptyFolders.Enabled = searchEmptyFiles.Enabled = patternFileTypes.Enabled = true;
            stopTask.Enabled = false;
            #endregion
        }

        private void stopTask_Click(object sender, EventArgs e) {
            TerminateRequested = true;
            stopTask.Enabled = false;
        }

        private void clearButton_Click(object sender, EventArgs e) {
            logs.Items.Clear();
            removedFilesList.Clear();

            removedFilesList.Visible = clearButton.Enabled = removeAll.Enabled = false;
            removedAmount.Text = "";
            logs.HorizontalExtent = 0; // Reset horizontal scroll bar to default value
        }

        private void removeAll_Click(object sender, EventArgs e) {
            bool moveFilesToRecycleBin = FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin;

            DialogResult result = MessageBox.Show("Are you sure that you want to remove all files?" + nl + nl
                + "Selected action: " + (moveFilesToRecycleBin ? "Move to recycle bin" : "Remove entirely"),
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (!result.HasFlag(DialogResult.Yes)) {
                return;
            }

            removedFilesList.Visible = true;

            emptyFilesList.ForEach(file => {
                if (File.Exists(file.FullName) && file.Length <= 0 && !FileUtils.IsFileLocked(file.FullName)) {
                    try {
                        if (moveFilesToRecycleBin) {
                            FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
                        } else {
                            file.Delete();
                        }
                    } catch (Exception ex) {
                        LogException(ex.Message + " " + ex.StackTrace);
                        return;
                    }

                    removedFilesList.AppendText("File " + (moveFilesToRecycleBin ? "moved to recycle bin" : "removed") + ": " + file.FullName);
                    removedFilesList.AppendText(nl);
                }
            });

            emptyDirectories.ForEach(dir => {
                if (Directory.Exists(dir.FullName) && dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0) {
                    try {
                        if (moveFilesToRecycleBin) {
                            FileSystem.DeleteDirectory(dir.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
                        } else {
                            dir.Delete(false);
                        }
                    } catch (Exception ex) {
                        LogException(ex.Message + " " + ex.StackTrace);
                        return;
                    }

                    removedFilesList.AppendText("Directory " + (moveFilesToRecycleBin ? "moved to recycle bin" : "removed") + ": " + dir.FullName);
                    removedFilesList.AppendText(nl);
                }
            });

            emptyFilesList.Clear();
            emptyDirectories.Clear();

            removeAll.Enabled = false;
            removedAmount.Text = "";
            clearButton.Enabled = false;
        }

        // Both empty folders and files are called
        private void searchFiles_CheckedChanged(object sender, EventArgs e) {
            patternFileTypes.Visible = searchEmptyFiles.Checked;
            searchButton.Enabled = searchEmptyFolders.Checked || (searchEmptyFiles.Checked && patternFileTypes.Text != "");
        }

        private void exitItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void settingsMenu_Click(object sender, EventArgs e) {
            AddOwnedForm(settings);
            settings.ShowDialog();
        }

        private void patternFileTypes_TextChanged(object sender, EventArgs e) {
            searchButton.Enabled = patternFileTypes.Text != "";
        }

        private void showErrors_Click(object sender, EventArgs e) {
            AddOwnedForm(ErrorViewer);
            ErrorViewer.ShowDialog();
        }

        private void logsFilesBox_MouseDown(object sender, MouseEventArgs e) {
            logs.ContextMenuStrip = null;

            if (e.Button.HasFlag(MouseButtons.Left) && logs.IndexFromPoint(e.Location) == -1) {
                logs.ClearSelected();
            }

            if (logs.SelectedItems.Count == 0 || !e.Button.HasFlag(MouseButtons.Right)) {
                return;
            }

            ContextMenuStrip cms = new();
            cms.Click += OnLogsFilesBoxClicked;

            cms.Items.Add("Move to recycle bin");
            cms.Items.Add(logs.SelectedItems.Count == 1 ? "Remove" : "Remove all selected ones");
            cms.Items.Add("Add directory to ignore list");
            cms.Items.Add("Open in file explorer");
            cms.Items.Add("Hide from list");

            if (sender is Control control) {
                cms.Show(this, new Point(e.X + control.Left, e.Y + control.Top));
            }

            logs.ContextMenuStrip = cms;
        }

        private void OnLogsFilesBoxClicked(object sender, EventArgs e) {
            if (e is MouseEventArgs mouse && mouse.Button.HasFlag(MouseButtons.Left) && logs.SelectedItems.Count > 0) {
                ContextMenuStrip cms = sender as ContextMenuStrip;
                object selectedItem = logs.SelectedItem;

                try {
                    var toolStripItem = cms?.GetItemAt(mouse.Location);
                    if (toolStripItem is not null) {
                        if (toolStripItem.Text == "Open in file explorer") {
                            try {
                                string si = selectedItem.ToString();
                                System.Diagnostics.Process.Start("explorer.exe", File.Exists(si) ? new FileInfo(si).DirectoryName : si);
                            } catch (Exception ex) {
                                LogException(ex.Message + " " + ex.StackTrace);
                                MessageBox.Show("File not found", "The file to open does not exist: " + ex.Message, MessageBoxButtons.OK);
                            }
                        } else if (toolStripItem.Text == "Remove" || toolStripItem.Text == "Remove all selected ones" || toolStripItem.Text == "Move to recycle bin") {
                            RecycleOption recycleOption = toolStripItem.Text.Contains("Remove") ? RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin;

                            if (logs.SelectedItems.Count > 1 && toolStripItem.Text == "Remove all selected ones") {
                                foreach (var item in logs.SelectedItems) {
                                    string si = item.ToString();

                                    try {
                                        if (Directory.Exists(si)) {
                                            FileSystem.DeleteDirectory(si, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
                                        } else if (File.Exists(si)) {
                                            FileSystem.DeleteFile(si, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
                                        }
                                    } catch (Exception ex) {
                                        LogException(ex.Message + " " + ex.StackTrace);
                                    }
                                }

                                for (int i = logs.SelectedIndices.Count - 1; i >= 0; i--) {
                                    logs.Items.RemoveAt(logs.SelectedIndices[i]);
                                }
                            } else {
                                string si = selectedItem.ToString();
                                if (Directory.Exists(si)) {
                                    FileSystem.DeleteDirectory(si, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
                                } else if (File.Exists(si)) {
                                    FileSystem.DeleteFile(si, UIOption.OnlyErrorDialogs, recycleOption, UICancelOption.ThrowException);
                                }

                                logs.Items.Remove(selectedItem);
                            }

                            if (logs.Items.Count == 0) {
                                emptyFilesList.Clear();
                                emptyDirectories.Clear();

                                removeAll.Enabled = clearButton.Enabled = false;
                                removedAmount.Text = "";
                            }
                        } else if (toolStripItem.Text == "Add directory to ignore list") {
                            string directory = selectedItem.ToString();
                            if (File.Exists(directory)) {
                                directory = new FileInfo(directory).DirectoryName;
                            }

                            if (Directory.Exists(directory)) {
                                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Add(directory);
                            }
                        } else if (toolStripItem.Text == "Hide from list") {
                            string name = selectedItem.ToString();

                            int amount = 0;
                            if ((amount = emptyFilesList.RemoveAll(fInfo => fInfo.FullName.Equals(name))) == 0) {
                                amount = emptyDirectories.RemoveAll(dInfo => dInfo.FullName.Equals(name));
                            }

                            if (amount > 0) {
                                logs.Items.Remove(selectedItem);

                                if (logs.Items.Count == 0) {
                                    removeAll.Enabled = clearButton.Enabled = false;
                                    removedAmount.Text = "";
                                }
                            }
                        }
                    }
                } catch (Exception exc) {
                    LogException(exc.Message + " " + exc.StackTrace);
                }

                if (cms is not null) {
                    cms.Close();
                }
            }

            logs.ClearSelected();
        }

        public static void LogException(string s) {
            if (!FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging || string.IsNullOrWhiteSpace(s) || ActiveForm is null) {
                return;
            }

            ActiveForm.Invoke((MethodInvoker) delegate {
                ErrorViewer.errorLogs.AppendText(s + nl + nl);
            });
        }

        public static void ChangeProgressBar(int count) {
            if (count < 0) {
                count = 1;
            }

            // maybe better calculation?
            count /= 10;

            while (count > 100) {
                count /= 10;
            }

            var lastForm = Application.OpenForms[Application.OpenForms.Count - 1];
            if (lastForm is not null) {
                for (int i = 0; i < lastForm.Controls.Count; i++) {
                    if (lastForm.Controls[i] is ProgressBar pb) {
                        pb.Invoke((MethodInvoker) delegate {
                            pb.Value = count;
                        });

                        break;
                    }
                }
            }
        }

        private async Task DoAsync(int ms) {
            await Task.Delay(ms);
        }
    }
}
