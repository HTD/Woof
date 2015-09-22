using System;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace Woof.Core {
    
    /// <summary>
    /// Quick access to current project's assebly information
    /// </summary>
    public class AssemblyInfo {

        private Assembly _A;
        private string _Name;
        private string _Title;
        private string _Description;
        private string _Company;
        private string _Product;
        private string _Copyright;
        private string _Trademark;
        private string _Version;
        private string _Namespace;

        public AssemblyInfo(Assembly assembly) { _A = assembly; }

        /// <summary>
        /// Executing assembly cache
        /// </summary>
        private Assembly A {
            get {
                return _A ?? (_A = Assembly.GetExecutingAssembly());
            }
        }

        /// <summary>
        /// Name (suitable for event source)
        /// </summary>
        public string Name {
            get {
                return _Name ?? (_Name = A.GetName().Name);
            }
        }

        /// <summary>
        /// Title (suitable for displayed service name)
        /// </summary>
        public string Title {
            get {
                return _Title ?? (_Title = (A.GetCustomAttribute<AssemblyTitleAttribute>().Title));
            }
        }

        /// <summary>
        /// Description (suitable for service description)
        /// </summary>
        public string Description {
            get {
                return _Description ?? (_Description = (A.GetCustomAttribute<AssemblyDescriptionAttribute>().Description));
            }
        }

        /// <summary>
        /// Company name
        /// </summary>
        public string Company {
            get {
                return _Company ?? (_Company = (A.GetCustomAttribute<AssemblyCompanyAttribute>().Company));
            }
        }

        /// <summary>
        /// Internal product name (suitable as service name identifier)
        /// </summary>
        public string Product {
            get {
                return _Product ?? (_Product = (A.GetCustomAttribute<AssemblyProductAttribute>().Product));
            }
        }

        /// <summary>
        /// Copyright information
        /// </summary>
        public string Copyright {
            get {
                return _Copyright ?? (_Copyright = (A.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright));
            }
        }

        /// <summary>
        /// Trademark
        /// </summary>
        public string Trademark {
            get {
                return _Trademark ?? (_Trademark = (A.GetCustomAttribute<AssemblyTrademarkAttribute>().Trademark));
            }
        }

        /// <summary>
        /// Version
        /// </summary>
        public string Version {
            get {
                return _Version ?? (_Version = Application.ProductVersion);
            }
        }

        /// <summary>
        /// Main program namespace
        /// </summary>
        public string Namespace {
            get {
                return _Namespace ?? (_Namespace = A.EntryPoint.ReflectedType.Namespace);
            }
        }

        /// <summary>
        /// Creates new ResourceManager from given resource file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ResourceManager Resources(string filename) {
            return new ResourceManager(String.Format("{0}.{1}", Namespace, filename), A);
        }
    }

}