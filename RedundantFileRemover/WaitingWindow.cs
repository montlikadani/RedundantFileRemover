using System;
using System.Windows.Forms;

namespace RedundantFileRemover {
    public partial class WaitingWindow : Form {

        public WaitingWindow(Form owner) {
            InitializeComponent();

            Owner = owner;

            string nl = Environment.NewLine;

            infoText.Text = $"Please wait until the program collects all of directories... {nl + nl}" +
                        $"This process can take more than 30 sec depending how many files are on your drive.";
        }

        private void WaitingWindow_FormClosed(object sender, FormClosedEventArgs e) {
            RedundantFileRemover.TerminateRequested = true;
        }

        private void WaitingWindow_Resize(object sender, EventArgs e) {
            Owner.WindowState = WindowState;
        }
    }
}
