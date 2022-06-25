using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace tests;

internal class CopyPasteTests : Tests
{
    [Test]
    public void CannotCopyRoot()
    {
        var d = Get10By10View();
        
        var top = new Toplevel();
        top.Add(d.View);

        Assert.IsTrue(d.IsRoot);
        var copy = new CopyOperation(d,new MultiSelectionManager());

        Assert.IsTrue(copy.IsImpossible);
    }
}
