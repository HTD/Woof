using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Woof.Core {

    /// <summary>
    /// Argument processing class
    /// </summary>
    public class ArgsParser {

        /// <summary>
        /// True if the command line contains no arguments
        /// </summary>
        public bool NoArgs = true;

        /// <summary>
        /// True if the command line contains no parameters
        /// </summary>
        public bool NoParameters = true;

        /// <summary>
        /// True if the command line contains no options
        /// </summary>
        public bool NoOptions = true;

        /// <summary>
        /// Command line arguments without switch characters
        /// </summary>
        private readonly Dictionary<string, string> _Parameters = new Dictionary<string, string>();

        /// <summary>
        /// Command line arguments starting with switch
        /// </summary>
        private readonly Dictionary<string, string> _Options = new Dictionary<string, string>();

        /// <summary>
        /// Command line arguments without switch characters, getter
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string[] Parameters { get { return _Parameters.Keys.ToArray(); } }

        /// <summary>
        /// Command line arguments without switch characters, getter
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string[] Options { get { return _Options.Keys.ToArray(); } }

        /// <summary>
        /// Parses command line into parameters and options
        /// </summary>
        /// <param name="args">command line arguments</param>
        public ArgsParser(IEnumerable<string> args) {
            foreach (var arg in args) {
                var match = Regex.Match(arg, @"^(/|-{0,2})(.*)$");
                if (match.Success) {
                    NoArgs = false;
                    var part = match.Groups[2].Value;
                    var split = part.Split('=');
                    if (match.Groups[1].Value.Length < 1) {
                        if (_Parameters.ContainsKey(split[0])) _Parameters[split[0]] = split.Length > 1 ? split[1] : null;
                        else _Parameters.Add(split[0], split.Length > 1 ? split[1] : null);
                        NoParameters = false;
                    } else {
                        if (_Options.ContainsKey(split[0])) _Options[split[0]] = split.Length > 1 ? split[1] : null;
                        else _Options.Add(split[0], split.Length > 1 ? split[1] : null);
                        NoOptions = false;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if command line contains specified parameter
        /// </summary>
        /// <param name="value">parameter name</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool HasParameter(string value) {
            var aliases = value.Split('|');
            return aliases.Any(alias => _Parameters.ContainsKey(alias));
        }

        /// <summary>
        /// Returns true if command line contains specified option
        /// </summary>
        /// <param name="value">option name</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool HasOption(string value) {
            var aliases = value.Split('|');
            return aliases.Any(alias => _Options.ContainsKey(alias));
        }

        /// <summary>
        /// Returns specified command line parameter value
        /// </summary>
        /// <param name="key">parameter name</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string GetParameterValue(string key) {
            return _Parameters[key];
        }

        /// <summary>
        /// Returns specified command line option value
        /// </summary>
        /// <param name="key">option name</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string GetOptionValue(string key) {
            return _Options[key];
        }

    }

}