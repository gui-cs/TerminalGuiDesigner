using System.Data;
using Terminal.Gui;

namespace TerminalGuiDesigner;

/// <summary>
/// Creates new <see cref="View"/> instances configured to have
/// sensible dimensions and content for dragging/configuring in
/// the designer.
/// </summary>
public class ViewFactory
{
    public ViewFactory()
    {
    }

    public View Create(Type t)
    {
        if (typeof(TableView).IsAssignableFrom(t))
        {
            return CreateTableView();
        }

        var instance = (View)Activator.CreateInstance(t);

        instance.SetActualText("Heya");

        instance.Width = Math.Max(instance.Bounds.Width, 4);

        return instance;
    }

    private TableView CreateTableView()
    {
        var dt = new DataTable();
        dt.Columns.Add("Column 0");
        dt.Columns.Add("Column 1");
        dt.Columns.Add("Column 2");
        dt.Columns.Add("Column 3");

        return new TableView
        {
            Width = 50,
            Height = 5,
            Table = dt
        };
    }
}
