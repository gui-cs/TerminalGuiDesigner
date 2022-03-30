using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Terminal.Gui;
using TerminalGuiDesigner.FromCode;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Converts a <see cref="View"/> in memory into code in a '.Designer.cs' class file
/// </summary>
public class ViewToCode
{
    /// <summary>
    /// Creates a new class file and accompanying '.Designer.cs' file based on
    /// <see cref="Window"/>
    /// </summary>
    /// <param name="csFilePath"></param>
    /// <param name="namespaceName"></param>
    /// <param name="designerFile">Designer.cs file that will be created along side the <paramref name="csFilePath"/></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public Design GenerateNewView(FileInfo csFilePath, string namespaceName,Type viewType, out SourceCodeFile sourceFile)
    {
        if (csFilePath.Name.EndsWith(SourceCodeFile.ExpectedExtension))
        {
            throw new ArgumentException($@"{nameof(csFilePath)} should be a class file not the designer file e.g. c:\MyProj\MyWindow1.cs");
        }

        var className = Path.GetFileNameWithoutExtension(csFilePath.Name);
        sourceFile = new SourceCodeFile(csFilePath);

        var csharpCode = GetGenerateNewViewCode(className, namespaceName);
        File.WriteAllText(sourceFile.CsFile.FullName, csharpCode);

        var view = (View)(Activator.CreateInstance(viewType) ?? throw new Exception($"Could not create instance of Type '{viewType}' ('Activator.CreateInstance' returned null)"));

        // Unlike Window and Dialog the default constructor on
        // View will be a size 0 view.  Make it big so it can be 
        // edited
        if(viewType == typeof(View))
        {
            view.Width = Dim.Fill();
            view.Height = Dim.Fill();
        }

        if(viewType == typeof(Toplevel))
        {
            view.ColorScheme = Colors.TopLevel;

            view.Add(new MenuBar{
                Data = "menuBar",
                ColorScheme = Colors.Base
            });

            view.Add(new Window{
                Data = "content",
                Y = 1, // space for menu
                Width = Dim.Fill(),
                Height = Dim.Fill(1),// space for status bar
            });

            view.Add(new StatusBar{
                Data = "statusBar"
            });
        }

        var design = new Design(sourceFile, Design.RootDesignName, view);
        design.CreateSubControlDesigns();

        GenerateDesignerCs(design, sourceFile,viewType);


        var decompiler = new CodeToView(sourceFile);
        return decompiler.CreateInstance();
    }

    /// <summary>
    /// Returns the code that would be added to the MyWindow.cs file of a new window
    /// so that it is ready for use with the MyWindow.Designer.cs file (in which
    /// we will put all our design time gubbins).
    /// </summary>
    /// <param name="className"></param>
    /// <param name="namespaceName"></param>
    /// <param name="viewType"></param>
    /// <returns></returns>
    public static string GetGenerateNewViewCode(string className, string namespaceName)
    {
        string indent = "    ";

        var ns = new CodeNamespace(namespaceName);
        ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

        CodeCompileUnit compileUnit = new CodeCompileUnit();
        compileUnit.Namespaces.Add(ns);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration(className);
        class1.IsPartial = true;

        ns.Types.Add(class1);

        var constructor = new CodeConstructor();
        constructor.Attributes = MemberAttributes.Public;
        constructor.Statements.Add(new CodeSnippetStatement($"{indent}{indent}{indent}{SourceCodeFile.InitializeComponentMethodName}();"));

        class1.Members.Add(constructor);

        CSharpCodeProvider provider = new CSharpCodeProvider();

        using (var sw = new StringWriter())
        {
            IndentedTextWriter tw = new IndentedTextWriter(sw, indent);

            // Generate source code using the code provider.
            provider.GenerateCodeFromCompileUnit(compileUnit, tw,
                new CodeGeneratorOptions());

            tw.Close();

            return sw.ToString();
        }
    }

    public void GenerateDesignerCs(Design rootDesign, SourceCodeFile file,Type viewType)
    {
        var rosylyn = new CodeToView(file);

        var ns = new CodeNamespace(rosylyn.Namespace);
        ns.Imports.Add(new CodeNamespaceImport("System"));
        ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));


        CodeCompileUnit compileUnit = new CodeCompileUnit();
        compileUnit.Namespaces.Add(ns);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration(rosylyn.ClassName);
        class1.IsPartial = true;
        class1.BaseTypes.Add(new CodeTypeReference(viewType));

        var initMethod = new CodeMemberMethod();
        initMethod.Name = SourceCodeFile.InitializeComponentMethodName;

        var args = new CodeDomArgs(class1, initMethod);

        // Add designable root properties to the InitializeComponent method
        foreach (var prop in rootDesign.GetDesignableProperties())
        {
            // We don't need to name the root view in code
            if(prop is NameProperty)
                continue;

            prop.ToCode(args);
        }

        AddSubViewsToDesignerCs(rootDesign.View, args);

        class1.Members.Add(initMethod);
        ns.Types.Add(class1);

        CSharpCodeProvider provider = new CSharpCodeProvider();

        using (var sw = new StringWriter())
        {
            IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");

            // Generate source code using the code provider.
            provider.GenerateCodeFromCompileUnit(compileUnit, tw,
                new CodeGeneratorOptions());

            tw.Close();

            File.WriteAllText(file.DesignerFile.FullName, sw.ToString());
        }
    }

    private void AddSubViewsToDesignerCs(View forView, CodeDomArgs args)
    {
        // TODO: we should detect RelativeTo etc here meaning one view depends
        // on anothers position and therefore the dependant view should be output
        // after

        // order the controls top left to lower right so that tab order is good
        foreach (var sub in forView.Subviews.OrderBy(v=>v.Frame.Y).ThenBy(v=>v.Frame.X))
        {
            // If the sub child has a Design (and is not an public part of another control,
            // For example Contentview subview of Window
            if (sub.Data is Design d)
            {
                // The user is designing this view so it needs to be persisted
                var toCode = new DesignToCode(d);

                var parent = sub.SuperView?.GetNearestDesign();
            
                // Build the design code 
                toCode.ToCode(args,
                    // if our parent is the root then the designed control should be assigned to 'this'
                    parent == null || parent.IsRoot ? new CodeThisReferenceExpression():
                    // the view we are adding to is not root but some deeper nested view so reference it by name
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),parent.FieldName)
                );

                // TabToCode handles children so don't handle them
                // here too otherwise we end up adding each view twice!
                if(sub is TabView)
                    continue;
            }

            // now recurse down the view hierarchy
            AddSubViewsToDesignerCs(sub, args);
        }
    }
}
