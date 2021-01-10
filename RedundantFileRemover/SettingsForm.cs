using RedundantFileRemover.UserSettingsData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RedundantFileRemover {
    public partial class SettingsForm : Form {

        public SettingsForm() {
            InitializeComponent();

            FormClosing += new FormClosingEventHandler(SettingsForm_FormClosing);
            Application.ApplicationExit += new EventHandler(SettingsForm_FormExit);

            #region Load saved data into memory
            searchInSubDirs.Checked = FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories;
            errorLogging.Checked = FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging;
            alwaysClearLogs.Checked = FileDataReader.ProgramSettings.SettingsWindow.AlwaysClearLogs;
            moveFilesToBin.Checked = FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin;

            FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.ForEach(a => filterList.Items.Add(a));
            #endregion

            if (filterList.Items.Count > 0) {
                removeFilters.Enabled = true;
            }
        }

        void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
            SettingsForm_FormExit(sender, e);

            // Hiding ShowErrors button in main window
            if (!errorLogging.Checked && Owner is RedundantFileRemover rfr) {
                rfr.showErrors.Enabled = false;
            }
        }

        void SettingsForm_FormExit(object sender, EventArgs e) {
            #region save cached data
            FileDataReader.ProgramSettings.SettingsWindow.ErrorLogging = errorLogging.Checked;
            FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories = searchInSubDirs.Checked;
            FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.AddRange(filterList.Text.Split('\n'));
            FileDataReader.ProgramSettings.SettingsWindow.AlwaysClearLogs = alwaysClearLogs.Checked;
            FileDataReader.ProgramSettings.SettingsWindow.MoveFileToRecycleBin = moveFilesToBin.Checked;
            #endregion
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
            var dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK) {
                string selectedPath = dialog.SelectedPath;
                if (FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Contains(selectedPath)) {
                    return;
                }

                if (Owner is RedundantFileRemover main && selectedPath == main.folderPath.Text) {
                    error.Text = "Error: The path can't be the same with the selected folder.";
                    return;
                }

                error.Text = "";

                filterList.Items.Add(selectedPath);
                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Add(selectedPath);
                removeFilters.Enabled = true;
            }
        }

        private void removeFilters_Click(object sender, EventArgs e) {
            if (filterList.SelectedItems.Count == 0) {
                filterList.Items.Clear();
                FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Clear();
            } else {
                List<string> list = new List<string>();
                foreach (string r in filterList.SelectedItems) {
                    list.Add(r);
                }

                foreach (string item in list) {
                    filterList.Items.Remove(item);
                    FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Remove(item);
                }

                filterList.SelectedItems.Clear();
            }

            if (filterList.Items.Count == 0) {
                removeFilters.Enabled = false;
            }
        }

        private void filterList_MouseDown(object sender, MouseEventArgs e) {
            if (filterList.SelectedItems.Count == 0 || e.Button != MouseButtons.Right) {
                return;
            }

            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Click += OnFilterListClicked;

            cms.Items.Add("Remove from list");
            cms.Items.Add("Open in file explorer");

            if (sender is Control control) {
                cms.Show(this, new Point(e.X + control.Left, e.Y + control.Top));
            }

            filterList.ContextMenuStrip = cms;
        }

        private void OnFilterListClicked(object sender, EventArgs e) {
            if (e is MouseEventArgs mouse && mouse.Button == MouseButtons.Left && filterList.SelectedItems.Count > 0) {
                ContextMenuStrip cms = sender is ContextMenuStrip ? sender as ContextMenuStrip : null;
                object selectedItem = filterList.SelectedItem;

                filterList.Items.Remove(selectedItem);

                var toolStripItem = cms.GetItemAt(mouse.Location);
                if (toolStripItem != null) {
                    if (toolStripItem.Text == "Open in file explorer") {
                        try {
                            System.Diagnostics.Process.Start("explorer.exe", selectedItem.ToString());
                        } catch (Exception ex) {
                            RedundantFileRemover.LogException(ex.Message + " " + ex.StackTrace);
                            MessageBox.Show("File not found", "The file to open does not exist: " + ex.Message, MessageBoxButtons.OK);
                        }
                    } else if (toolStripItem.Text == "Remove from list") {
                        FileDataReader.ProgramSettings.SettingsWindow.IgnoredDirectories.Remove(selectedItem.ToString());
                    }
                }

                if (filterList.Items.Count == 0) {
                    removeFilters.Enabled = false;
                }

                if (cms != null) {
                    cms.Close();
                }

                filterList.SelectedItems.Clear();
            }
        }

        private void errorLoggingEnabled_CheckedChanged(object sender, EventArgs e) {
            alwaysClearLogs.Visible = errorLogging.Checked;
        }
    }
}
