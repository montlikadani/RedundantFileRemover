using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Sys­tem.Threading.Tasks;
using System.Collections.Generic;

namespace RedundantFileRemover {
    public partial class RedundantFileRemover : Form {

        private bool paused = false;

        private readonly List<DirectoryInfo> emptyDirectories = new List<DirectoryInfo>();
        private readonly List<FileInfo> emptyFilesList = new List<FileInfo>();
        private readonly List<string> ignoredDirectories = new List<string>();

        public RedundantFileRemover() {
            InitializeComponent();
            folderPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void browseButton_Click(object sender, EventArgs e) {
            var openFolder = new FolderBrowserDialog();
            if (openFolder.ShowDialog() == DialogResult.OK) {
                folderPath.Text = openFolder.SelectedPath;
            }
        }

        private async void searchButton_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(folderPath.Text)) {
                return;
            }

            logs.Clear();

            var directoryInfo = new DirectoryInfo(folderPath.Text);
            if (!directoryInfo.Exists) {
                MessageBox.Show(this, "File/directory not exists");
                return;
            }

            paused = false;
            searchButton.Enabled = false;
            browseFiltersButton.Enabled = false;
            browseButton.Enabled = false;
            removeAll.Enabled = false;
            stopTask.Enabled = true;
            logs.ScrollBars = ScrollBars.Both;
            onlyFoundFiles.Enabled = false;
            searchEmptyFolders.Enabled = false;
            searchEmptyFiles.Enabled = false;
            patternFileTypes.Enabled = false;
            clearButton.Enabled = false;
            emptyFilesList.Clear();
            emptyDirectories.Clear();
            removedAmount.Text = "";
            if (searchEmptyFolders.Checked) {
                removedAmount.Text = "0 empty folders";
            }
            if (searchEmptyFiles.Checked) {
                removedAmount.Text += " 0 empty files";
            }

            int emptyFiles = 0;
            int emptyFolders = 0;
            if (searchEmptyFiles.Checked) {
                foreach (FileInfo fileInfo in FileUtils.GetFiles(directoryInfo, patternFileTypes.Text.Replace(',', '|').Replace(" ", ""), false)) {
                    if (paused) {
                        break;
                    }

                    if (!FileUtils.IsFileHasDefaultAttribute(fileInfo) || IsDirectoryInIgnoredList(fileInfo.FullName)) {
                        continue;
                    }

                    if (!onlyFoundFiles.Checked) {
                        logs.AppendText(fileInfo.FullName);
                        logs.AppendText(Environment.NewLine);
                    }

                    if (fileInfo.Exists && fileInfo.Length <= 0) {
                        emptyFilesList.Add(fileInfo);
                        removedAmount.Text = (searchEmptyFolders.Checked ? emptyFolders + " empty folders, " : "") + ++emptyFiles + " empty files";

                        if (onlyFoundFiles.Checked) {
                            logs.AppendText(fileInfo.FullName);
                            logs.AppendText(Environment.NewLine);
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

                    if (!FileUtils.IsFileHasDefaultAttribute(dInfo) || IsDirectoryInIgnoredList(dInfo.FullName)) {
                        continue;
                    }

                    if (!onlyFoundFiles.Checked) {
                        logs.AppendText(dInfo.FullName);
                        logs.AppendText(Environment.NewLine);
                    }

                    try {
                        if (dInfo.Exists && !Directory.EnumerateFileSystemEntries(dInfo.FullName).Any() && dInfo.GetFiles().Length == 0 && dInfo.GetDirectories().Length == 0) {
                            removedAmount.Text = ++emptyFolders + " empty folders" + (searchEmptyFiles.Checked ? ", " + emptyFiles + " empty files" : "");
                            emptyDirectories.Add(dInfo);

                            if (onlyFoundFiles.Checked) {
                                logs.AppendText(dInfo.FullName);
                                logs.AppendText(Environment.NewLine);
                            }
                        }
                    } catch (Exception) {
                    }

                    await DoAsync(5);
                }
            }

            await Task.WhenAll();

            if (emptyFiles == 0 && emptyFolders == 0) {
                DialogResult result = MessageBox.Show("There is no any empty " + (searchEmptyFiles.Checked ? "file " : "")
                    + (searchEmptyFolders.Checked ? "and folder " : "") + ".", "Empty directory", MessageBoxButtons.OK);
                if (result != DialogResult.OK) {
                    return;
                }
            }

            if (emptyFolders > 0 || emptyFiles > 0) {
                removeAll.Enabled = true;
            }

            if (logs.TextLength > 0) {
                clearButton.Enabled = true;
            }

            searchButton.Enabled = true;
            browseFiltersButton.Enabled = true;
            browseButton.Enabled = true;
            onlyFoundFiles.Enabled = true;
            searchEmptyFolders.Enabled = true;
            searchEmptyFiles.Enabled = true;
            patternFileTypes.Enabled = true;
            stopTask.Enabled = false;
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
            DialogResult result = MessageBox.Show("Are you sure that you want to remove all files?",
                "Confirmation", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes) {
                return;
            }

            emptyFilesList.ForEach(file => {
                if (file.Exists && file.Length <= 0) {
                    file.Delete();

                    logs.AppendText(Environment.NewLine);
                    logs.AppendText("File removed: " + file.FullName);
                }
            });

            emptyDirectories.ForEach(dir => {
                if (dir.Exists && dir.GetFiles().Length == 0 && dir.GetDirectories().Length == 0) {
                    dir.Delete();

                    logs.AppendText(Environment.NewLine);
                    logs.AppendText("Directory removed: " + dir.FullName);
                }
            });

            emptyFilesList.Clear();
            emptyDirectories.Clear();

            removeAll.Enabled = false;
            removedAmount.Text = "";
        }

        private void browseFiltersButton_Click(object sender, EventArgs e) {
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK) {
                if (dialog.SelectedPath == folderPath.Text) {
                    error.Text = "Error: The path can't be the same with the selected folder.";
                    return;
                }

                error.Text = "";

                filterList.Items.Add(dialog.SelectedPath);
                ignoredDirectories.Add(dialog.SelectedPath);
                removeFilters.Enabled = true;
            }
        }

