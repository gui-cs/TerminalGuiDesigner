using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;

class TabViewTests
{
    [Test]
    public void TestRoundTrip_PreserveTabs()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PreserveTabs.cs");
        var designOut = viewToCode.GenerateNewWindow(file, "YourNamespace", out var sourceCode);

        var factory = new ViewFactory();
        var tvOut = factory.Create(typeof(TabView));

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, tvOut, designOut, "myTabview"));

        viewToCode.GenerateDesignerCs(designOut.View, sourceCode);

        var tabOut = designOut.View.GetActualSubviews().OfType<TabView>().Single();

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tabIn = designBackIn.View.GetActualSubviews().OfType<TabView>().Single();

        Assert.AreEqual(2,tabIn.Tabs.Count());

        Assert.AreEqual("Tab1",tabIn.Tabs.ElementAt(0).Text);
        Assert.AreEqual("Tab2",tabIn.Tabs.ElementAt(1).Text);
    }
}