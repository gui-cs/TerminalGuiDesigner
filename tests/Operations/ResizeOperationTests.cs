using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace UnitTests.Operations;

internal class ResizeOperationTests : Tests
{
    [TestCase(true)]
    [TestCase(false)]
    public void TestResizeWhenNotAtOrigin(bool withMouse)
    {
        RoundTrip<Dialog, View>((d, v) =>
        {
            var root = d.GetRootDesign();
            root.View.Width = Dim.Fill();
            root.View.Height = Dim.Fill();

            var screen = root.View.ContentToScreen(new Point(0, 0));
            ClassicAssert.AreEqual(1, screen.X, "Expected root view of Dialog to have border 1 so client area starting at screen coordinates 1,1");
            ClassicAssert.AreEqual(1, screen.Y);


            // within Dialog there is a View
            v.X = 3;
            v.Y = 5;
            v.Width = 10;
            v.Height = 10;

            // within the View there is a TabView
            var tab = new TabView
            {
                X = 2,
                Y = 1,
                Width = 5,
                Height = 5,
            };
            new AddViewOperation(tab, d, "mytabview").Do();
            var tabDesign = (Design)tab.Data;

            // Diagram of client area of View (v)
            //(0,0 client - 4,6 screen)
            /*..............
             *..xxxxx....... (0,0 client is 6,7 screen)
             *..xxxxx.......
             *..xxxxx.......
             *..xxxxx.......
             *..xxxxx.......
             *....(4,4 client - 10,11 screen)
             * 
             * */
            // Double check the above figures
            screen = v.ContentToScreen(new Point(0, 0));
            ClassicAssert.AreEqual(4, screen.X);
            ClassicAssert.AreEqual(6, screen.Y);
            screen = tab.ContentToScreen(new Point(0,0));
            ClassicAssert.AreEqual(6, screen.X);
            ClassicAssert.AreEqual(7, screen.Y);
            screen = tab.ContentToScreen(new Point(4, 4));
            ClassicAssert.AreEqual(10, screen.X);
            ClassicAssert.AreEqual(11, screen.Y);

            Application.Begin((Dialog)root.View);
            root.View.LayoutSubviews();

            if (!withMouse)
            {
                // we are drag resizing the TabView
                // Tab view is at 2,1 with width 5 and height 5 so
                // its lower right is 6,5.  Drag resize to 7,7 (+1,+2)
                var op = new ResizeOperation(tabDesign, 7, 7);
                op.Do();
            }
            else
            {
                var hit = root.View.HitTest(new MouseEvent { X = 10, Y = 11 },out _, out var isLowerRight);
                ClassicAssert.AreSame(tab, hit, "Expected above diagram which already passed asserts to work for HitTest too given the above screen coordinates");
                ClassicAssert.IsTrue(isLowerRight);

                MouseDrag(root, 10, 11, 11, 13);
            }

            ClassicAssert.AreEqual((Dim)6, tab.Width); // (5+1)
            ClassicAssert.AreEqual((Dim)7, tab.Height); // (5+2)
        }, out _);
    }
}
