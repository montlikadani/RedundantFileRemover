
namespace RedundantFileRemover {
    partial class ErrorViewer {
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
            this.errorLogs = new System.Windows.Forms.TextBox();
            this.clearErrorLog = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // errorLogs
            // 
            this.errorLogs.Location = new System.Drawing.Point(12, 12);
            this.errorLogs.Multiline = true;
            this.errorLogs.Name = "errorLogs";
            this.errorLogs.ReadOnly = true;
            this.errorLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.errorLogs.Size = new System.Drawing.Size(737, 438);
            this.errorLogs.TabIndex = 0;
            this.errorLogs.TextChanged += new System.EventHandler(this.errorLogs_TextChanged);
            // 
            // clearErrorLog
            // 
            this.clearErrorLog.Location = new System.Drawing.Point(13, 457);
            this.clearErrorLog.Name = "clearErrorLog";
            this.clearErrorLog.Size = new System.Drawing.Size(75, 23);
            this.clearErrorLog.TabIndex = 1;
            this.clearErrorLog.Text = "Clear";
            this.clearErrorLog.UseVisualStyleBackColor = true;
            this.clearErrorLog.Click += new System.EventHandler(this.clearErrorLog_Click);
            // 
            // ErrorViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(761, 493);
            this.Controls.Add(this.clearErrorLog);
            this.Controls.Add(this.errorLogs);
            this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.Name = "ErrorViewer";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ErrorViewer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button clearErrorLog;
        public System.Windows.Forms.TextBox errorLogs;
    }
}