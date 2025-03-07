using System.Xml.Linq;
using Terminal.Gui;

namespace UnitTests;

public class SuppressedPropertyTests
{
    [Test]
    public void TestCanFocusCorrect()
    {
        var lbl = ViewFactory.Create<Label>();
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")),"myLabel", lbl);
        
        Assert.That(lbl.CanFocus,Is.True);
        var p = d.GetDesignableProperty(nameof(View.CanFocus));
        Assert.That(p,Is.InstanceOf<SuppressedProperty>());
        Assert.That(p.GetValue(),Is.False);
    }

    [Test]
    public void TestCanFocusCorrect_AsChild()
    {
        var w = ViewFactory.Create<Window>();
        var lbl = ViewFactory.Create<Label>();
        lbl.Data = "mylbl";
        var d = new Design(new SourceCodeFile(new FileInfo("yarg.cs")), "mywin", w);
        w.Add(lbl);
        
        d.CreateSubControlDesigns();

        Assert.That(lbl.Data,Is.InstanceOf<Design>());

        d = (Design)lbl.Data;

        Assert.That(lbl.CanFocus, Is.True);
        var p = d.GetDesignableProperty(nameof(View.CanFocus));
        Assert.That(p, Is.InstanceOf<SuppressedProperty>());
        Assert.That(p.GetValue(), Is.False);
    }
}
