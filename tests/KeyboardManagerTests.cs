using System.IO;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.UI;

namespace UnitTests;

[TestFixture]
[TestOf( typeof( KeyboardManager ) )]
[Category( "UI" )]
internal class KeyboardManagerTests : Tests
{
    [Test]
    public void Backspace_WithDateFieldSelected()
    {
        var v = ViewFactory.Create(typeof(DateField));
        var d = new Design(new SourceCodeFile(new FileInfo("ff.cs")), "ff", v);
        v.Data = d;

        var mgr = new KeyboardManager(new KeyMap());
        ClassicAssert.IsFalse(mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers())));
        
        Application.Top.Add(v);
        v.Bounds = new Rect(0, 0, 6, 1);
        v.Draw();
    }

    [Test]
    public void ButtonRename()
    {
        var v = (Button)ViewFactory.Create(typeof(Button));
        var d = new Design(new SourceCodeFile(new FileInfo("ff.cs")), "ff", v);
        v.Data = d;

        var mgr = new KeyboardManager(new KeyMap());
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));

        ClassicAssert.IsTrue(string.IsNullOrWhiteSpace(v.Text));

        mgr.HandleKey(v, new KeyEvent(Key.B, new KeyModifiers { Shift = true }));
        mgr.HandleKey(v, new KeyEvent((Key)'a', new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent((Key)'d', new KeyModifiers()));

        ClassicAssert.AreEqual("Bad", v.Text);

        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));
        mgr.HandleKey(v, new KeyEvent(Key.Backspace, new KeyModifiers()));

        ClassicAssert.AreEqual("B", v.Text);
    }
}