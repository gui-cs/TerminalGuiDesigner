using System.IO;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests;

internal class LabelTests : Tests
{
    [Test]
    public void Test_ChangingLabelX()
    {
        var file = new FileInfo("Test_ChangingLabelX.cs");
        var viewToCode = new ViewToCode();
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));

        var op = new AddViewOperation(new Label("Hello World"), designOut, "myLabel");
        op.Do();

        // the Hello world label
        var lblDesign = designOut.GetAllDesigns().Single(d => d.View is Label);
        var xProp = lblDesign.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals("X"));

        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), Pos.At(10)));
        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), Pos.Percent(50)));
        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), Pos.At(10)));
        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), Pos.Percent(50)));
        OperationManager.Instance.Undo();
        OperationManager.Instance.Undo();
        OperationManager.Instance.Redo();
        OperationManager.Instance.Redo();

        ClassicAssert.AreEqual($"Factor({0.5})",lblDesign.View.X.ToString());
    }

    [Test]
    public void Test_ChangingLabelX_PosDesigner()
    {
        var file = new FileInfo("Test_ChangingLabelX.cs");
        var viewToCode = new ViewToCode();
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));

        var op = new AddViewOperation(new Label("Hello World"), designOut, "myLabel");
        op.Do();

        // the Hello world label
        var lblDesign = designOut.GetAllDesigns().Single(d => d.View is Label);
        var xProp = lblDesign.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals("X"));

        lblDesign.View.IsInitialized = true;

        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), Pos.At(10)));

        var percent50 = Pos.Percent(50);
        var percent30 = Pos.Percent(30);

        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), percent50));
        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), Pos.At(10)));
        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), percent30));
        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), percent50));
        OperationManager.Instance.Do(new SetPropertyOperation(lblDesign, xProp, xProp.GetValue(), Pos.At(10)));

        OperationManager.Instance.Undo();
        OperationManager.Instance.Redo();
        OperationManager.Instance.Undo();
        OperationManager.Instance.Redo();
        OperationManager.Instance.Undo();
        OperationManager.Instance.Undo();
        OperationManager.Instance.Redo();
        OperationManager.Instance.Redo();
        OperationManager.Instance.Undo();

        ClassicAssert.AreEqual($"Factor({0.5})", lblDesign.View.X.ToString());
    }
}