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
        }

        void Form_Exit(object sender, EventArgs e) {
            Program.ConfigFile.SaveAllSettings();
        }

        private void browseButton_Click(object sender, EventArgs e) {
            var openFolder = new FolderBrowserDialog();
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

            logs.Clear();

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
            searchButton.Enabled = false;
            UseWaitCursor = true;
            browseButton.Enabled = false;
            removeAll.Enabled = false;
            stopTask.Enabled = true;
            logs.ScrollBars = ScrollBars.Both;
            onlyFoundFiles.Enabled = false;
            searchEmptyFolders.Enabled = false;
            searchEmptyFiles.Enabled = false;
            patternFileTypes.Enabled = false;
            settingsMenu.Enabled = false;
            clearButton.Enabled = false;
            if (settings.alwaysClearLogs.Checked) {
                ErrorViewer.errorLogs.Clear();
            }
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

            object obj = new object();

            int emptyFiles = 0, emptyFolders = 0;
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
                            logs.AppendText(fileInfo.FullName);
                            logs.AppendText(nl);
                        }

                        if (fileInfo.Exists && fileInfo.Length <= 0) {
                            emptyFilesList.Add(fileInfo);
                            removedAmount.Text = (searchEmptyFolders.Checked ? emptyFolders + " empty folders, " : "") + ++emptyFiles + " empty files";

                            if (onlyFoundFiles.Checked) {
                                logs.AppendText(fileInfo.FullName);
                                logs.AppendText(nl);
                            }
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
                            logs.AppendText(dInfo.FullName);
                            logs.AppendText(nl);
                        }

                        try {
                            if (dInfo.Exists && !Directory.EnumerateFileSystemEntries(dInfo.FullName).Any() && dInfo.GetFiles().Length == 0 && dInfo.GetDirectories().Length == 0) {
                                removedAmount.Text = ++emptyFolders + " empty folders" + (searchEmptyFiles.Checked ? ", " + emptyFiles + " empty files" : "");
                                emptyDirectories.Add(dInfo);

                                if (onlyFoundFiles.Checked) {
                                    logs.AppendText(dInfo.FullName);
                                    logs.AppendText(nl);
                                }
                            }
                        } catch (Exception ex) {
                            LogException(ex.Message + " " + ex.StackTrace);
                        }
                    }

                    await DoAsync(5);
                }
            }

            if (waitingWindow != null && !waitingWindow.IsDisposed) {
                waitingWindow.Dispose();
                waitingWindow.Close();
            }

            if (emptyFiles == 0 && emptyFolders == 0) {
                DialogResult result = MessageBox.Show("There is no any empty " + (searchEmptyFiles.Checked ? "file " : "")
                    + (searchEmptyFolders.Checked ? "and folder " : "") + ".", "Empty directory", MessageBoxButtons.OK);
                if (result != DialogResult.OK) {
                    return;
                }
            }

            #region Re-enabling options
            if (emptyFolders > 0 || emptyFiles > 0) {
                removeAll.Enabled = true;
            }

            if (logs.TextLength > 0) {
                clearButton.Enabled = true;
            }

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
            logs.Clear();

            clearButton.Enabled = false;
            removeAll.Enabled = false;
            removedAmount.Text = "";
        }

        private void removeAll_Click(object sender, EventArgs e) {
            DialogResult result = MessageBox.Show("Are you sure that you want to remove all files?" + nl + nl
                + "Selected action: " + (FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin ? "Move to recycle bin" : "Remove entirely"),
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) {
                return;
            }

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

                    logs.AppendText(nl);
                    logs.AppendText("File removed: " + file.FullName);
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

                    logs.AppendText(nl);
                    logs.AppendText("Directory removed: " + dir.FullName);
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
