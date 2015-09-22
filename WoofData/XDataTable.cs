using System;
using System.Data;

namespace Woof.Data {

    /// <summary>
    /// DataTable extensions
    /// </summary>
    public class XDataTable {
        /// <summary>
        /// Returns new DataTable for given record type with optional name set
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DataTable For(Type type, string name = null) {
            var dataTable = new DataTable(name);
            var properties = type.GetProperties();
            var fields = type.GetFields();
            foreach (var p in properties) dataTable.Columns.Add(p.Name, p.PropertyType);
            foreach (var f in fields) dataTable.Columns.Add(f.Name, f.FieldType);
            return dataTable;
        }

    }

}