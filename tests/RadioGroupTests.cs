using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;

class RadioGroupTests
{
    [Test]
    public void TestRoundTrip_PreserveRadioGroups()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PreserveRadioGroups.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(View), out var sourceCode);

        var factory = new ViewFactory();
        var rgOut = factory.Create(typeof(RadioGroup));

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, rgOut, designOut, "myRadioGroup"));

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(View));

        var tabOut = designOut.View.GetActualSubviews().OfType<RadioGroup>().Single();

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var rgIn = designBackIn.View.GetActualSubviews().OfType<RadioGroup>().Single();

        Assert.AreEqual(2,rgIn.RadioLabels.Length);

        Assert.AreEqual("Option 1",rgIn.RadioLabels[0].ToString());
        Assert.AreEqual("Option 2",rgIn.RadioLabels[1].ToString());
    }
}
