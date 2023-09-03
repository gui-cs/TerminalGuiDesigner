using NUnit.Framework;
using System.Linq;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests
{
    internal class BorderColorTests : Tests
    {
        [Test]
        public void TestRoundTrip_BorderColors_NeverSet()
        {
            var result = RoundTrip<Window, FrameView>((d, v) =>
            {
                // Our view should not have any border color to start with
                Assert.AreEqual(-1, (int)v.Border.BorderBrush);
                Assert.AreEqual(-1, (int)v.Border.Background);

            }, out _);

            // Since there were no changes we would expect them to stay the same
            // after reload
            Assert.AreEqual(-1, (int)result.Border.BorderBrush);
            Assert.AreEqual(-1, (int)result.Border.Background);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestCopyPasteContainer(bool alsoSelectSubElements)
        {
            RoundTrip<Window, FrameView>((d, v) =>
                {
                    new AddViewOperation(new Label(), d, "lbl1").Do();
                    new AddViewOperation(new Label(), d, "lbl2").Do();

                    Assert.AreEqual(2, v.GetActualSubviews().Count(), "Expected the FrameView to have 2 children (lbl1 and lbl2)");

                    Design[] toCopy;

                    if (alsoSelectSubElements)
                    {
                        var lbl1Design = (Design)d.View.GetActualSubviews().First().Data;
                        Assert.AreEqual("lbl1", lbl1Design.FieldName);

                        toCopy = new Design[] { d, lbl1Design };
                    }
                    else
                    {
                        toCopy = new[] { d };
                    }

                    // copy the FrameView
                    Assert.IsTrue(new CopyOperation(toCopy).Do());

                    var rootDesign = d.GetRootDesign();

                    // paste the FrameView
                    Assert.IsTrue(new PasteOperation(rootDesign).Do());

                    var rootSubviews = rootDesign.View.GetActualSubviews();

                    Assert.AreEqual(2, rootSubviews.Count, "Expected root to have 2 FrameView now");
                    Assert.IsTrue(rootSubviews.All(v => v is FrameView));

                    Assert.IsTrue(
                        rootSubviews.All(f => f.GetActualSubviews().Count() == 2),
                        "Expected both FrameView (copied and pasted) to have the full contents of 2 Labels");

                    // Since there were no changes we would expect them to stay the same
                    // after reload
                    foreach (var rsv in rootSubviews)
                    {
                        Assert.AreEqual(-1, (int)rsv.Border.BorderBrush);
                        Assert.AreEqual(-1, (int)rsv.Border.Background);
                        Assert.Null(GetFieldValue<Color?>(rsv.Border, "borderBrush"));
                        Assert.Null(GetFieldValue<Color?>(rsv.Border, "background"));
                    }
                }
                , out _);
        }

        private T GetFieldValue<T>(object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj)!;
        }
    }
}
