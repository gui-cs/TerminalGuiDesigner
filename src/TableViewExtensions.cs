using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Extension methods for the <see cref="DataTable"/> class.
/// </summary>
public static class TableViewExtensions
{
    /// <summary>
    /// Returns the <see cref="TableView.Table"/> as a <see cref="DataTable"/>
    /// by hard casting. This will fail if source is e.g. <see cref="EnumerableTableSource{T}"/>.
    /// </summary>
    /// <param name="tv">TableView to get the underlying table from.</param>
    /// <returns>Underlying data table wrapped by <paramref name="tv"/>.</returns>
    public static DataTable GetDataTable(this TableView tv)
    {
        return ((DataTableSource)tv.Table).DataTable;
    }

    /// <summary>
    /// Reorders all <paramref name="newOrder"/> columns to match the element order
    /// they appear in.
    /// </summary>
    /// <param name="tv"><see cref="TableView"/> whose <see cref="TableView.Table"/> columns will be changed.</param>
    /// <param name="newOrder">The new order for all columns in <paramref name="tv"/>.</param>
    public static void ReOrderColumns(this TableView tv, DataColumn[] newOrder)
    {
        var dt = tv.GetDataTable();

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
