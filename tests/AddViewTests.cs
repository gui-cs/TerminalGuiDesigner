using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;

public class AddViewTests : Tests
{
    [Test]
    public void TestAdd_Undo()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAdd_Undo.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Dialog), out var sourceCode);

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        var op = new AddViewOperation(sourceCode, lbl, designOut, "label1");

        OperationManager.Instance.Do(op);
        Assert.AreEqual(1,designOut.View.GetActualSubviews().OfType<Label>().Count());

        OperationManager.Instance.Undo();
        Assert.AreEqual(0,designOut.View.GetActualSubviews().OfType<Label>().Count());

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Dialog));

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        Assert.AreEqual(0,designBackIn.View.GetActualSubviews().OfType<Label>().Count());
    }

    [Test]
    public void TestAddUndoRedo_RoundTrip()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAddUndoRedo_RoundTrip.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Dialog), out var sourceCode);

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        var op = new AddViewOperation(sourceCode, lbl, designOut, "label1");

        OperationManager.Instance.Do(op);
        OperationManager.Instance.Undo();
        OperationManager.Instance.Redo();

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Dialog));

        var lblOut = designOut.View.GetActualSubviews().OfType<Label>().Single();

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblIn = designBackIn.View.GetActualSubviews().OfType<Label>().Single();

        Assert.AreEqual(lblOut.Text,lblIn.Text);
    }
    

    /// <summary>
    /// 60 is one of those numbers that can't be modelled exactly in float so ends
    /// up with many decimal places.  This needs to be rounded in the code generated
    /// and has to have the suffix 'f' to ensure that the value is treated as a float
    /// and not a double (which won't compile)
    /// </summary>
    /// <param name="offset"></param>
    [TestCase(1)]
    [TestCase(0)]
    [TestCase(-1)]
    public void Test60Percent_RoundTrip(int offset)
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("Test60Percent_RoundTrip.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog), out var sourceCode);

        var factory = new ViewFactory();
        var lbl = factory.Create(typeof(Label));
        var op = new AddViewOperation(sourceCode, lbl, designOut, "label1");

        OperationManager.Instance.Do(op);
        lbl.Width = offset == 0 ? Dim.Percent(60) : Dim.Percent(60) + offset;
        lbl.X = offset == 0 ? Pos.Percent(60) : Pos.Percent(60) + offset;

        viewToCode.GenerateDesignerCs(designOut, sourceCode, typeof(Dialog));

        var lblOut = designOut.View.GetActualSubviews().OfType<Label>().Single();

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var lblIn = designBackIn.View.GetActualSubviews().OfType<Label>().Single();

        Assert.AreEqual(lblOut.Text, lblIn.Text);
    }
}