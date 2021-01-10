using System;
using System.Windows.Forms;

namespace RedundantFileRemover {
    static class Program {

        private static UserSettingsData.FileDataReader fdr;
        public static UserSettingsData.FileDataReader ConfigFile => fdr;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            fdr = new UserSettingsData.FileDataReader();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RedundantFileRemover());
        }
    }
}
