using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace tests;

internal class CopyPasteTests : Tests
{
    [Test]
    public void CannotCopyRoot()
    {
        var d = Get10By10View();
        
        var top = new Toplevel();
        top.Add(d.View);

        Assert.IsTrue(d.IsRoot);
        var copy = new CopyOperation(d,new MultiSelectionManager());

        Assert.IsTrue(copy.IsImpossible);
    }

    [Test]
    public void CopyPasteTableView()
    {
        var d = Get10By10View();

        var tv = (TableView)new ViewFactory().Create(typeof(TableView));
        
        Assert.IsTrue(
            new AddViewOperation(d.SourceCode,tv,d,"mytbl").Do()
        );

        var tvDesign = (Design)tv.Data;

        Assert.IsFalse(tv.Style.InvertSelectedCellFirstCharacter,
            "Expected default state for this flag to be false");

        Assert.IsFalse(tv.FullRowSelect,
            "Expected default state for this flag to be false");

        tv.Table.Rows.Clear();
        tv.Table.Columns.Clear();
        tv.Table.Columns.Add("Yarg",typeof(int));
        tv.Table.Columns.Add("Blerg",typeof(DateTime));

        // flip these flags to so we can check that it is
        // properly cloned
        tv.Style.InvertSelectedCellFirstCharacter = true;
        tv.FullRowSelect = true;

        OperationManager.Instance.ClearUndoRedo();

        var selectionManager = new MultiSelectionManager();

        var copy = new CopyOperation(tvDesign,selectionManager);
        OperationManager.Instance.Do(copy);
        
        Assert.AreEqual(0,OperationManager.Instance.UndoStackSize,
            "Since you cannot Undo a Copy we expected undo stack to be empty");

        Assert.IsEmpty(selectionManager.Selected);

        var paste = new PasteOperation(d,selectionManager);
        OperationManager.Instance.Do(paste);

        Assert.AreEqual(1,OperationManager.Instance.UndoStackSize,
            "Undo stack should contain the paste operation");

        Assert.IsNotEmpty(selectionManager.Selected,
            "After pasting, the new clone should be selected");

        var tv2Design = selectionManager.Selected.Single();
        var tv2 = (TableView)tv2Design.View;

        // The cloned table style/properties should match the copied ones
        Assert.IsTrue(tv2.Style.InvertSelectedCellFirstCharacter);
        Assert.IsTrue(tv2.FullRowSelect);

        // The cloned table columns should match the copied ones
        Assert.AreEqual(2,tv2.Table.Columns.Count);
        Assert.AreEqual(0,tv2.Table.Rows.Count);
        Assert.AreEqual("Yarg",tv2.Table.Columns[0].ColumnName);
        Assert.AreEqual("Blerg",tv2.Table.Columns[1].ColumnName);
        Assert.AreEqual(typeof(int),tv2.Table.Columns[0].DataType);
        Assert.AreEqual(typeof(DateTime),tv2.Table.Columns[1].DataType);

        Assert.AreNotSame(tv.Table,tv2.Table, 
            "Cloned table should be a new table not a reference to the old one");
    }
}
