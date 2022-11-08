using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using NUnit.Framework;
using Terminal.Gui;
using Terminal.Gui.Graphs;
using TerminalGuiDesigner;
using TerminalGuiDesigner.ToCode;
using Attribute = Terminal.Gui.Attribute;

namespace UnitTests;

internal class PropertyTests : Tests
{
    [Test]
    public void TestPropertyOfType_Pos()
    {
        var d = new Design(new SourceCodeFile(nameof(this.TestPropertyOfType_Pos) + ".cs"), "FFF", new Label());
        var xProp = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals(nameof(View.X)));

        xProp.SetValue(Pos.Center());

        var rhs = (CodeSnippetExpression)xProp.GetRhs();

        // The code generated for a Property of Type Pos should be the function call
        Assert.AreEqual(rhs.Value, "Pos.Center()");
    }

    [Test]
    public void TestPropertyOfType_Size()
    {
        var d = new Design(new SourceCodeFile(nameof(this.TestPropertyOfType_Size) + ".cs"), "FFF", new ScrollView());
        var xProp = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals(nameof(View.X)));

        xProp.SetValue(Pos.Center());

        var rhs = (CodeSnippetExpression)xProp.GetRhs();

        // The code generated for a Property of Type Pos should be the function call
        Assert.AreEqual(rhs.Value, "Pos.Center()");
    }

    [Test]
    public void TestPropertyOfType_Attribute()
    {
        var d = new Design(new SourceCodeFile(nameof(this.TestPropertyOfType_Attribute) + ".cs"), "FFF", new GraphView());
        var colorProp = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals(nameof(GraphView.GraphColor)));

        colorProp.SetValue(null);

        var rhs = (CodeSnippetExpression)colorProp.GetRhs();
        Assert.AreEqual(rhs.Value, "null");

        colorProp.SetValue(Attribute.Make(Color.BrightMagenta, Color.Blue));

        rhs = (CodeSnippetExpression)colorProp.GetRhs();
        Assert.AreEqual(rhs.Value, "Terminal.Gui.Attribute.Make(Color.BrightMagenta,Color.Blue)");
    }

    [Test]
    public void TestPropertyOfType_PointF()
    {
        var d = new Design(new SourceCodeFile(nameof(this.TestPropertyOfType_PointF) + ".cs"), "FFF", new GraphView());
        var pointProp = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals(nameof(GraphView.ScrollOffset)));

        pointProp.SetValue(new PointF(4.5f, 4.1f));

        var rhs = (CodeObjectCreateExpression)pointProp.GetRhs();

        // The code generated should be a new PointF
        Assert.AreEqual(rhs.Parameters.Count, 2);
    }

    [Test]
    public void TestPropertyOfType_Rune()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestPropertyOfType_Rune.cs");
        var lv = new LineView();
        var d = new Design(new SourceCodeFile(file), "lv", lv);
        var prop = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals("LineRune"));

        prop.SetValue('F');

        Assert.AreEqual(new Rune('F'), lv.LineRune);

        var code = ExpressionToCode(prop.GetRhs());

        Assert.AreEqual("'F'", code);
    }

    [Test]
    public void TestChanging_LineViewOrientation()
    {
        var v = this.Get10By10View();
        var lv = (LineView)new ViewFactory().Create(typeof(LineView));
        var d = new Design(v.SourceCode, "lv", lv);

        v.View.Add(lv);
        lv.IsInitialized = true;

        Assert.AreEqual(Orientation.Horizontal, lv.Orientation);
        Assert.AreEqual(Application.Driver.HRLine, lv.LineRune);
        var prop = d.GetDesignableProperty(nameof(LineView.Orientation));

        Assert.IsNotNull(prop);
        prop?.SetValue(Orientation.Vertical);
        Assert.AreEqual(Application.Driver.VLine, lv.LineRune);

        // now try with a dim fill
        lv.Height = Dim.Fill();
        lv.Width = 1;

        prop?.SetValue(Orientation.Horizontal);
        Assert.AreEqual(Orientation.Horizontal, lv.Orientation);
        Assert.AreEqual(Application.Driver.HRLine, lv.LineRune);
        Assert.AreEqual(Dim.Fill(), lv.Width);
        Assert.AreEqual(Dim.Sized(1), lv.Height);
    }

    public static string ExpressionToCode(CodeExpression expression)
    {
        CSharpCodeProvider provider = new ();

        using (var sw = new StringWriter())
        {
            IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");
            provider.GenerateCodeFromExpression(expression, tw, new CodeGeneratorOptions());
            tw.Close();

            return sw.GetStringBuilder().ToString();
        }
    }
}
