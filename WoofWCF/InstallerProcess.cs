using System.ComponentModel;
using System.ServiceProcess;

namespace Woof.ServiceModel {
    
    /// <summary>
    /// Extend this class in service assembly
    /// </summary>
    [RunInstaller(true)]
    public class InstallerProcess : ServiceProcessInstaller {

        public InstallerProcess() {
            var user = Config.ServiceUser;
            var passwd = Config.ServicePassword;
            if (passwd == "") passwd = null;
            switch (user) {
                case "LocalService":
                    this.Account = ServiceAccount.LocalService;
                    break;
                case "LocalSystem":
                    this.Account = ServiceAccount.LocalSystem;
                    break;
                case "NetworkService":
                case "":
                    this.Account = ServiceAccount.NetworkService;
                    break;
                default:
                    this.Account = ServiceAccount.User;
                    this.Username = user;
                    this.Password = passwd;
                    break;
            }
        }
    }

}