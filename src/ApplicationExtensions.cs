using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace TerminalGuiDesigner
{
    public static class ApplicationExtensions
    {
        public static View? FindDeepestView(View start, int x, int y)
        {
            var method = typeof(Application).GetMethod(nameof(FindDeepestView),
                BindingFlags.Static | BindingFlags.NonPublic,
                new[] { typeof(View), typeof(int), typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() });

            if (method == null)
                throw new Exception("Static method FindDeepestView not found on Application class");

            int resx = 0;
            int resy = 0;

            return (View?)method.Invoke(null, new object[] { start, x, y, resx, resy });
        }
    }
}
