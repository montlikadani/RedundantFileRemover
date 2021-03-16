using RedundantFileRemover.UserSettingsData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RedundantFileRemover {
    public partial class SettingsForm : Form {

        public SettingsForm() {
            InitializeComponent();

            Load += new EventHandler(SettingsForm_Load);
            FormClosing += new FormClosingEventHandler(SettingsForm_FormClosing);
            Application.ApplicationExit += new EventHandler(SettingsForm_FormExit);
        }

        void SettingsForm_Load(object sender, EventArgs e) {
            #region Load saved data into memory
            searchInSubDirs.Checked = FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories;
            errorLogging.Checked = FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging;
            alwaysClearLogs.Checked = FileDataReader.ProgramSettings.SettingsWindow.AlwaysClearLogs;
            moveFilesToBin.Checked = FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin;

            filterList.Items.Clear();

            FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Where(dir => dir != "")
                .Where(d => !d.Contains(FileDataReader.ProgramSettings.MainWindow.FolderPath))
                .Distinct().ToList().ForEach(a => filterList.Items.Add(a));
            #endregion

            if (filterList.Items.Count > 0) {
                removeFilters.Enabled = true;
            }
        }

        void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
            SettingsForm_FormExit(sender, e);

            // ShowErrors button in main window
            if (Owner is RedundantFileRemover rfr) {
                if (!errorLogging.Checked) {
                    rfr.showErrors.Enabled = false;
                } else {
                    rfr.showErrors.Visible = rfr.showErrors.Enabled = true;
                }
            }
        }

        void SettingsForm_FormExit(object sender, EventArgs e) {
            FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging = errorLogging.Checked;
            FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories = searchInSubDirs.Checked;

            foreach (var item in filterList.Items) {
                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Add(item.ToString());
            }

            FileDataReader.ProgramSettings.SettingsWindow.AlwaysClearLogs = alwaysClearLogs.Checked;
            FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin = moveFilesToBin.Checked;
        }

        public bool IsDirectoryInIgnoredList(string dir) {
            foreach (string ign in FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories) {
                if (ign != "" && dir.Contains(ign)) {
                    return true;
                }
            }

            return false;
        }

        private void browseFiltersButton_Click(object sender, EventArgs e) {
            var dialog = new FolderBrowserDialog {
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK) {
                string selectedPath = dialog.SelectedPath;
                if (FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Contains(selectedPath)) {
                    return;
                }

                if (Owner is RedundantFileRemover main && selectedPath == main.folderPath.Text) {
                    error.Text = "Error: The path can't be the same with the selected folder!";
                    return;
                }

                error.Text = "";

                filterList.Items.Add(selectedPath);
                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Add(selectedPath);
                removeFilters.Enabled = true;
            }

            dialog.Dispose();
        }

        private void removeFilters_Click(object sender, EventArgs e) {
            if (filterList.SelectedItems.Count == 0) {
                filterList.Items.Clear();
                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Clear();
            } else {
                List<string> list = new();
                foreach (string r in filterList.SelectedItems) {
                    list.Add(r);
                }

                foreach (string item in list) {
                    filterList.Items.Remove(item);
                    FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Remove(item);
                }

                filterList.ClearSelected();
            }

            if (filterList.Items.Count == 0) {
                removeFilters.Enabled = false;
            }
        }

        private void filterList_MouseDown(object sender, MouseEventArgs e) {
            filterList.ContextMenuStrip = null;

            if (e.Button.HasFlag(MouseButtons.Left) && filterList.IndexFromPoint(e.Location) == -1) {
                filterList.ClearSelected();
            }

            if (filterList.SelectedItems.Count == 0 || !e.Button.HasFlag(MouseButtons.Right)) {
                return;
            }

            ContextMenuStrip cms = new();
            cms.Click += OnFilterListClicked;

            cms.Items.Add("Remove from list");
            cms.Items.Add("Open in file explorer");

            if (sender is Control control) {
                cms.Show(this, new Point(e.X + control.Left, e.Y + control.Top));
            }

            filterList.ContextMenuStrip = cms;
        }

        private void OnFilterListClicked(object sender, EventArgs e) {
            if (e is MouseEventArgs mouse && mouse.Button.HasFlag(MouseButtons.Left) && filterList.SelectedItems.Count > 0) {
                ContextMenuStrip cms = sender as ContextMenuStrip;
                object selectedItem = filterList.SelectedItem;

                var toolStripItem = cms?.GetItemAt(mouse.Location);
                if (toolStripItem is not null) {
                    if (toolStripItem.Text == "Open in file explorer") {
                        try {
                            System.Diagnostics.Process.Start("explorer.exe", selectedItem.ToString());
                        } catch (Exception ex) {
                            RedundantFileRemover.LogException(ex.Message + " " + ex.StackTrace);
                            MessageBox.Show("File not found", "The file to open does not exist: " + ex.Message, MessageBoxButtons.OK);
                        }
                    } else if (toolStripItem.Text == "Remove from list") {
                        FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Remove(selectedItem.ToString());
                        filterList.Items.Remove(selectedItem);
                    }
                }

                if (filterList.Items.Count == 0) {
                    removeFilters.Enabled = false;
                }

                if (cms is not null) {
                    cms.Close();
                }
            }

            filterList.ClearSelected();
        }

        private void errorLoggingEnabled_CheckedChanged(object sender, EventArgs e) {
            alwaysClearLogs.Visible = errorLogging.Checked;
        }
    }
}
