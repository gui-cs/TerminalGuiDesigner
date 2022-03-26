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
        var d = new Design(null,"FFF",new GraphView());
        var colorProp = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals(nameof(GraphView.GraphColor)));

        colorProp.SetValue(null);

        var rhs = (CodeSnippetExpression)colorProp.GetRhs();
        Assert.AreEqual(rhs.Value,"null");

        // TODO: Set it to green or something and check GetRhs

    }

}
