using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;
using TerminalGuiDesigner.ToCode;
using TerminalGuiDesigner;

namespace UnitTests
{
    class VisiblePropertyTests : Tests
    {
        [Test]
        public void TestSettingVisible_False()
        {
            var result = RoundTrip<Window, Label>((d, v) =>
            {
                // In Designer

                var prop = d.GetDesignableProperties().Single(
                    p => p.PropertyInfo.Name.Equals(nameof(View.Visible))
                    );
                
                // Should start off visible
                ClassicAssert.AreEqual(true, prop.GetValue());
                ClassicAssert.True(v.Visible);

                prop.SetValue(false);

                // Prop should know it is not visible
                ClassicAssert.AreEqual(false, prop.GetValue());
                // But View in editor should remain visible so that it can be clicked on etc
                ClassicAssert.True(v.Visible);

            },out _);

            // After reloading Designer
            var d = (Design)result.Data;
            var prop = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals(nameof(View.Visible)));

            ClassicAssert.AreEqual(false, prop.GetValue());
            ClassicAssert.True(result.Visible);
        }
    }
}
