using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using NUnit.Framework;
using Terminal.Gui;
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
        ClassicAssert.AreEqual(rhs.Value, "Pos.Center()");
    }

    [Test]
    public void TestPropertyOfType_Size()
    {
        var d = new Design(new SourceCodeFile(nameof(this.TestPropertyOfType_Size) + ".cs"), "FFF", new ScrollView());
        var xProp = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals(nameof(View.X)));

        xProp.SetValue(Pos.Center());

        var rhs = (CodeSnippetExpression)xProp.GetRhs();

        // The code generated for a Property of Type Pos should be the function call
        ClassicAssert.AreEqual(rhs.Value, "Pos.Center()");
    }

    [Test]
    public void TestPropertyOfType_Attribute()
    {
        var d = new Design(new SourceCodeFile(nameof(this.TestPropertyOfType_Attribute) + ".cs"), "FFF", new GraphView());
        var colorProp = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals(nameof(GraphView.GraphColor)));

        colorProp.SetValue(null);

        var rhs = (CodeSnippetExpression)colorProp.GetRhs();
        ClassicAssert.AreEqual(rhs.Value, "null");

        colorProp.SetValue(new Attribute(Color.BrightMagenta, Color.Blue));

        rhs = (CodeSnippetExpression)colorProp.GetRhs();
        ClassicAssert.AreEqual(rhs.Value, "new Terminal.Gui.Attribute(Color.BrightMagenta,Color.Blue)");
    }

    [Test]
    public void TestPropertyOfType_PointF()
    {
        var d = new Design(new SourceCodeFile(nameof(this.TestPropertyOfType_PointF) + ".cs"), "FFF", new GraphView());
        var pointProp = d.GetDesignableProperties().Single(p => p.PropertyInfo.Name.Equals(nameof(GraphView.ScrollOffset)));

        pointProp.SetValue(new PointF(4.5f, 4.1f));

        var rhs = (CodeObjectCreateExpression)pointProp.GetRhs();

        // The code generated should be a new PointF
        ClassicAssert.AreEqual(rhs.Parameters.Count, 2);
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

        ClassicAssert.AreEqual(new Rune('F'), lv.LineRune);

        var code = ExpressionToCode(prop.GetRhs());

        ClassicAssert.AreEqual("new System.Text.Rune('F')", code);
    }

    [Test]
    public void TestChanging_LineViewOrientation()
    {
        var v = Get10By10View();
        var lv = (LineView)new ViewFactory().Create(typeof(LineView));
        var d = new Design(v.SourceCode, "lv", lv);

        v.View.Add(lv);
        lv.IsInitialized = true;

        ClassicAssert.AreEqual(Orientation.Horizontal, lv.Orientation);
        ClassicAssert.AreEqual(new Rune('─'), lv.LineRune);
        var prop = d.GetDesignableProperty(nameof(LineView.Orientation));

        ClassicAssert.IsNotNull(prop);
        prop?.SetValue(Orientation.Vertical);
        ClassicAssert.AreEqual(ConfigurationManager.Glyphs.VLine, lv.LineRune);

        // now try with a dim fill
        lv.Height = Dim.Fill();
        lv.Width = 1;

        prop?.SetValue(Orientation.Horizontal);
        ClassicAssert.AreEqual(Orientation.Horizontal, lv.Orientation);
        ClassicAssert.AreEqual(ConfigurationManager.Glyphs.HLine, lv.LineRune);
        ClassicAssert.AreEqual(Dim.Fill(), lv.Width);
        ClassicAssert.AreEqual(Dim.Sized(1), lv.Height);
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
