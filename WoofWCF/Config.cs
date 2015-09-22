using System;
using System.Reflection;
using Woof.Core;

namespace Woof.ServiceModel {

    public static class Config {

        public static Type Type { get; set; }
        public static string Company { get; set; }
        public static string ServiceName { get; set; }
        public static string Version { get; set; }
        public static string ServiceUser { get; set; }
        public static string ServicePassword { get; set; }
        public static string DisplayName { get; set; }
        public static string Description { get; set; }
        public static string Uri { get; set; }
        public static string DefaultContentType { get; set; }
        public static string[] OriginsAllowed { get; set; }
        public static bool SupportIncompatibleBrowsers { get; set; }
        public static int MaxBufferSize { get; set; }
        public static int MaxReceivedMessageSize { get; set; }
        public static bool TestMode { get; set; }
        public static bool Preset { get; set; }
        public static Action StartAction { get; set; }
        public static Action StopAction { get; set; }


        
        /// <summary>
        /// Default service configuration
        /// </summary>
        static Config() {
            Company = "CodeDog";
            ServiceName = "Woof";
            Version = "1.0";
            ServiceUser = "LocalSystem";
            DisplayName = "WOOF!";
            Description = "WOOF! WOOF!";
            Uri = "http://localhost:8080/woof";
            DefaultContentType = "application/json; charset=UTF-8";
            OriginsAllowed = new string[] { "*" };
            SupportIncompatibleBrowsers = true;
            Preset = true;
            MaxBufferSize = 524288;
            MaxReceivedMessageSize = 524288;
        }

        /// <summary>
        /// Sets service configuration from assembly
        /// </summary>
        /// <param name="assembly"></param>
        internal static void SetFromAssembly(Assembly assembly) {
            var assemblyInfo = new AssemblyInfo(assembly);
            ServiceName = assemblyInfo.Product;
            Version = assemblyInfo.Version.ToString();
            DisplayName = assemblyInfo.Title;
            Description = assemblyInfo.Description;
            Company = assemblyInfo.Company;
        }
        
    }

}