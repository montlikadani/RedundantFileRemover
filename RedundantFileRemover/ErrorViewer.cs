using System;
using System.Windows.Forms;

namespace RedundantFileRemover {
    public partial class ErrorViewer : Form {

        public ErrorViewer() {
            InitializeComponent();
        }

        private void clearErrorLog_Click(object sender, EventArgs e) {
            errorLogs.Clear();
        }

        private void errorLogs_TextChanged(object sender, EventArgs e) {
            if (errorLogs.TextLength + 50 >= errorLogs.MaxLength) {
                errorLogs.Clear(); // Auto clean up
            }
        }
    }
}
