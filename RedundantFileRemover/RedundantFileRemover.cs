using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Sys­tem.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;
using RedundantFileRemover.UserSettingsData;

namespace RedundantFileRemover {
    public partial class RedundantFileRemover : Form {

        private bool paused = false;

        private readonly SettingsForm settings = new SettingsForm();

        public static ErrorViewer ErrorViewer { get; } = new ErrorViewer();

        private readonly List<DirectoryInfo> emptyDirectories = new List<DirectoryInfo>();
        private readonly List<FileInfo> emptyFilesList = new List<FileInfo>();

        private static readonly string nl = Environment.NewLine;

        public RedundantFileRemover() {
            InitializeComponent();

            FormClosing += new FormClosingEventHandler(Form_Closing);
            Application.ApplicationExit += new EventHandler(Form_Exit);

            #region Load data from cache
            folderPath.Text = FileDataReader.ProgramSettings.MainWindow.FolderPath;
            onlyFoundFiles.Checked = FileDataReader.ProgramSettings.MainWindow.PrintOnlyFoundFiles;
            searchEmptyFolders.Checked = FileDataReader.ProgramSettings.MainWindow.SearchEmptyFolders;
            searchEmptyFiles.Checked = FileDataReader.ProgramSettings.MainWindow.SearchEmptyFiles;
            patternFileTypes.Text = FileDataReader.ProgramSettings.MainWindow.PatternFileTypes;
            showErrors.Visible = FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging;
            autoScroll.Checked = FileDataReader.ProgramSettings.MainWindow.AutoScroll;

            if ((!searchEmptyFiles.Checked || patternFileTypes.Text == "") && !searchEmptyFolders.Checked) {
                searchButton.Enabled = false;
            }
            #endregion
        }

        void Form_Closing(object sender, FormClosingEventArgs e) {
            FileDataReader.ProgramSettings.MainWindow.PrintOnlyFoundFiles = onlyFoundFiles.Checked;
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

            if (openFolder.ShowDialog() == DialogResult.OK) {
                if (!FileUtils.IsNotSpecialFolder(openFolder.SelectedPath)) {
                    MessageBox.Show("The selected path can't be system files!", "System file detected", MessageBoxButtons.OK);
                    return;
                }

                folderPath.Text = openFolder.SelectedPath;
            }
        }

        private async void searchButton_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(folderPath.Text)) {
                return;
            }

            if (!FileUtils.IsNotSpecialFolder(folderPath.Text)) {
                MessageBox.Show("The selected path can't be system files!", "System file detected", MessageBoxButtons.OK);
                return;
            }

            logs.Items.Clear();

            var directoryInfo = new DirectoryInfo(folderPath.Text);
            if (!directoryInfo.Exists) {
                MessageBox.Show(this, "File/directory not exists");
                return;
            }

            Form waitingWindow = null;
            if (directoryInfo.GetDirectories().Length > 5) {
                waitingWindow = new Form() {
                    Owner = this,
                    Text = "Collecting",
                    StartPosition = FormStartPosition.CenterScreen,
                    ShowIcon = false,
                    ShowInTaskbar = false,
                    Size = new Size(350, 150),
                    MinimizeBox = false,
                    MaximizeBox = false,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    ControlBox = false // Lets hide every buttons from window
                };

                Label processLabel = new Label() {
                    Text = "Please wait until the program collects all of directories..." + nl + nl +
                        "This process can took more than 30 sec depending how many files are on your drive.",
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft JhengHei UI", 9)
                };
                waitingWindow.Controls.Add(processLabel);
                waitingWindow.Show();
                processLabel.Invalidate();
                processLabel.Update();
                processLabel.Refresh();
            }

            #region Disabling and clearing some options
            paused = false;
            UseWaitCursor = true;
            searchButton.Enabled = false;
            browseButton.Enabled = false;
            removeAll.Enabled = false;
            stopTask.Enabled = true;
            onlyFoundFiles.Enabled = false;
            searchEmptyFolders.Enabled = false;
            searchEmptyFiles.Enabled = false;
            patternFileTypes.Enabled = false;
            settingsMenu.Enabled = false;
            clearButton.Enabled = false;
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

