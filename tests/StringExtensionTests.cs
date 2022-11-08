﻿using NUnit.Framework;
using TerminalGuiDesigner;

namespace UnitTests
{
    internal class StringExtensionTests
    {
        [TestCase("bob", "bob", new[] {"fish"})]
        [TestCase("fish", "fish2", new[] { "fish" })]
        [TestCase("fish2", "fish3", new[] { "fish1","fish2" })]
        [TestCase(null, "blank", new[] { "fish1", "fish2" })]
        [TestCase("  ", "blank", new[] { "fish1", "fish2" })]
        [TestCase("bob", "bob3", new[] { "bob", "bob2" })]
        [TestCase("Fish1", "Fish1", new[] { "fish1"})] // default behavior is case sensitive
        public void TestMakeUnique_DefaultComparer(string stringIn, string expectedOut, string[] whenCollection)
        {
            Assert.AreEqual(expectedOut, stringIn.MakeUnique(whenCollection));
        }

        [TestCase("bob", "bob", new[] { "fish" })]
        [TestCase("Fish", "Fish2", new[] { "fish" })]
        [TestCase("fIsh2", "fIsh3", new[] { "fish1", "fish2" })]
        [TestCase("fish2", "fish3", new[] { "fiSH1", "fiSH2" })]
        public void TestMakeUnique_IgnoreCaps(string stringIn, string expectedOut, string[] whenCollection)
        {
            Assert.AreEqual(expectedOut, stringIn.MakeUnique(whenCollection,System.StringComparer.InvariantCultureIgnoreCase));
        }
    }
}
