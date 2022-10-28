using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

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

        /// <summary>
        /// Creates a new instance of a <see cref="View"/> of type <typeparamref name="T"/>.  Then calls the
        /// provided <paramref name="adjust"/> action before writting out and reading back the code.  Returns
        /// the read back in instance of your control so you can compare that it matches expectations
        /// </summary>
        /// <typeparam name="T">Type of subview to create (e.g. <see cref="Label"/>)</typeparam>
        /// <param name="adjust">Mutator for making pre save changes you want to conform can be read in properly</param>
        /// <param name="caller"></param>
        /// <returns>The read in object state after round trip (generate code file then read that code back in)</returns>
        protected T RoundTrip<T>(Action<Design,T> adjust, [CallerMemberName] string? caller = null) where T : View
        {
            var viewToCode = new ViewToCode();

            var file = new FileInfo(caller + ".cs");
            var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View), out var sourceCode);

            var factory = new ViewFactory();
            var viewOut = (T)factory.Create(typeof(T));

            OperationManager.Instance.Do(new AddViewOperation(sourceCode, viewOut, designOut, "myViewOut"));
            adjust((Design)viewOut.Data, viewOut);

            viewToCode.GenerateDesignerCs(designOut, sourceCode, typeof(View));

            var codeToView = new CodeToView(sourceCode);
            var designBackIn = codeToView.CreateInstance();

            return designBackIn.View.GetActualSubviews().OfType<T>().Single();
        }
    }
}
