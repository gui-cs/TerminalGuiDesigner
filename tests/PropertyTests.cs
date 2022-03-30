using NUnit.Framework;
using System.CodeDom;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace tests;

public class PropertyTests
{
    [Test]
    public void TestPropertyOfType_Pos()
    {
        var d = new Design(null,"FFF",new Label());
        var xProp = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals(nameof(View.X)));

        xProp.SetValue(Pos.Center());

        var rhs = (CodeSnippetExpression)xProp.GetRhs();

        // The code generated for a Property of Type Pos should be the function call
        Assert.AreEqual(rhs.Value,"Pos.Center()");

    }

    [Test]
    public void TestPropertyOfType_Attribute()
    {
        
        var driver = new FakeDriver ();
        Application.Init (driver, new FakeMainLoop (() => FakeConsole.ReadKey (true)));
        driver.Init (() => { });

        try
        {
            var d = new Design(null,"FFF",new GraphView());
            var colorProp = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals(nameof(GraphView.GraphColor)));

            colorProp.SetValue(null);

            var rhs = (CodeSnippetExpression)colorProp.GetRhs();
            Assert.AreEqual(rhs.Value,"null");

            colorProp.SetValue(Attribute.Make(Color.BrightMagenta,Color.Blue));

            rhs = (CodeSnippetExpression)colorProp.GetRhs();
            Assert.AreEqual(rhs.Value,"Terminal.Gui.Attribute.Make(Color.BrightMagenta,Color.Blue)");
            
        }
        finally
        {
            driver.End();
            Application.Shutdown();
        }
    }
    [Test]
    public void TestPropertyOfType_PointF()
    {
        var d = new Design(null,"FFF",new GraphView());
        var pointProp = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals(nameof(GraphView.ScrollOffset)));

        pointProp.SetValue(new PointF(4.5f,4.1f));

        var rhs = (CodeObjectCreateExpression)pointProp.GetRhs();

        // The code generated should be a new PointF
        Assert.AreEqual(rhs.Parameters.Count,2);

    }
}
