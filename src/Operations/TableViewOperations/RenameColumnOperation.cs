﻿using System.Data;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.TableViewOperations;

/// <summary>
/// Renames a <see cref="DataColumn"/> in a <see cref="TableView"/>.
/// </summary>
public class RenameColumnOperation : ColumnOperation
{
    private string? originalName;
    private string? newColumnName;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenameColumnOperation"/> class.
    /// </summary>
    /// <param name="design">The <see cref="Design"/> wrapper for a <see cref="TableView"/>.</param>
    /// <param name="column">The column to rename.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="design"/> does not wrap a <see cref="TableView"/>.</exception>
    public RenameColumnOperation(Design design, DataColumn column)
        : base(design, column)
    {
        this.originalName = column.ColumnName;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Rename Column '{originalName}'";
    }

    /// <inheritdoc/>
    public override void Redo()
    {
        Column.ColumnName = newColumnName;
        TableView.Update();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        Column.ColumnName = originalName;
        TableView.Update();
    }

    /// <inheritdoc/>
    protected override bool DoImpl()
    {
        if (Modals.GetString("Rename Column", "Column Name", originalName, out var newColumnName))
        {
            this.newColumnName = newColumnName;
            Column.ColumnName = newColumnName;
            TableView.Update();
            return true;
        }

        return false;
    }
}