using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;

class TextValidateFieldTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveProvider()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PreserveProvider.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Window));

        var factory = new ViewFactory();
        var tvfOut = (TextValidateField)factory.Create(typeof(TextValidateField));

        Assert.IsNotNull(tvfOut.Provider);

        OperationManager.Instance.Do(new AddViewOperation(tvfOut, designOut, "myfield"));

        viewToCode.GenerateDesignerCs(designOut, typeof(Window));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tvfIn = designBackIn.View.GetActualSubviews().OfType<TextValidateField>().Single();

        Assert.IsNotNull(tvfIn.Provider);
    }
}
