
namespace RedundantFileRemover {
    partial class RedundantFileRemover {
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
            System.Windows.Forms.Label pathToSearch;
            this.browseButton = new System.Windows.Forms.Button();
            this.folderPath = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.logs = new System.Windows.Forms.TextBox();
            this.stopTask = new System.Windows.Forms.Button();
            this.clearButton = new System.Windows.Forms.Button();
            this.removeAll = new System.Windows.Forms.Button();
            this.onlyFoundFiles = new System.Windows.Forms.CheckBox();
            this.searchEmptyFolders = new System.Windows.Forms.CheckBox();
            this.fileFilters = new System.Windows.Forms.Label();
            this.browseFiltersButton = new System.Windows.Forms.Button();
            this.error = new System.Windows.Forms.Label();
            this.removedAmount = new System.Windows.Forms.Label();
            this.searchEmptyFiles = new System.Windows.Forms.CheckBox();
            this.patternFileTypes = new System.Windows.Forms.TextBox();
            this.filterList = new System.Windows.Forms.ListBox();
            this.removeFilters = new System.Windows.Forms.Button();
            pathToSearch = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pathToSearch
            // 
            pathToSearch.AutoSize = true;
            pathToSearch.Location = new System.Drawing.Point(23, 18);
            pathToSearch.Name = "pathToSearch";
            pathToSearch.Size = new System.Drawing.Size(86, 15);
            pathToSearch.TabIndex = 3;
            pathToSearch.Text = "Path to search";
            // 
            // browseButton
            // 
            this.browseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.browseButton.Location = new System.Drawing.Point(919, 40);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(101, 31);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Add";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // folderPath
            // 
            this.folderPath.Location = new System.Drawing.Point(26, 40);
            this.folderPath.Name = "folderPath";
            this.folderPath.Size = new System.Drawing.Size(884, 23);
            this.folderPath.TabIndex = 1;
            this.folderPath.Text = "C:\\";
            // 
            // searchButton
            // 
            this.searchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.searchButton.Location = new System.Drawing.Point(918, 107);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(101, 31);
            this.searchButton.TabIndex = 4;
            this.searchButton.Text = "Search";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(29, 159);
            this.logs.Multiline = true;
            this.logs.Name = "logs";
            this.logs.ReadOnly = true;
            this.logs.Size = new System.Drawing.Size(884, 331);
            this.logs.TabIndex = 5;
            // 
            // stopTask
            // 
            this.stopTask.Enabled = false;
            this.stopTask.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.stopTask.Location = new System.Drawing.Point(919, 146);
            this.stopTask.Name = "stopTask";
            this.stopTask.Size = new System.Drawing.Size(101, 31);
            this.stopTask.TabIndex = 9;
            this.stopTask.Text = "Cancel";
            this.stopTask.UseVisualStyleBackColor = true;
            this.stopTask.Click += new System.EventHandler(this.stopTask_Click);
            // 
            // clearButton
            // 
            this.clearButton.Enabled = false;
            this.clearButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.clearButton.Location = new System.Drawing.Point(813, 496);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(101, 31);
            this.clearButton.TabIndex = 10;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // removeAll
            // 
            this.removeAll.Enabled = false;
            this.removeAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.removeAll.Location = new System.Drawing.Point(919, 183);
            this.removeAll.Name = "removeAll";
            this.removeAll.Size = new System.Drawing.Size(103, 33);
            this.removeAll.TabIndex = 11;
            this.removeAll.Text = "Remove all";
            this.removeAll.UseVisualStyleBackColor = true;
            this.removeAll.Click += new System.EventHandler(this.removeAll_Click);
            // 
            // onlyFoundFiles
            // 
            this.onlyFoundFiles.AutoSize = true;
            this.onlyFoundFiles.Location = new System.Drawing.Point(26, 69);
            this.onlyFoundFiles.Name = "onlyFoundFiles";
            this.onlyFoundFiles.Size = new System.Drawing.Size(140, 19);
            this.onlyFoundFiles.TabIndex = 12;
            this.onlyFoundFiles.Text = "Print only found files";
            this.onlyFoundFiles.UseVisualStyleBackColor = true;
            // 
            // searchEmptyFolders
            // 
            this.searchEmptyFolders.AutoSize = true;
            this.searchEmptyFolders.Checked = true;
            this.searchEmptyFolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.searchEmptyFolders.Location = new System.Drawing.Point(336, 69);
            this.searchEmptyFolders.Name = "searchEmptyFolders";
            this.searchEmptyFolders.Size = new System.Drawing.Size(145, 19);
            this.searchEmptyFolders.TabIndex = 13;
            this.searchEmptyFolders.Text = "Search empty folders";
            this.searchEmptyFolders.UseVisualStyleBackColor = true;
            this.searchEmptyFolders.CheckedChanged += new System.EventHandler(this.searchFiles_CheckedChanged);
            // 
            // fileFilters
            // 
            this.fileFilters.AutoSize = true;
            this.fileFilters.Location = new System.Drawing.Point(26, 522);
            this.fileFilters.Name = "fileFilters";
            this.fileFilters.Size = new System.Drawing.Size(148, 15);
            this.fileFilters.TabIndex = 14;
            this.fileFilters.Text = "Add directory exceptions";
            // 
            // browseFiltersButton
            // 
            this.browseFiltersButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.browseFiltersButton.AutoSize = true;
            this.browseFiltersButton.Location = new System.Drawing.Point(180, 511);
            this.browseFiltersButton.Name = "browseFiltersButton";
            this.browseFiltersButton.Size = new System.Drawing.Size(35, 26);
            this.browseFiltersButton.TabIndex = 15;
            this.browseFiltersButton.Text = "...";
            this.browseFiltersButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.browseFiltersButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.browseFiltersButton.UseVisualStyleBackColor = true;
            this.browseFiltersButton.Click += new System.EventHandler(this.browseFiltersButton_Click);
            // 
            // error
            // 
            this.error.AutoSize = true;
            this.error.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.error.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.error.ForeColor = System.Drawing.SystemColors.Control;
            this.error.Location = new System.Drawing.Point(202, 519);
            this.error.Name = "error";
            this.error.Size = new System.Drawing.Size(0, 16);
            this.error.TabIndex = 17;
            this.error.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // removedAmount
            // 
            this.removedAmount.AutoSize = true;
            this.removedAmount.Location = new System.Drawing.Point(30, 140);
            this.removedAmount.Name = "removedAmount";
            this.removedAmount.Size = new System.Drawing.Size(0, 15);
            this.removedAmount.TabIndex = 18;
            // 
            // searchEmptyFiles
            // 
            this.searchEmptyFiles.AutoSize = true;
            this.searchEmptyFiles.Checked = true;
            this.searchEmptyFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.searchEmptyFiles.Location = new System.Drawing.Point(336, 94);
            this.searchEmptyFiles.Name = "searchEmptyFiles";
            this.searchEmptyFiles.Size = new System.Drawing.Size(128, 19);
            this.searchEmptyFiles.TabIndex = 21;
            this.searchEmptyFiles.Text = "Search empty files";
            this.searchEmptyFiles.UseVisualStyleBackColor = true;
            this.searchEmptyFiles.CheckedChanged += new System.EventHandler(this.searchFiles_CheckedChanged);
            // 
            // patternFileTypes
            // 
            this.patternFileTypes.Location = new System.Drawing.Point(478, 94);
            this.patternFileTypes.Name = "patternFileTypes";
            this.patternFileTypes.Size = new System.Drawing.Size(100, 23);
            this.patternFileTypes.TabIndex = 22;
            this.patternFileTypes.Text = ".ini, .log, .txt";
            // 
            // filterList
            // 
            this.filterList.FormattingEnabled = true;
            this.filterList.HorizontalScrollbar = true;
            this.filterList.ItemHeight = 15;
            this.filterList.Location = new System.Drawing.Point(29, 541);
            this.filterList.Name = "filterList";
            this.filterList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.filterList.Size = new System.Drawing.Size(422, 94);
            this.filterList.TabIndex = 23;
            this.filterList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.filterList_MouseDown);
            // 
            // removeFilters
            // 
            this.removeFilters.Enabled = false;
            this.removeFilters.Location = new System.Drawing.Point(29, 642);
            this.removeFilters.Name = "removeFilters";
            this.removeFilters.Size = new System.Drawing.Size(75, 23);
            this.removeFilters.TabIndex = 24;
            this.removeFilters.Text = "Remove";
            this.removeFilters.UseVisualStyleBackColor = true;
            this.removeFilters.Click += new System.EventHandler(this.removeFilters_Click);
            // 
            // RedundantFileRemover
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 704);
            this.Controls.Add(this.removeFilters);
            this.Controls.Add(this.filterList);
            this.Controls.Add(this.patternFileTypes);
            this.Controls.Add(this.searchEmptyFiles);
            this.Controls.Add(this.removedAmount);
            this.Controls.Add(this.error);
            this.Controls.Add(this.browseFiltersButton);
            this.Controls.Add(this.fileFilters);
            this.Controls.Add(this.searchEmptyFolders);
            this.Controls.Add(this.onlyFoundFiles);
            this.Controls.Add(this.removeAll);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.stopTask);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(pathToSearch);
            this.Controls.Add(this.folderPath);
            this.Controls.Add(this.browseButton);
            this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "RedundantFileRemover";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Redundant File Remover";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox folderPath;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.TextBox logs;
        private System.Windows.Forms.Button stopTask;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button removeAll;
        private System.Windows.Forms.CheckBox onlyFoundFiles;
        private System.Windows.Forms.CheckBox searchEmptyFolders;
        private System.Windows.Forms.Label fileFilters;
        private System.Windows.Forms.Button browseFiltersButton;
        private System.Windows.Forms.Label error;
        private System.Windows.Forms.Label removedAmount;
        private System.Windows.Forms.CheckBox searchEmptyFiles;
        private System.Windows.Forms.TextBox patternFileTypes;
        private System.Windows.Forms.ListBox filterList;
        private System.Windows.Forms.Button removeFilters;
    }
}

