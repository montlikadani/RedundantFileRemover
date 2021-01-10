
namespace RedundantFileRemover {
    partial class SettingsForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.removeFilters = new System.Windows.Forms.Button();
            this.filterList = new System.Windows.Forms.ListBox();
            this.error = new System.Windows.Forms.Label();
            this.browseFiltersButton = new System.Windows.Forms.Button();
            this.fileFilters = new System.Windows.Forms.Label();
            this.searchInSubDirs = new System.Windows.Forms.CheckBox();
            this.errorLoggingLabel = new System.Windows.Forms.Label();
            this.errorLogging = new System.Windows.Forms.CheckBox();
            this.alwaysClearLogs = new System.Windows.Forms.CheckBox();
            this.moveFilesToBin = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // removeFilters
            // 
            this.removeFilters.Enabled = false;
            this.removeFilters.Location = new System.Drawing.Point(14, 338);
            this.removeFilters.Name = "removeFilters";
            this.removeFilters.Size = new System.Drawing.Size(87, 29);
            this.removeFilters.TabIndex = 29;
            this.removeFilters.Text = "Remove";
            this.removeFilters.UseVisualStyleBackColor = true;
            this.removeFilters.Click += new System.EventHandler(this.removeFilters_Click);
            // 
            // filterList
            // 
            this.filterList.FormattingEnabled = true;
            this.filterList.HorizontalScrollbar = true;
            this.filterList.ItemHeight = 16;
            this.filterList.Location = new System.Drawing.Point(14, 232);
            this.filterList.Name = "filterList";
            this.filterList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.filterList.Size = new System.Drawing.Size(492, 100);
            this.filterList.TabIndex = 28;
            this.filterList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.filterList_MouseDown);
            // 
            // error
            // 
            this.error.AutoSize = true;
            this.error.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.error.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.error.ForeColor = System.Drawing.SystemColors.Control;
            this.error.Location = new System.Drawing.Point(214, 212);
            this.error.Name = "error";
            this.error.Size = new System.Drawing.Size(0, 16);
            this.error.TabIndex = 27;
            this.error.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // browseFiltersButton
            // 
            this.browseFiltersButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.browseFiltersButton.AutoSize = true;
            this.browseFiltersButton.Location = new System.Drawing.Point(167, 205);
            this.browseFiltersButton.Name = "browseFiltersButton";
            this.browseFiltersButton.Size = new System.Drawing.Size(41, 26);
            this.browseFiltersButton.TabIndex = 26;
            this.browseFiltersButton.Text = "...";
            this.browseFiltersButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.browseFiltersButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.browseFiltersButton.UseVisualStyleBackColor = true;
            this.browseFiltersButton.Click += new System.EventHandler(this.browseFiltersButton_Click);
            // 
            // fileFilters
            // 
            this.fileFilters.AutoSize = true;
            this.fileFilters.Location = new System.Drawing.Point(12, 213);
            this.fileFilters.Name = "fileFilters";
            this.fileFilters.Size = new System.Drawing.Size(149, 16);
            this.fileFilters.TabIndex = 25;
            this.fileFilters.Text = "Add directory exceptions";
            // 
            // searchInSubDirs
            // 
            this.searchInSubDirs.AutoSize = true;
            this.searchInSubDirs.Checked = true;
            this.searchInSubDirs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.searchInSubDirs.Location = new System.Drawing.Point(14, 15);
            this.searchInSubDirs.Name = "searchInSubDirs";
            this.searchInSubDirs.Size = new System.Drawing.Size(165, 20);
            this.searchInSubDirs.TabIndex = 30;
            this.searchInSubDirs.Text = "Search in sub-directories";
            this.searchInSubDirs.UseVisualStyleBackColor = true;
            // 
            // errorLoggingLabel
            // 
            this.errorLoggingLabel.AutoSize = true;
            this.errorLoggingLabel.Location = new System.Drawing.Point(11, 38);
            this.errorLoggingLabel.Name = "errorLoggingLabel";
            this.errorLoggingLabel.Size = new System.Drawing.Size(83, 16);
            this.errorLoggingLabel.TabIndex = 31;
            this.errorLoggingLabel.Text = "Error logging";
            // 
            // errorLogging
            // 
            this.errorLogging.AutoSize = true;
            this.errorLogging.Checked = true;
            this.errorLogging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.errorLogging.Location = new System.Drawing.Point(25, 58);
            this.errorLogging.Name = "errorLogging";
            this.errorLogging.Size = new System.Drawing.Size(74, 20);
            this.errorLogging.TabIndex = 32;
            this.errorLogging.Text = "Enabled";
            this.errorLogging.UseVisualStyleBackColor = true;
            this.errorLogging.CheckedChanged += new System.EventHandler(this.errorLoggingEnabled_CheckedChanged);
            // 
            // alwaysClearLogs
            // 
            this.alwaysClearLogs.AutoSize = true;
            this.alwaysClearLogs.Location = new System.Drawing.Point(25, 85);
            this.alwaysClearLogs.Name = "alwaysClearLogs";
            this.alwaysClearLogs.Size = new System.Drawing.Size(261, 20);
            this.alwaysClearLogs.TabIndex = 33;
            this.alwaysClearLogs.Text = "Always clear logs before search operation";
            this.alwaysClearLogs.UseVisualStyleBackColor = true;
            // 
            // moveFilesToBin
            // 
            this.moveFilesToBin.AutoSize = true;
            this.moveFilesToBin.Location = new System.Drawing.Point(14, 112);
            this.moveFilesToBin.Name = "moveFilesToBin";
            this.moveFilesToBin.Size = new System.Drawing.Size(272, 20);
            this.moveFilesToBin.TabIndex = 34;
            this.moveFilesToBin.Text = "Move files to recycle bin instead of deleting";
            this.moveFilesToBin.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 382);
            this.Controls.Add(this.moveFilesToBin);
            this.Controls.Add(this.alwaysClearLogs);
            this.Controls.Add(this.errorLogging);
            this.Controls.Add(this.errorLoggingLabel);
            this.Controls.Add(this.searchInSubDirs);
            this.Controls.Add(this.removeFilters);
            this.Controls.Add(this.filterList);
            this.Controls.Add(this.error);
            this.Controls.Add(this.browseFiltersButton);
            this.Controls.Add(this.fileFilters);
            this.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SettingsForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button removeFilters;
        private System.Windows.Forms.ListBox filterList;
        private System.Windows.Forms.Label error;
        private System.Windows.Forms.Button browseFiltersButton;
        private System.Windows.Forms.Label fileFilters;
        private System.Windows.Forms.CheckBox searchInSubDirs;
        private System.Windows.Forms.Label errorLoggingLabel;
        public System.Windows.Forms.CheckBox alwaysClearLogs;
        private System.Windows.Forms.CheckBox errorLogging;
        public System.Windows.Forms.CheckBox moveFilesToBin;
    }
}