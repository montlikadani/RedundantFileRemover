using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RedundantFileRemover {
    public partial class SettingsForm : Form {

        public SettingsForm() {
            InitializeComponent();

            Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
            FormClosing += new FormClosingEventHandler(SettingsForm_FormClosing);
            Application.ApplicationExit += new EventHandler(SettingsForm_FormExit);

            searchInSubDirs.Checked = CachedData.SearchInSubDirectories;
            errorLogging.Checked = CachedData.ErrorLogging;
            alwaysClearLogs.Checked = CachedData.AlwaysClearLogs;

            CachedData.IgnoredDirectories.ForEach(a => filterList.Items.Add(a));

            if (filterList.Items.Count > 0) {
                removeFilters.Enabled = true;
            }
        }

        void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            Properties.Settings.Default.Save();
        }

        void SettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
            CachedData.SearchInSubDirectories = searchInSubDirs.Checked;
            CachedData.ErrorLogging = errorLogging.Checked;
            CachedData.AlwaysClearLogs = alwaysClearLogs.Checked;

            // Hiding ShowErrors button in main window
            if (!errorLogging.Checked && Owner is RedundantFileRemover rfr) {
                rfr.showErrors.Enabled = false;
            }
        }

        void SettingsForm_FormExit(object sender, EventArgs e) {
            Properties.Settings.Default.ErrorLogging = CachedData.ErrorLogging;
            Properties.Settings.Default.SearchInSubDirs = CachedData.SearchInSubDirectories;
            Properties.Settings.Default.IgnoredDirectories = CachedData.IgnoredDirectories;
            Properties.Settings.Default.AlwaysClearLogs = CachedData.AlwaysClearLogs;
        }

        public bool IsDirectoryInIgnoredList(string dir) {
            foreach (string ign in CachedData.IgnoredDirectories) {
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
                if (CachedData.IgnoredDirectories.Contains(selectedPath)) {
                    return;
                }

                if (Owner is RedundantFileRemover main && selectedPath == main.folderPath.Text) {
                    error.Text = "Error: The path can't be the same with the selected folder.";
                    return;
                }

                error.Text = "";

                filterList.Items.Add(selectedPath);
                CachedData.IgnoredDirectories.Add(selectedPath);
                removeFilters.Enabled = true;
            }
        }

        private void removeFilters_Click(object sender, EventArgs e) {
            if (filterList.SelectedItems.Count == 0) {
                filterList.Items.Clear();
                CachedData.IgnoredDirectories.Clear();
            } else {
                List<string> list = new List<string>();
                foreach (string r in filterList.SelectedItems) {
                    list.Add(r);
                }

                foreach (string item in list) {
                    filterList.Items.Remove(item);
                    CachedData.IgnoredDirectories.Remove(item);
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
                        CachedData.IgnoredDirectories.Remove(selectedItem.ToString());
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