            Graphics g = logs.CreateGraphics();
            object obj = new object();
            int lastStringWidth = 0;

            if (searchEmptyFiles.Checked) {
                foreach (FileInfo fileInfo in FileUtils.GetFiles(directoryInfo, patternFileTypes.Text.Replace(',', '|').Replace(" ", ""), false)) {
                    if (paused) {
                        break;
                    }

                    if (waitingWindow != null && !waitingWindow.IsDisposed) {
                        waitingWindow.Dispose();
                        waitingWindow.Close();
                    }

                    if (ErrorViewer.errorLogs.TextLength > 0 && !showErrors.Enabled) {
                        showErrors.Enabled = FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging; // Enable show errors button
                    }

                    lock (obj) {
                        if (!FileUtils.IsFileNotHidden(fileInfo) || settings.IsDirectoryInIgnoredList(fileInfo.FullName)) {
                            continue;
                        }

                        if (!onlyFoundFiles.Checked) {
                            logs.Items.Add(fileInfo.FullName);
                        }

                        if (fileInfo.Exists && fileInfo.Length <= 0) {
                            emptyFilesList.Add(fileInfo);
                            removedAmount.Text = (searchEmptyFolders.Checked ? emptyDirectories.Count + " empty folders, " : "") + emptyFilesList.Count + " empty files";

                            if (onlyFoundFiles.Checked) {
                                logs.Items.Add(fileInfo.FullName);
                            }
                        }

                        // Change horizontal scroll bar if path is too long
                        int sWidth = (int) g.MeasureString(fileInfo.FullName, logs.Font).Width;
                        if (sWidth > logs.Width && lastStringWidth < sWidth) {
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
                foreach (DirectoryInfo dInfo in FileUtils.GetFiles(directoryInfo, "*", true)) {
                    if (paused) {
                        break;
                    }

                    if (waitingWindow != null && !waitingWindow.IsDisposed) {
                        waitingWindow.Dispose();
                        waitingWindow.Close();
                    }

                    if (ErrorViewer.errorLogs.TextLength > 0 && !showErrors.Enabled) {
                        showErrors.Enabled = FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging; // Enable show errors button
                    }

                    lock (obj) {
                        if (!FileUtils.IsFileNotHidden(dInfo) || settings.IsDirectoryInIgnoredList(dInfo.FullName)) {
                            continue;
                        }

                        if (!onlyFoundFiles.Checked) {
                            logs.Items.Add(dInfo.FullName);
                        }

                        try {
                            if (dInfo.Exists && !Directory.EnumerateFileSystemEntries(dInfo.FullName).Any() && dInfo.GetFiles().Length == 0 && dInfo.GetDirectories().Length == 0) {
                                emptyDirectories.Add(dInfo);
                                removedAmount.Text = emptyDirectories.Count + " empty folders" + (searchEmptyFiles.Checked ? ", " + emptyFilesList.Count + " empty files" : "");

                                if (onlyFoundFiles.Checked) {
                                    logs.Items.Add(dInfo.FullName);
                                }
                            }
                        } catch (Exception ex) {
                            LogException(ex.Message + " " + ex.StackTrace);
                        }

                        // Change horizontal scroll bar if path is too long
                        int sWidth = (int) g.MeasureString(dInfo.FullName, logs.Font).Width;
                        if (sWidth > logs.Width && lastStringWidth < sWidth) {
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

            if (waitingWindow != null && !waitingWindow.IsDisposed) {
                waitingWindow.Dispose();
                waitingWindow.Close();
            }

            if (!paused && emptyFilesList.Count == 0 && emptyDirectories.Count == 0) {
                DialogResult result = MessageBox.Show("There is no any empty " + (searchEmptyFiles.Checked ? "file " : "")
                    + (searchEmptyFolders.Checked ? "and folder " : "") + ".", "Empty directory", MessageBoxButtons.OK);
                if (result != DialogResult.OK) {
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

            logs.SelectionMode = SelectionMode.One;
            UseWaitCursor = false;
            searchButton.Enabled = true;
            settingsMenu.Enabled = true;
            browseButton.Enabled = true;
            onlyFoundFiles.Enabled = true;
            searchEmptyFolders.Enabled = true;
            searchEmptyFiles.Enabled = true;
            patternFileTypes.Enabled = true;
            stopTask.Enabled = false;
            #endregion
        }

        private void stopTask_Click(object sender, EventArgs e) {
            paused = true;
            stopTask.Enabled = false;
        }

        private void clearButton_Click(object sender, EventArgs e) {
            logs.Items.Clear();
            removedFilesList.Clear();

            removedFilesList.Visible = false;
            clearButton.Enabled = false;
            removeAll.Enabled = false;
            removedAmount.Text = "";
            logs.HorizontalExtent = 1; // Reset horizontal scroll bar to default value
        }

        private void removeAll_Click(object sender, EventArgs e) {
            DialogResult result = MessageBox.Show("Are you sure that you want to remove all files?" + nl + nl
                + "Selected action: " + (FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin ? "Move to recycle bin" : "Remove entirely"),
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) {
                return;
            }

            removedFilesList.Visible = true;

            emptyFilesList.ForEach(file => {
                if (file.Exists && file.Length <= 0) {
                    try {
                        if (FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin) {
                            file.MoveTo(@"C:\$Recycle.Bin");
                        } else {
                            file.Delete();
                        }
                    } catch (Exception ex) {
                        LogException(ex.Message + " " + ex.StackTrace);
                        return;
                    }

                    removedFilesList.AppendText("File removed: " + file.FullName);
                    removedFilesList.AppendText(nl);
                }
            });

            emptyDirectories.ForEach(dir => {
                if (dir.Exists && dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0) {
                    try {
                        if (FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin) {
                            dir.MoveTo(@"C:\$Recycle.Bin");
                        } else {
                            dir.Delete();
                        }
                    } catch (Exception ex) {
                        LogException(ex.Message + " " + ex.StackTrace);
                        return;
                    }

                    removedFilesList.AppendText("Directory removed: " + dir.FullName);
                    removedFilesList.AppendText(nl);
                }
            });

            emptyFilesList.Clear();
            emptyDirectories.Clear();

            removeAll.Enabled = false;
            removedAmount.Text = "";
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
            if (logs.SelectedItems.Count == 0 || e.Button != MouseButtons.Right) {
                return;
            }

            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Click += OnLogsFilesBoxClicked;

            cms.Items.Add("Remove");
            cms.Items.Add("Open in file explorer");

            if (sender is Control control) {
                cms.Show(this, new Point(e.X + control.Left, e.Y + control.Top));
            }

            logs.ContextMenuStrip = cms;
        }

        private void OnLogsFilesBoxClicked(object sender, EventArgs e) {
            if (e is MouseEventArgs mouse && mouse.Button == MouseButtons.Left && logs.SelectedItems.Count > 0) {
                ContextMenuStrip cms = sender as ContextMenuStrip;
                object selectedItem = logs.SelectedItem;

                var toolStripItem = cms?.GetItemAt(mouse.Location);
                if (toolStripItem != null) {
                    if (toolStripItem.Text == "Open in file explorer") {
                        try {
                            string si = selectedItem.ToString();
                            System.Diagnostics.Process.Start("explorer.exe", File.Exists(si) ? new FileInfo(si).DirectoryName : si);
                        } catch (Exception ex) {
                            LogException(ex.Message + " " + ex.StackTrace);
                            MessageBox.Show("File not found", "The file to open does not exist: " + ex.Message, MessageBoxButtons.OK);
                        }
                    } else if (toolStripItem.Text == "Remove") {
                        logs.Items.Remove(selectedItem);

                        string si = selectedItem.ToString();
                        if (Directory.Exists(si)) {
                            Directory.Delete(si, true);
                        } else if (File.Exists(si)) {
                            File.Delete(si);
                        }
                    }
                }

                if (cms != null) {
                    cms.Close();
                }

                logs.SelectedItems.Clear();
            }
        }

        public static bool LogException(string s) {
            if (!FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging || string.IsNullOrWhiteSpace(s)) {
                return false;
            }

            ErrorViewer.errorLogs.AppendText(s);
            ErrorViewer.errorLogs.AppendText(nl + nl);
            return true;
        }

        private async Task DoAsync(int ms) {
            await Task.Delay(ms);
        }
    }
}