        private async Task DoAsync(int ms) {
            await Task.Delay(ms);
        }

        private void searchFiles_CheckedChanged(object sender, EventArgs e) {
            patternFileTypes.Visible = searchEmptyFiles.Checked;
            searchButton.Enabled = searchEmptyFolders.Checked || searchEmptyFiles.Checked;
        }

        private void filterList_MouseDown(object sender, MouseEventArgs e) {
            if (filterList.SelectedItems.Count == 0 || e.Button != MouseButtons.Right) {
                return;
            }

            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Click += OnFilterListClicked;

            cms.Items.Add("Remove from list");

            if (sender is Control control) {
                cms.Show(this, new System.Drawing.Point(e.X + control.Left, e.Y + control.Top));
            }

            filterList.ContextMenuStrip = cms;
        }

        private void OnFilterListClicked(object sender, EventArgs e) {
            if (e is MouseEventArgs mouse && mouse.Button == MouseButtons.Left && filterList.SelectedItems.Count > 0) {
                object selectedItem = filterList.SelectedItem;
                filterList.Items.Remove(selectedItem);
                ignoredDirectories.Remove(selectedItem.ToString());

                if (filterList.Items.Count == 0) {
                    removeFilters.Enabled = false;
                }

                if (sender is ContextMenuStrip contextMenuStrip) {
                    contextMenuStrip.Dispose();
                }

                filterList.SelectedItems.Clear();
            }
        }

        private void removeFilters_Click(object sender, EventArgs e) {
            if (filterList.SelectedItems.Count == 0) {
                filterList.Items.Clear();
                ignoredDirectories.Clear();
            } else {
                List<string> list = new List<string>();
                foreach (string r in filterList.SelectedItems) {
                    list.Add(r);
                }

                foreach (string item in list) {
                    filterList.Items.Remove(item);
                    ignoredDirectories.Remove(item);
                }

                filterList.SelectedItems.Clear();
            }

            if (filterList.Items.Count == 0) {
                removeFilters.Enabled = false;
            }
        }

        public bool IsDirectoryInIgnoredList(string dir) {
            foreach (string ign in ignoredDirectories) {
                if (ign != "" && dir.Contains(ign)) {
                    return true;
                }
            }

            return false;
        }
    }
}
