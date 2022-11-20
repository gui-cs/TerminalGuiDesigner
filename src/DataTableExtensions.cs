using System.Data;

namespace TerminalGuiDesigner
{
    /// <summary>
    /// Extension methods for the <see cref="DataTable"/> class.
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// Reorders all <paramref name="newOrder"/> columns to match the element order
        /// they appear in.
        /// </summary>
        /// <param name="dt">Table whose columns to reorder</param>
        /// <param name="newOrder">The new order for all columns in <paramref name="dt"/>.</param>
        public static void ReOrderColumns(this DataTable dt, DataColumn[] newOrder)
        {
            if (newOrder.Intersect(dt.Columns.Cast<DataColumn>()).Count() != newOrder.Length)
            {
                throw new ArgumentException("Not all columns provided appeared in DataTable or additional columns were in the provided array.");
            }

            for (int i = 0; i < newOrder.Length; i++)
            {
                newOrder[i].SetOrdinal(i);
            }
        }
    }
}
