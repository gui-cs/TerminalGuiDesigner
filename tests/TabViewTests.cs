using System.IO;
using System.Linq;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.Operations.TabOperations;
using TerminalGuiDesigner.ToCode;

namespace UnitTests;

class TabViewTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveTabs()
    {
        TabView tabIn = this.RoundTrip<Dialog, TabView>(
            (d, t) =>
            Assert.IsNotEmpty(t.Tabs, "Expected default TabView created by ViewFactory to have some placeholder Tabs"),
            out TabView tabOut);

        Assert.AreEqual(2, tabIn.Tabs.Count());

        Assert.AreEqual("Tab1", tabIn.Tabs.ElementAt(0).Text);
        Assert.AreEqual("Tab2", tabIn.Tabs.ElementAt(1).Text);
    }

    /// <summary>
    /// Creates a Dialog with a <see cref="TabView"/> in it.  Returns the <see cref="Design"/>
    /// </summary>
    /// <returns></returns>
    private Design GetTabView()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestGetTabView.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var factory = new ViewFactory();
        var tvOut = factory.Create(typeof(TabView));

        OperationManager.Instance.Do(new AddViewOperation(tvOut, designOut, "myTabview"));

        return (Design)tvOut.Data;
    }

    [Test]
    public void TestChangeTabViewOrder_MoveTabLeft()
    {
        var d = this.GetTabView();
        var tv = (TabView)d.View;

        Assert.AreEqual("Tab1", tv.Tabs.ElementAt(0).Text);
        Assert.AreEqual("Tab2", tv.Tabs.ElementAt(1).Text);

        // Select Tab1
        tv.SelectedTab = tv.Tabs.First();

        // try to move tab 1 left
        var cmd = new MoveTabOperation(d, tv.SelectedTab, -1);
        Assert.IsFalse(cmd.Do(), "Expected not to be able to move tab left because selected is the first");

        // Select Tab2
        tv.SelectedTab = tv.Tabs.Last();

        Assert.AreEqual(tv.SelectedTab.Text, "Tab2", "Tab2 should be selected before operation is applied");

        // try to move tab 2 left
        cmd = new MoveTabOperation(d, tv.SelectedTab, -1);
        Assert.IsFalse(cmd.IsImpossible);
        Assert.IsTrue(cmd.Do());

        Assert.AreEqual(tv.SelectedTab.Text, "Tab2", "Tab2 should still be selected after operation is applied");

        // tabs should now be in reverse order
        Assert.AreEqual("Tab2", tv.Tabs.ElementAt(0).Text);
        Assert.AreEqual("Tab1", tv.Tabs.ElementAt(1).Text);

        cmd.Undo();

        // undoing command should revert to original tab order
        Assert.AreEqual("Tab1", tv.Tabs.ElementAt(0).Text);
        Assert.AreEqual("Tab2", tv.Tabs.ElementAt(1).Text);
    }

    [Test]
    public void TestRemoveTabOperation()
    {
        var d = this.GetTabView();
        var tv = (TabView)d.View;

        Assert.AreEqual(2, tv.Tabs.Count);
        Assert.AreEqual("Tab1", tv.Tabs.ElementAt(0).Text);
        Assert.AreEqual("Tab2", tv.Tabs.ElementAt(1).Text);

        // Select Tab1
        tv.SelectedTab = tv.Tabs.First();

        // try to remove the first tab
        Assert.IsTrue(OperationManager.Instance.Do(new RemoveTabOperation(d, tv.SelectedTab)));

        Assert.AreEqual(1, tv.Tabs.Count);
        Assert.AreEqual("Tab2", tv.Tabs.ElementAt(0).Text);

        // remove the last tab (tab2)
        Assert.IsTrue(OperationManager.Instance.Do(new RemoveTabOperation(d, tv.SelectedTab)));
        Assert.IsEmpty(tv.Tabs);

        OperationManager.Instance.Undo();

        Assert.AreEqual(1, tv.Tabs.Count);
        Assert.AreEqual("Tab2", tv.Tabs.ElementAt(0).Text);

        OperationManager.Instance.Redo();
        Assert.IsEmpty(tv.Tabs);

        // undo removing both
        OperationManager.Instance.Undo();
        OperationManager.Instance.Undo();

        Assert.AreEqual(2, tv.Tabs.Count);
        Assert.AreEqual("Tab1", tv.Tabs.ElementAt(0).Text.ToString());
        Assert.AreEqual("Tab2", tv.Tabs.ElementAt(1).Text.ToString());
    }

    [Test]
    public void TestRoundTrip_DuplicateTabNames()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_DuplicateTabNames.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var factory = new ViewFactory();
        var tvOut = (TabView)factory.Create(typeof(TabView));

        OperationManager.Instance.Do(new AddViewOperation(tvOut, designOut, "myTabview"));

        // Give both tabs the same name
        tvOut.Tabs.ElementAt(0).Text = "MyTab";
        tvOut.Tabs.ElementAt(1).Text = "MyTab";

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var tabOut = designOut.View.GetActualSubviews().OfType<TabView>().Single();

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tabIn = designBackIn.View.GetActualSubviews().OfType<TabView>().Single();

        Assert.AreEqual(2, tabIn.Tabs.Count());

        Assert.AreEqual("MyTab", tabIn.Tabs.ElementAt(0).Text.ToString());
        Assert.AreEqual("MyTab", tabIn.Tabs.ElementAt(1).Text.ToString());
    }

    [Test]
    public void TestAddingSubcontrolToTab()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestAddingSubcontrolToTab.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(Dialog));

        var factory = new ViewFactory();
        var tvOut = (TabView)factory.Create(typeof(TabView));

        OperationManager.Instance.Do(new AddViewOperation(tvOut, designOut, "myTabview"));

        var label = factory.Create(typeof(Label));

        OperationManager.Instance.Do(new AddViewOperation(label, (Design)tvOut.Data, "myLabel"));
        Assert.Contains(label, tvOut.SelectedTab.View.Subviews.ToArray(), "Expected currently selected tab to have the new label but it did not");

        viewToCode.GenerateDesignerCs(designOut, typeof(Dialog));

        var codeToView = new CodeToView(designOut.SourceCode);
        var designBackIn = codeToView.CreateInstance();

        var tabIn = designBackIn.View.GetActualSubviews().OfType<TabView>().Single();
        var tabInLabel = tabIn.SelectedTab.View.Subviews.Single();

        Assert.AreEqual(label.Text, tabInLabel.Text);
    }

    [Test]
    public void TestGetAllDesigns_TabView()
    {
        var tv = new TabView();

        var source = new SourceCodeFile(new FileInfo("yarg.cs"));

        var lbl1 = new Design(source, "lbl1", new Label("fff"));
        lbl1.View.Data = lbl1;

        var lbl2 = new Design(source, "lbl2", new Label("ddd"));
        lbl2.View.Data = lbl2;

        tv.AddTab(new Tab("Yay", lbl1.View), true);
        tv.AddTab(new Tab("Yay", lbl2.View), false);

        var tvDesign = new Design(source, "tv", tv);

        Assert.Contains(tvDesign, tvDesign.GetAllDesigns().ToArray());
        Assert.Contains(lbl1, tvDesign.GetAllDesigns().ToArray());
        Assert.Contains(lbl2, tvDesign.GetAllDesigns().ToArray());

        Assert.AreEqual(3, tvDesign.GetAllDesigns().Count(), $"Expected only 3 Designs but they were {string.Join(",", tvDesign.GetAllDesigns())}");
    }

    [Test]
    public void TabView_IsBorderless_DependsOnShowBorder()
    {
        var inst = new TabView();

        Assert.IsTrue(inst.Style.ShowBorder);
        Assert.False(inst.IsBorderlessContainerView());

        inst.Style.ShowBorder = false;

        Assert.True(inst.IsBorderlessContainerView());
    }

    [Test]
    public void TabView_IsBorderless_DependsOnTabsOnBottom()
    {
        var inst = new TabView();

        Assert.IsFalse(inst.Style.TabsOnBottom);
        Assert.False(inst.IsBorderlessContainerView());

        inst.Style.TabsOnBottom = true;

        Assert.True(inst.IsBorderlessContainerView());
    }
}