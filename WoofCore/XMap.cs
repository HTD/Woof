using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Woof.Core {
    
    /// <summary>
    /// Information about fields, properties and object values of any given object
    /// </summary>
    public class XMap : XDictionary<string, object> {
        /// <summary>
        /// Target object
        /// </summary>
        public object Target;
        /// <summary>
        /// Binding flags used for mapping properties and fields, Public | Instance are used as defaults
        /// </summary>
        public BindingFlags Binding;
        /// <summary>
        /// If mapping of properties and fields is done
        /// </summary>
        private bool Done;
        /// <summary>
        /// Type cache
        /// </summary>
        private Type _Type;
        /// <summary>
        /// Properties cache
        /// </summary>
        private PropertyInfo[] _Properties;
        /// <summary>
        /// Fields cache
        /// </summary>
        private FieldInfo[] _Fields;
        /// <summary>
        /// Target type
        /// </summary>
        private Type Type { get { return _Type ?? (_Type = Target.GetType()); } }
        /// <summary>
        /// Target properties
        /// </summary>
        private PropertyInfo[] Properties { get { return _Properties ?? (_Properties = Type.GetProperties(Binding)); } }
        /// <summary>
        /// Target fields
        /// </summary>
        private FieldInfo[] Fields { get { return _Fields ?? (_Fields = Type.GetFields(Binding)); } }
        /// <summary>
        /// Target types
        /// </summary>
        public Dictionary<string, Type> Types { get; set; }
        /// <summary>
        /// First property or field
        /// </summary>
        public KeyValuePair<string, object> First { get { return this.First(); } }
        /// <summary>
        /// Named property or field access override
        /// </summary>
        /// <param name="key">property or field name</param>
        /// <returns>value</returns>
        public new object this[string key] {
            get { return base[key]; }
            set {
                base[key] = value;
                if (Done) {
                    if (Type.IsValueType) throw new InvalidOperationException("Value types cannot be assigned like this");
                    SetValue(key, value);
                }
            }
        }

        /// <summary>
        /// Creates object map as special dictionary
        /// </summary>
        /// <param name="target">target object</param>
        /// <param name="binding">binding flags used for mapping properties and fields, Public | Instance are used as defaults</param>
        public XMap(object target, BindingFlags binding = BindingFlags.Default) {
            if (target == null) throw new ArgumentNullException("target");
            Binding = binding == BindingFlags.Default ? BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public : binding;
            Target = target;
            Types = new Dictionary<string, Type>();
            if (Properties.Length > 0)
                foreach (var p in Properties) {
                    Types.Add(p.Name, p.PropertyType);
                    Add(p.Name, p.GetValue(target));
                }
            if (Fields.Length > 0)
                foreach (var f in Fields) {
                    Types.Add(f.Name, f.FieldType);
                    Add(f.Name, f.GetValue(target));
                }
            Done = true;
        }

        /// <summary>
        /// Sets value of a specified property or field
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void SetValue(string key, object value) {
            if ((new List<PropertyInfo>(Properties)).Exists(p => p.Name == key))
                Type.GetProperty(key, Binding).SetValue(Target, value);
            if ((new List<FieldInfo>(Fields)).Exists(f => f.Name == key))
                Type.GetField(key, Binding).SetValue(Target, value is DBNull ? null : value);
        }

    }

}