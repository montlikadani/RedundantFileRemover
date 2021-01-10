using System.Collections;
using System.ComponentModel;
using System.IO;

namespace RedundantFileRemover {
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer {

        public Installer() {
            InitializeComponent();
        }

        public override void Uninstall(IDictionary savedState) {
            base.Uninstall(savedState);

            var path = Path.Combine(Context.Parameters["path"], "userConfigData.yml");
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }
}
