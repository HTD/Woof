using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Woof.ServiceModel {
    
    /// <summary>
    /// Extend this class in service assembly
    /// </summary>
    [RunInstaller(true)]
    public class Installer : System.ServiceProcess.ServiceInstaller {

        public Installer() {
            this.ServiceName = Config.ServiceName;
            this.DisplayName = Config.DisplayName;
            this.Description = Config.Description;
            this.DelayedAutoStart = false;
            this.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.AfterInstall += MyServiceInstaller_AfterInstall;
        }

        void MyServiceInstaller_AfterInstall(object sender, InstallEventArgs e) {
            ServiceController controller = new ServiceController(this.ServiceName);
            try {
                controller.Start();

            } finally {
                controller.Dispose();
            }
        }
    }

}
