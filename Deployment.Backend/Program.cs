using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deployment.Backend {
    class Program {
        static void Main(string[] args) {
            var assembly = new Woof.Core.AssemblyInfo(typeof(Program).Assembly);
            var version = assembly.Version;
            var title = assembly.Title;
            var message = String.Format("Woof Backend v{0} installed.", version);
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
