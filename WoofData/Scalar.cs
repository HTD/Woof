using System;
using System.Diagnostics.CodeAnalysis;

namespace Woof.Data {
    
    /// <summary>
    /// Special SQL scalar return type with automated implicit conversion and comparison
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
    public class Scalar {

        /// <summary>
        /// Actual object value
        /// </summary>
        private readonly object Value;

        /// <summary>
        /// Constructor assigning the value
        /// </summary>
        /// <param name="value"></param>
        public Scalar(object value) {
            Value = value;
        }

        /// <summary>
        /// Equality / null test
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj == null) return false;
            var s = obj as Scalar;
            if (s == null) return false;
            if (s.Value == null || s.Value is DBNull) return false;
            return s.Value == Value;
        }

        /// <summary>
        /// Equality test helper
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        /// <summary>
        /// String value override
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Value.ToString();
        }

        /// <summary>
        /// Equality operator override
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator ==(Scalar s1, Scalar s2) {
            var o1IsNull = ReferenceEquals(s1, null);
            var o2IsNull = ReferenceEquals(s2, null);
            if (o1IsNull) return o2IsNull || s2.Value is DBNull || s2.Value == null;
            if (o2IsNull) return s1.Value is DBNull || s1.Value == null;
            return s1.Value != s2.Value;
        }

        /// <summary>
        /// Inequality operator override
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static bool operator !=(Scalar s1, Scalar s2) {
            return !(s1 == s2);
        }

        /// <summary>
        /// Scalar to String conversion
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator string(Scalar s) {
            return s.Value == null ? null : s.Value.ToString();
        }

        /// <summary>
        /// Scalar to decimal conversion
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator decimal(Scalar s) {
            if (s.Value == null) return 0;
            if (s.Value is decimal) return (decimal)s.Value;
            if (s.Value.ToString() == "") return 0;
            if (s.Value.ToString() == "True") return 1;
            if (s.Value.ToString() == "False") return 0;
            return decimal.Parse(s);
        }

        /// <summary>
        /// Scalar to int conversion
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator int(Scalar s) {
            if (s.Value == null) return 0;
            if (s.Value is int) return (int)s.Value;
            if (s.Value.ToString() == "") return 0;
            if (s.Value.ToString() == "True") return 1;
            if (s.Value.ToString() == "False") return 0;
            return int.Parse(s);
        }

        /// <summary>
        /// Scalar to bool conversion
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator bool(Scalar s) {
            if (s.Value == null) return false;
            if (s.Value is int) return (int)s.Value != 0;
            if (s.Value.ToString() == "True") return true;
            if (s.Value.ToString() == "False") return false;
            return s > 0;
        }

        /// <summary>
        /// Scalar to DateTime conversion
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator DateTime(Scalar s) {
            if (s.Value == null) return DateTime.MinValue;
            return (DateTime)s.Value;
        }
    }

}