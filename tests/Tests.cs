using NUnit.Framework;
using System;
using System.IO;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace tests
{
    public class Tests
    {
        static bool _init = false;

        [SetUp]
        public virtual void SetUp()
        {
            if (_init)
            {
                throw new InvalidOperationException("After did not run.");
            }

            Application.Init(new FakeDriver(), new FakeMainLoop(() => FakeConsole.ReadKey(true)));
            _init = true;
        }

        [TearDown]
        public virtual void TearDown()
        {
            Application.Shutdown();
            _init = false;

            SelectionManager.Instance.LockSelection = false;
            SelectionManager.Instance.Clear();
            ColorSchemeManager.Instance.Clear();
        }

        protected Design Get10By10View()
        {
            // start with blank slate
            OperationManager.Instance.ClearUndoRedo();

            var v = new View(new Rect(0, 0, 10, 10));
            var d = new Design(new SourceCodeFile(new FileInfo("TenByTen.cs")), Design.RootDesignName, v);
            v.Data = d;

            return d;
        }
    }
}
