using NUnit.Framework;
using System;
using Terminal.Gui;

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
        }
    }
}
