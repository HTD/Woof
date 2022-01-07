using System;
using NPOI.SS.UserModel;

namespace Woof.NPOI {

    /// <summary>
    /// Provides metadata for XSSF cell properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class XCellAttribute : Attribute {

        /// <summary>
        /// Gets or sets the value defining the horizontal alignment for the cell.
        /// </summary>
        public HorizontalAlignment Alignment { get; set; }

        /// <summary>
        /// Gets or sets the column display name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the data format string.<br/>
        /// For date formats use '/' as date part separator, ':' as time separator. Use YMDHMS for date and time parts.<br/>
        /// For number formats use '#' for number, '0' for number or zero, '.' as decimal point and ',' for thousands separator.
        /// </summary>
        public string? DataFormat { get; set; }

        /// <summary>
        /// Gets or sets the column width in points.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the column should be added to the summary row.
        /// </summary>
        public bool AddSum { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the column should be skipped in export.
        /// </summary>
        public bool Skip { get; set; }

    }

}