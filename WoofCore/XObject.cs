using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Woof.Core {
    
    /// <summary>
    /// Extended object methods
    /// </summary>
    public static class XObject {

        /// <summary>
        /// Makes fast, shallow copy of an object
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="i">source object</param>
        /// <returns>shallow copy</returns>
        public static T Clone<T>(T i) {
            return (T)i.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(i, null);
        }

        /// <summary>
        /// Makes full copy of an object using XML serialization over MemoryStream
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="i">source object</param>
        /// <returns>full copy</returns>
        public static T CloneDeep<T>(T i) {
            if (Object.ReferenceEquals(i, null)) return default(T);
            var x = new XmlSerializer(i.GetType());
            using (var m = new MemoryStream()) {
                x.Serialize(m, i);
                m.Seek(0, SeekOrigin.Begin);
                return (T)x.Deserialize(m);
            }
        }

    }

}