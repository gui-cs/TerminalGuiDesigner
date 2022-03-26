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
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Dialog), out var sourceCode);

        var factory = new ViewFactory();
        var tvOut = factory.Create(typeof(TabView));

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, tvOut, designOut, "myTabview"));

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Dialog));

        var tabOut = designOut.View.GetActualSubviews().OfType<TabView>().Single();

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tabIn = designBackIn.View.GetActualSubviews().OfType<TabView>().Single();

        Assert.AreEqual(2,tabIn.Tabs.Count());

        Assert.AreEqual("Tab1",tabIn.Tabs.ElementAt(0).Text);
        Assert.AreEqual("Tab2",tabIn.Tabs.ElementAt(1).Text);
    }


    [Test]
    public void TestAddingSubcontrolToTab()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAddingSubcontrolToTab.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Dialog), out var sourceCode);

        var factory = new ViewFactory();
        var tvOut = (TabView)factory.Create(typeof(TabView));

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, tvOut, designOut, "myTabview"));

        var label = factory.Create(typeof(Label));

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, label, (Design)tvOut.Data, "myLabel"));
        Assert.Contains(label, tvOut.SelectedTab.View.Subviews.ToArray(),"Expected currently selected tab to have the new label but it did not");

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Dialog));

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tabIn = designBackIn.View.GetActualSubviews().OfType<TabView>().Single();
        var tabInLabel = tabIn.SelectedTab.View.Subviews.Single();

        Assert.AreEqual(label.Text,tabInLabel.Text);
    }

    [Test]
    public void TestGetAllDesigns_TabView()
    {
        var tv = new TabView();

        var source = new SourceCodeFile(new FileInfo("yarg.cs"));

        var lbl1 = new Design(source,"lbl1",new Label("fff"));
        lbl1.View.Data = lbl1;

        var lbl2 = new Design(source,"lbl2",new Label("ddd"));
        lbl2.View.Data = lbl2;

        tv.AddTab(new TabView.Tab("Yay",lbl1.View),true);
        tv.AddTab(new TabView.Tab("Yay",lbl2.View),false);
    
        var tvDesign = new Design(source,"tv",tv);

        Assert.Contains(tvDesign,tvDesign.GetAllDesigns().ToArray());
        Assert.Contains(lbl1,tvDesign.GetAllDesigns().ToArray());
        Assert.Contains(lbl2,tvDesign.GetAllDesigns().ToArray());

        Assert.AreEqual(3,tvDesign.GetAllDesigns().Count(),$"Expected only 3 Designs but they were {string.Join(",",tvDesign.GetAllDesigns())}");
    }
}