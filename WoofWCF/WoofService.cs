using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using Woof.Core;

namespace Woof.ServiceModel {

    
    public class WoofService {

        /// <summary>
        /// Separator line matching console window width
        /// </summary>
        string Line { get { return "".PadRight(Console.WindowWidth, '-'); } }

        /// <summary>
        /// Service information string
        /// </summary>
        string InfoString {
            get {
                var builder = new StringBuilder();
                builder.Append("{0}");
                builder.AppendLine("{1} v{2}");
                builder.Append("{0}");
                builder.AppendLine("Description:{3}");
                builder.AppendLine("Service log:{4}");
                builder.AppendLine("Service name:{5}");
                builder.AppendLine("Service user:{6}");
                builder.AppendLine("Listens at:{7}");
                builder.AppendLine("Origins allowed:{8}");
                builder.Append("{0}");
                return String.Format(
                    builder.ToString().Replace(":{", "\t:  {"),
                    Line,
                    Config.DisplayName,
                    Config.Version,
                    Config.Description,
                    Config.Company,
                    Config.ServiceName,
                    Config.ServiceUser,
                    Config.Uri,
                    String.Join("; ", Config.OriginsAllowed)
                );
            }
        }

        /// <summary>
        /// Installer help string
        /// </summary>
        string HelpString {
            get {
                return String.Format(
                    Messages.InstallerHelp,
                    Config.DisplayName,
                    Config.Description,
                    Path.GetFileName(Application.ExecutablePath)
                );
            }
        }

        /// <summary>
        /// Program type
        /// </summary>
        Type ProgramType;
        
        /// <summary>
        /// Program command line arguments
        /// </summary>
        ArgsParser ProgramArgs;

        /// <summary>
        /// Value which should be returned from Main()
        /// </summary>
        public int ReturnValue;

        public WoofService(Type programType, Type serviceType, string[] args) {
            ProgramType = programType;
            ProgramArgs = new ArgsParser(args);
            Config.Type = serviceType;
            if (Config.Preset) Config.SetFromAssembly(programType.Assembly);
            if (ProgramArgs.NoArgs && ProgramArgs.NoOptions && ProgramArgs.NoParameters) ServiceBase.Run(new[] { new WinService() });
            else if (ProgramArgs.HasOption("i")) Install();
            else if (ProgramArgs.HasOption("u")) Uninstall();
            else if (ProgramArgs.HasOption("t")) Test();
            else Help();
        }

        /// <summary>
        /// Installs the service
        /// </summary>
        void Install() {
            Console.Write(InfoString);
            try {
                using (var i = new AssemblyInstaller(ProgramType.Assembly, null) { UseNewContext = true }) {
                    var s = new Hashtable();
                    try {
                        i.Install(s);
                        i.Commit(s);
                    } catch {
                        try {
                            i.Rollback(s);
                        } catch { }
                        throw;
                    }
                }
                Console.WriteLine(Messages.Done);
            } catch (Exception x) {
                Console.Error.WriteLine(x.Message);
                ReturnValue = 1;
            }
        }

        /// <summary>
        /// Handles uninstall options
        /// </summary>
        /// <returns></returns>
        void Uninstall() {
            string s = ProgramArgs.HasOption("s") ? ProgramArgs.GetOptionValue("s") : null;
            string e = ProgramArgs.HasOption("e") ? ProgramArgs.GetOptionValue("e") : null;
            string l = ProgramArgs.HasOption("l") ? ProgramArgs.GetOptionValue("l") : null;
            if (string.IsNullOrEmpty(s)) {
                if (String.IsNullOrEmpty(e) && String.IsNullOrEmpty(l)) UninstallThis();
            } else UninstallService(s);
            if (!String.IsNullOrEmpty(e)) {
                if (EventLog.SourceExists(e)) {
                    Console.Write(String.Format(Messages.DeletingEventSource, e));
                    EventLog.DeleteEventSource(e);
                    Console.WriteLine(Messages.OK);
                } else {
                    Console.WriteLine(String.Format(Messages.NoEventSource, e));
                    ReturnValue = 1;
                }
            }
            if (!String.IsNullOrEmpty(l)) {
                if (EventLog.Exists(l)) {
                    Console.Write(String.Format(Messages.DeletingEventLog, l));
                    EventLog.Delete(l);
                    Console.WriteLine(Messages.OK);
                } else {
                    Console.WriteLine(String.Format(Messages.NoEventLog, l));
                    ReturnValue = 1;
                }
            }
        }

        /// <summary>
        /// Uninstalls this service
        /// </summary>
        void UninstallThis() {
            try {
                using (var i = new AssemblyInstaller(ProgramType.Assembly, null) { UseNewContext = true }) {
                    IDictionary s = new Hashtable();
                    try {
                        i.Uninstall(s);
                    } catch {
                        try {
                            i.Rollback(s);
                        } catch { }
                        throw;
                    }
                }
                Console.WriteLine(Messages.Done);
            } catch (Exception x) {
                Console.Error.WriteLine(x.Message);
                ReturnValue = 1;
            }
        }

        /// <summary>
        /// Uninstalls specified service
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        void UninstallService(string name) {
            try {
                using (var i = new System.ServiceProcess.ServiceInstaller() { Context = new InstallContext(), ServiceName = name }) {
                    try {
                        i.Uninstall(null);
                    } catch {
                        try {
                            i.Rollback(null);
                        } catch { }
                        throw;
                    }
                    Console.WriteLine(Messages.Done);
                }
            } catch (Exception x) {
                Console.Error.WriteLine(x.Message);
                ReturnValue = 1;
            }
        }

        /// <summary>
        /// Tests service in console
        /// </summary>
        void Test() {
            Config.TestMode = true;
            Console.WindowWidth = 160;
            Console.WindowHeight = 50;
            Console.Write(InfoString);
            Console.WriteLine(Messages.InitializingService);
            var service = new WinService();
            Console.WriteLine(Messages.Starting);
            service.Start();
            Console.ReadKey(true);
            service.Shutdown();
            Console.WriteLine(Messages.Done);
        }

        /// <summary>
        /// Displays installer help
        /// </summary>
        void Help() {
            Console.WriteLine(HelpString);
        }

    }

}