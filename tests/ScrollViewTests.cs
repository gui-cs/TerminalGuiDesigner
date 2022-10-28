using NUnit.Framework;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace tests;

class ScrollViewTests : Tests
{
    [Test]
    public void TestRoundTrip_PreserveContentSize()
    {
        var scrollViewIn = RoundTrip<ScrollView>((d,s) =>
                s.ContentSize = new Size(10, 5)
                );

        Assert.AreEqual(10, scrollViewIn.ContentSize.Width);
        Assert.AreEqual(5, scrollViewIn.ContentSize.Height);
    }
    [Test]
    public void TestRoundTrip_PreserveContentViews()
    {
        var scrollViewIn = RoundTrip<ScrollView>((d, s) =>
                {
                    var op = new AddViewOperation(d.SourceCode, new Label("blarggg"), d, "myLbl");
                    op.Do();
                });

        var child = scrollViewIn.GetActualSubviews().Single();
        Assert.IsInstanceOf<Label>(child);
        Assert.IsInstanceOf<Design>(child.Data);

        var lblIn = (Design)child.Data;
        Assert.AreEqual("myLbl", lblIn.FieldName);
        Assert.AreEqual("blarggg", lblIn.View.Text.ToString());
    }
}
