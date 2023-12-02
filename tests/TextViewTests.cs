using NStack;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests;

internal class TextViewTests : Tests
{
    [Test]
    public void TestSettingToNull()
    {
        var tv = new TextView();

        var d = new Design(new SourceCodeFile("Blah.cs"), "mytv", tv);
        tv.Data = d;
        tv.Text = "fff";

        var op = new SetPropertyOperation(
            d,
            d.GetDesignableProperty("Text") ?? throw new System.Exception("Did not find expected designable property"),
            tv.Text, null);

        op.Do();

        ClassicAssert.IsTrue(ustring.IsNullOrEmpty(tv.Text));
    }
}
