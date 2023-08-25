using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Terminal.Gui;
using TerminalGuiDesigner.FromCode;

namespace TerminalGuiDesigner.ToCode;

/// <summary>
/// Converts a <see cref="View"/> in memory into code in a '.Designer.cs' class file.
/// </summary>
public class ViewToCode
{
    /// <summary>
    /// Returns the code that would be added to the MyWindow.cs file of a new window
    /// so that it is ready for use with the MyWindow.Designer.cs file (in which
    /// we will put all our design time gubbins).
    /// </summary>
    /// <param name="className">Class name to generate.</param>
    /// <param name="namespaceName">Namespace to declare in the code generated.</param>
    /// <returns>Code that would go in a <see cref="SourceCodeFile.CsFile"/> for a new <see cref="View"/>
    /// inheritor with <paramref name="className"/> including constructor and call to <see cref="CodeDomArgs.InitMethod"/>.</returns>
    public static string GetGenerateNewViewCode(string className, string namespaceName)
    {
        string indent = "    ";

        var ns = new CodeNamespace(namespaceName);
        ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

        CodeCompileUnit compileUnit = new CodeCompileUnit();
        compileUnit.Namespaces.Add(ns);

        AddCustomHeaderForViewFile(ns);

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
            provider.GenerateCodeFromCompileUnit(
                compileUnit,
                tw,
                new CodeGeneratorOptions());

            tw.Close();

            var code = sw.ToString();
            return TrimHeader(code);
        }
    }

    /// <summary>
    /// Creates a new class file and accompanying '.Designer.cs' file based on <paramref name="viewType"/>.
    /// </summary>
    /// <param name="csFilePath">Path to the MyClass.cs file (or MyClass.Designer.cs).</param>
    /// <param name="namespaceName">The namespace to declare in the .Designer.cs file generated.</param>
    /// <param name="viewType">Type of view to inherit from for users view (e.g. <see cref="Window"/>).</param>
    /// <returns><see cref="Design"/> whose <see cref="Design.SourceCode"/> points to the file generated.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="viewType"/> is null.</exception>
    public Design GenerateNewView(FileInfo csFilePath, string namespaceName, Type viewType)
    {
        if (viewType is null)
        {
            throw new ArgumentNullException(nameof(viewType));
        }

        var sourceFile = new SourceCodeFile(csFilePath);

        var className = Path.GetFileNameWithoutExtension(sourceFile.CsFile.Name);

        var csharpCode = GetGenerateNewViewCode(className, namespaceName);
        File.WriteAllText(sourceFile.CsFile.FullName, csharpCode);

        var prototype = (View)(Activator.CreateInstance(viewType) ?? throw new Exception($"Could not create instance of Type '{viewType}' ('Activator.CreateInstance' returned null)"));

        // Unlike Window and Dialog the default constructor on
        // View will be a size 0 view.  Make it big so it can be
        // edited
        if (viewType == typeof(View))
        {
            prototype.Width = Dim.Fill();
            prototype.Height = Dim.Fill();
        }

        // use the prototype to create a designer cs file
        var design = new Design(sourceFile, Design.RootDesignName, prototype);
        design.CreateSubControlDesigns();

        this.GenerateDesignerCs(design, viewType);

        /* Reload the designer cs file to create a new instance (which is returned).
         *  NOTE: prototype is not the same instance that is returned;
         */

        var decompiler = new CodeToView(sourceFile);
        return decompiler.CreateInstance();
    }

    /// <summary>
    /// Writes a .Designer.cs (<see cref="SourceCodeFile.DesignerFile"/>) to disk based on the
    /// current state of <paramref name="rootDesign"/>.
    /// </summary>
    /// <param name="rootDesign">The current state of the editor's root <see cref="Design"/>.
    /// A wrapper for an instance of the users root <see cref="View"/> inheritor (e.g. MyView:Window).</param>
    /// <param name="viewType">The <see cref="Type.BaseType"/> that the user's root view inherits from
    /// (e.g. `MyView : Window` inherits from Window).</param>
    public void GenerateDesignerCs(Design rootDesign, Type viewType)
    {
        var file = rootDesign.SourceCode;
        var rosylyn = new CodeToView(file);

        var ns = new CodeNamespace(rosylyn.Namespace);
        ns.Imports.Add(new CodeNamespaceImport("System"));
        ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

        this.AddCustomHeaderForDesignerCsFile(ns);

        CodeCompileUnit compileUnit = new CodeCompileUnit();
        compileUnit.Namespaces.Add(ns);

        CodeTypeDeclaration class1 = new CodeTypeDeclaration(rosylyn.ClassName);
        class1.IsPartial = true;
        class1.BaseTypes.Add(new CodeTypeReference(viewType));

        var initMethod = new CodeMemberMethod();
        initMethod.Name = SourceCodeFile.InitializeComponentMethodName;

        var args = new CodeDomArgs(class1, initMethod);

        this.AddColorSchemesToClass(args);

        // Add designable root properties to the InitializeComponent method
        foreach (var prop in rootDesign.GetDesignableProperties())
        {
            // We don't need to name the root view in code
            if (prop is NameProperty)
            {
                continue;
            }

            prop.ToCode(args);
        }

        this.AddSubViewsToDesignerCs(rootDesign.View, args);

        class1.Members.Add(initMethod);
        ns.Types.Add(class1);

        CSharpCodeProvider provider = new CSharpCodeProvider();

        using (var sw = new StringWriter())
        {
            IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");

            // Generate source code using the code provider.
            provider.GenerateCodeFromCompileUnit(
                compileUnit,
                tw,
                new CodeGeneratorOptions());

            tw.Close();

            File.WriteAllText(
                file.DesignerFile.FullName,
                TrimHeader(sw.ToString().Replace("Terminal.Gui.Color.-1", "(Color)(-1)")));
        }
    }

    /// <summary>
    /// Recursively adds all user designed sub-views of <paramref name="forView"/> to <see cref="SourceCodeFile.DesignerFile"/>.
    /// </summary>
    /// <param name="forView">Current <see cref="View"/> being written to .Designer.cs file whose children will be evaluated.</param>
    /// <param name="args">State of the .Designer.cs file.</param>
    /// <param name="parentViewExpression">Optional code expression that describes what the found sub-views should be added to
    /// when generating code.  Leave null to auto calculate e.g. <see cref="CodeThisReferenceExpression"/> for first level children.</param>
    public void AddSubViewsToDesignerCs(View forView, CodeDomArgs args, CodeExpression? parentViewExpression = null)
    {
        // order the controls top left to lower right so that tab order is good
        foreach (var sub in ViewExtensions.OrderViewsByScreenPosition(forView.Subviews))
        {
            // If the sub child has a Design (and is not an public part of another control,
            // For example ContentView sub-view of Window
            if (sub.Data is Design d)
            {
                if (args.OutputAlready.Contains(d))
                {
                    continue;
                }

                // The user is designing this view so it needs to be persisted
                var toCode = new DesignToCode(d);

                var parent = sub.SuperView?.GetNearestDesign();

                // if we did not get a specific parentViewExpression passed in then work out what to we are adding to
                // if our parent is the root then we are adding to 'this' otherwise reference it by parents FieldName
                parentViewExpression ??= parent == null || parent.IsRoot ? new CodeThisReferenceExpression() :
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), parent.FieldName);

                // Build the design code
                toCode.ToCode(args, parentViewExpression);

                // mark that we have now done this Design
                // so don't output it again even if there is some
                // weird loop
                args.OutputAlready.Add(d);

                // TabToCode handles children so don't handle them
                // here too otherwise we end up adding each view twice!
                if (sub is TabView)
                {
                    continue;
                }
            }

            // now recurse down the view hierarchy
            this.AddSubViewsToDesignerCs(sub, args);
        }
    }

    /// <summary>
    /// Removes the default header text generated by CodeDOM.
    /// </summary>
    /// <param name="code">Code generated by CodeDOM which should have header replaced.</param>
    /// <returns>Adjusted code that no longer has boilerplate CodeDOM disclaimer.</returns>
    private static string TrimHeader(string code)
    {
        var autoText = "// </auto-generated>";
        var idx = code.IndexOf(autoText);

        // Comment syntax has changed oh dear, best not trim it after all
        if (idx == -1)
        {
            return code;
        }

        return code.Substring(idx + autoText.Length);
    }

    /// <summary>
    /// <para>
    /// Adds TerminalGuiDesigner specific header comments to the CodDOM object
    /// that is being generated.  Header describes the version of the software used
    /// and what is/is not safe for the user to change.
    /// </para>
    /// <para>This method is for use in the <see cref="SourceCodeFile.CsFile"/>.</para>
    /// </summary>
    /// <param name="nSpace">CodeDOM object for the whole code file.</param>
    private static void AddCustomHeaderForViewFile(CodeNamespace nSpace)
    {
        var version = typeof(Design).Assembly.GetName()?.Version?.ToString()
                        ?? "unknown";

        // Namespace comments
        nSpace.Comments.Add(new
            CodeCommentStatement(" <auto-generated>"));
        nSpace.Comments.Add(new
            CodeCommentStatement("     This code was generated by:"));
        nSpace.Comments.Add(new
            CodeCommentStatement($"       TerminalGuiDesigner v{version}"));
        nSpace.Comments.Add(new CodeCommentStatement(
            "     You can make changes to this file and they will not be overwritten when saving."));
        nSpace.Comments.Add(new CodeCommentStatement(" </auto-generated>"));
        nSpace.Comments.Add(new CodeCommentStatement(
            "-----------------------------------" +
            "------------------------------------------"));
    }

    /// <summary>
    /// <para>
    /// Adds TerminalGuiDesigner specific header comments to the CodDOM object
    /// that is being generated.  Header describes the version of the software used
    /// and what is/is not safe for the user to change.
    /// </para>
    /// <para>This method is for use in the <see cref="SourceCodeFile.DesignerFile"/>.</para>
    /// </summary>
    /// <param name="nSpace">CodeDOM object for the whole code file.</param>
    private void AddCustomHeaderForDesignerCsFile(CodeNamespace nSpace)
    {
        var version = typeof(Design).Assembly.GetName()?.Version?.ToString()
                        ?? "unknown";

        // Namespace comments
        nSpace.Comments.Add(new
            CodeCommentStatement(" <auto-generated>"));
        nSpace.Comments.Add(new
            CodeCommentStatement("     This code was generated by:"));
        nSpace.Comments.Add(new
            CodeCommentStatement($"       TerminalGuiDesigner v{version}"));
        nSpace.Comments.Add(new CodeCommentStatement(
            "     Changes to this file may cause incorrect behavior and will be lost if"));
        nSpace.Comments.Add(new CodeCommentStatement(
        "     the code is regenerated."));
        nSpace.Comments.Add(new CodeCommentStatement(" </auto-generated>"));
        nSpace.Comments.Add(new CodeCommentStatement(
            "-----------------------------------" +
            "------------------------------------------"));
    }

    private void AddColorSchemesToClass(CodeDomArgs args)
    {
        foreach (var scheme in ColorSchemeManager.Instance.Schemes)
        {
            var toCode = new ColorSchemeToCode(scheme);
            toCode.ToCode(args);
        }
    }
}
