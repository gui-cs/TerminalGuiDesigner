using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner
{
    /// <summary>
    /// Extension methods for the <see cref="DataTable"/> class.
    /// </summary>
    public static class TableViewExtensions
    {
        /// <summary>
        /// Reorders all <paramref name="newOrder"/> columns to match the element order
        /// they appear in.
        /// </summary>
        /// <param name="tv"><see cref="TableView"/> whose <see cref="TableView.Table"/> columns will be changed.</param>
        /// <param name="newOrder">The new order for all columns in <paramref name="tv"/>.</param>
        public static void ReOrderColumns(this TableView tv, DataColumn[] newOrder)
        {
            var dt = tv.Table;

            foreach (DataColumn toRemove in dt.Columns.Cast<DataColumn>().Except(newOrder).ToArray())
            {
                dt.Columns.Remove(toRemove);
            }

            if (newOrder.Intersect(dt.Columns.Cast<DataColumn>()).Count() != newOrder.Length)
            {
                throw new ArgumentException("Not all columns provided appeared in DataTable or additional columns were in the provided array.");
            }

            for (int i = 0; i < newOrder.Length; i++)
            {
                newOrder[i].SetOrdinal(i);
            }

            tv.Update();
        }
    }
}
