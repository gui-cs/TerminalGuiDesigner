using NUnit.Framework;
using System.Linq;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests
{
    [TestFixture]
    internal class BorderColorTests : Tests
    {
        [Test]
        public void TestRoundTrip_BorderColors_NeverSet()
        {
            var result = RoundTrip<Window, FrameView>((d, v) =>
            {
                // Our view should not have any border color to start with
                Assert.Multiple( ( ) =>
                {
                    Assert.That((int)v.Border.BorderBrush, Is.EqualTo( -1 ));
                    Assert.That((int)v.Border.Background, Is.EqualTo( -1 ));
                } );

            }, out _);

            // Since there were no changes we would expect them to stay the same
            // after reload
            Assert.Multiple( ( ) =>
            {
                Assert.That((int)result.Border.BorderBrush, Is.EqualTo( -1 ));
                Assert.That((int)result.Border.Background, Is.EqualTo( -1 ));
            } );
        }

        [Test]
        public void TestCopyPasteContainer([Values]bool alsoSelectSubElements)
        {
            RoundTrip<Window, FrameView>((d, v) =>
                {
                    new AddViewOperation(new Label(), d, "lbl1").Do();
                    new AddViewOperation(new Label(), d, "lbl2").Do();

                    Assert.That(v.GetActualSubviews().Count, Is.EqualTo(2),"Expected the FrameView to have 2 children (lbl1 and lbl2)");

                    Design[] toCopy;

                    if (alsoSelectSubElements)
                    {
                        var lbl1Design = (Design)d.View.GetActualSubviews().First().Data;
                        Assert.That(lbl1Design.FieldName, Is.EqualTo("lbl1"));
                        toCopy = new Design[] { d, lbl1Design };
                    }
                    else
                    {
                        toCopy = new[] { d };
                    }

                    // copy the FrameView
                    Assert.That(new CopyOperation(toCopy).Do(), Is.True);

                    var rootDesign = d.GetRootDesign();

                    // paste the FrameView
                    Assert.That(new PasteOperation(rootDesign).Do(), Is.True);
                    
                    var rootSubviews = rootDesign.View.GetActualSubviews();

                    Assert.Multiple( ( ) =>
                    {
                        Assert.That(rootSubviews.Count, Is.EqualTo(2), "Expected root to have 2 FrameView now");
                        Assert.That(rootSubviews, Is.All.TypeOf<FrameView>());

                        Assert.That( rootSubviews.Select(f=>f.GetActualSubviews(  )),
                                     Has.All.Count.EqualTo( 2 ),
                                     "Expected both FrameView (copied and pasted) to have the full contents of 2 Labels");

                        // Since there were no changes we would expect them to stay the same
                        // after reload
                        Assert.That(rootSubviews.Select(rsv=> rsv.Border.BorderBrush).Cast<int>(  ), Has.All.EqualTo(-1));
                        Assert.That(rootSubviews.Select(rsv=> rsv.Border.Background).Cast<int>(  ), Has.All.EqualTo(-1));
                        Assert.That(rootSubviews.Select(rsv=> GetFieldValue<Color?>( rsv.Border, "borderBrush" )), Has.All.Null);
                        Assert.That(rootSubviews.Select(rsv=> GetFieldValue<Color?>( rsv.Border, "background" )), Has.All.Null);
                    } );
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
