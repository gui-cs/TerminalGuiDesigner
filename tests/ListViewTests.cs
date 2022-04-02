
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using NUnit.Framework;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.FromCode;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests;
class ListViewTests
{
    [Test]
    public void TestRoundTrip_PreserveList()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestRoundTrip_PreserveList.cs");
        var designOut = viewToCode.GenerateNewView(file, "YourNamespace",typeof(Window), out var sourceCode);

        var factory = new ViewFactory();
        var lvOut = (ListView)factory.Create(typeof(ListView));


        Assert.AreEqual(3, lvOut.Source.Count);

        OperationManager.Instance.Do(new AddViewOperation(sourceCode, lvOut, designOut, "myList"));

        viewToCode.GenerateDesignerCs(designOut, sourceCode,typeof(Window));

        var listOut = designOut.View.GetActualSubviews().OfType<ListView>().Single();

        var codeToView = new CodeToView(sourceCode);
        var designBackIn = codeToView.CreateInstance();

        var listIn = designBackIn.View.GetActualSubviews().OfType<ListView>().Single();

        Assert.AreEqual(3, listIn.Source.Count);
    }

    [Test]
    public void TestIListSourceProperty_Rhs()
    {
        var viewToCode = new ViewToCode();

        var file = new FileInfo("TestIListSourceProperty_Rhs.cs");
        var lv = new ListView();
        var d = new Design(new SourceCodeFile(file),"lv",lv);
        var prop = d.GetDesignableProperties().Single(p=>p.PropertyInfo.Name.Equals("Source"));

        Assert.IsNull(lv.Source);

        prop.SetValue(new string[]{"hi there","my friend"});

        Assert.AreEqual(2,lv.Source.Count);
        
        var code = ExpressionToCode(prop.GetRhs());

        Assert.AreEqual("new Terminal.Gui.ListWrapper(new string[] {\n            \"hi there\",\n            \"my friend\"})",code);
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
    