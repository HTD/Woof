using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Woof.Core;

namespace Woof.Data {
    
    /// <summary>
    /// SQL compatible boxed object value class
    /// </summary>
    public class Convert {

        /// <summary>
        /// SQL compatible boxed object
        /// </summary>
        public object Value;

        /// <summary>
        /// Converts a boxed object value to SQL compatible boxed object
        /// </summary>
        /// <param name="x"></param>
        /// <param name="t"></param>
        public Convert(object x, Type t = null) {
            if (x == null) { Value = DBNull.Value; return; }
            if (t == null) t = x.GetType();
            if (t.IsPrimitive || t.Equals(typeof(decimal)) || t.Equals(typeof(string)) || t.Equals(typeof(DateTime))) { Value = x; return; }
            if (x is List<int>) { Value = GetDataTable(x as List<int>); return; }
            if (x is List<string>) { Value = GetDataTable(x as List<string>); return; }
            if (x is IList) { Value = GetDataTable(x as IList); return; }
            if (x is Guid) { Value = ((Guid)x).ToString(); return; }
            Value = GetDataTable(x);
        }

        /// <summary>
        /// Converts generic object to DataTable with 1 row
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private DataTable GetDataTable(object x) {
            var m = new XMap(x);
            var t = new DataTable();
            var r = new object[m.Values.Count];
            var i = 0;
            Type type;
            Type nnType;
            m.ForEach(d => {
                type = m.Types[d.Key];
                nnType = Nullable.GetUnderlyingType(type);
                if (nnType != null) type = nnType;
                t.Columns.Add(d.Key, type);
                if (d.Value == null) r[i++] = DBNull.Value;
                else r[i++] = (m.Types[d.Key] == typeof(DateTime) && (DateTime)d.Value == DateTime.MinValue) ? DBNull.Value : d.Value;
            });
            t.Rows.Add(r);
            return t;
        }

        /// <summary>
        /// Converts generic list of objects to DataTable
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private DataTable GetDataTable(IList x) {
            if (x.Count < 1) return null;
            var m = new XMap(x[0]);
            var c = m.Values.Count;
            var t = GetDataTable(x[0]);
            object[] r;
            var rt = x.Count;
            for (int i = 0, rc = 1; rc < rt; i = 0, rc++) {
                r = new object[c];
                m = new XMap(x[rc]);
                m.ForEach(d => {
                    if (d.Value == null) r[i++] = DBNull.Value;
                    else r[i++] = (m.Types[d.Key] == typeof(DateTime) && (DateTime)d.Value == DateTime.MinValue) ? DBNull.Value : d.Value;
                });
                t.Rows.Add(r);
            }
            return t;
        }

        /// <summary>
        /// Converts a list of integers to DataTable
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private DataTable GetDataTable(List<int> x) {
            var t = new DataTable();
            t.Columns.Add("item", typeof(int));
            x.ForEach(i => t.Rows.Add(new object[] { i }));
            return t;
        }

        /// <summary>
        /// Converts a list of strings to DataTable
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private DataTable GetDataTable(List<string> x) {
            var t = new DataTable();
            t.Columns.Add("item", typeof(string));
            x.ForEach(i => t.Rows.Add(new object[] { i }));
            return t;
        }

    }

}