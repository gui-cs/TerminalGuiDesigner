using Microsoft.CSharp;
using NUnit.Framework;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Attribute;

namespace tests;

public class PropertyTests : Tests
{
    [Test]
    public void TestPropertyOfType_Pos()
    {
        var d = new Design(new SourceCodeFile(nameof(TestPropertyOfType_Pos) + ".cs"),"FFF",new Label());
        var xProp = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals(nameof(View.X)));

        xProp.SetValue(Pos.Center());

        var rhs = (CodeSnippetExpression)xProp.GetRhs();

        // The code generated for a Property of Type Pos should be the function call
        Assert.AreEqual(rhs.Value,"Pos.Center()");

    }

    [Test]
    public void TestPropertyOfType_Attribute()
    {
        var d = new Design(new SourceCodeFile(nameof(TestPropertyOfType_Attribute)+".cs"),"FFF",new GraphView());
        var colorProp = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals(nameof(GraphView.GraphColor)));

        colorProp.SetValue(null);

        var rhs = (CodeSnippetExpression)colorProp.GetRhs();
        Assert.AreEqual(rhs.Value,"null");

        colorProp.SetValue(Attribute.Make(Color.BrightMagenta,Color.Blue));

        rhs = (CodeSnippetExpression)colorProp.GetRhs();
        Assert.AreEqual(rhs.Value,"Terminal.Gui.Attribute.Make(Color.BrightMagenta,Color.Blue)");
    }
    [Test]
    public void TestPropertyOfType_PointF()
    {
        var d = new Design(new SourceCodeFile(nameof(TestPropertyOfType_PointF) + ".cs"), "FFF",new GraphView());
        var pointProp = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals(nameof(GraphView.ScrollOffset)));

        pointProp.SetValue(new PointF(4.5f,4.1f));

        var rhs = (CodeObjectCreateExpression)pointProp.GetRhs();

        // The code generated should be a new PointF
        Assert.AreEqual(rhs.Parameters.Count,2);

    }


    [Test]
    public void TestPropertyOfType_Rune()
    {

        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestPropertyOfType_Rune.cs");
        var lv = new LineView();
        var d = new Design(new SourceCodeFile(file),"lv",lv);
        var prop = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals("LineRune"));

        prop.SetValue('F');

        Assert.AreEqual(new Rune('F'),lv.LineRune);
            
        var code = ExpressionToCode(prop.GetRhs());

        Assert.AreEqual("'F'",code);
    }

    public static string ExpressionToCode(CodeExpression expression)
    {
        CSharpCodeProvider provider = new ();

        using (var sw = new StringWriter())
        {
            IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");
            provider.GenerateCodeFromExpression(expression,tw,new CodeGeneratorOptions());
            tw.Close();

            return sw.GetStringBuilder().ToString();
        }
    }
}
