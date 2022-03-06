using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using Terminal.Gui;

namespace TerminalGuiDesigner
{
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
        public Design GenerateNewWindow(FileInfo csFilePath, string namespaceName, out SourceCodeFile sourceFile)
        {
            if(csFilePath.Name.EndsWith(SourceCodeFile.ExpectedExtension))
            {
                throw new ArgumentException($@"{nameof(csFilePath)} should be a class file not the designer file e.g. c:\MyProj\MyWindow1.cs");
            }


            var className = Path.GetFileNameWithoutExtension(csFilePath.Name);
            sourceFile = new SourceCodeFile(csFilePath);

            string indent = "    ";

            var ns = new CodeNamespace(namespaceName);
            ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);

            CodeTypeDeclaration class1 = new CodeTypeDeclaration(className);
            class1.IsPartial = true;
            class1.BaseTypes.Add(new CodeTypeReference("Window")); //TODO: let user create things that aren't windows

            ns.Types.Add(class1);

            var constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Statements.Add(new CodeSnippetStatement($"{indent}{indent}{indent}InitializeComponent();"));

            class1.Members.Add(constructor);

            CSharpCodeProvider provider = new CSharpCodeProvider();

            using (var sw = new StringWriter())
            {
                IndentedTextWriter tw = new IndentedTextWriter(sw, indent);

                // Generate source code using the code provider.
                provider.GenerateCodeFromCompileUnit(compileUnit, tw,
                    new CodeGeneratorOptions());

                tw.Close();

                File.WriteAllText(sourceFile.CsFile.FullName, sw.ToString());
            }

            var w = new Window();
            var lbl = new Label("Hello World");
            lbl.Data = "label1"; // field name in the class
            w.Add(lbl);

            var design = new Design("root", w);
            design.CreateSubControlDesigns();

            GenerateDesignerCs(w, sourceFile);

            return design;
        }
        public void GenerateDesignerCs(View forView, SourceCodeFile file)
        {
            var rosylyn = new RoslynCodeToView(file);

            var ns = new CodeNamespace(rosylyn.Namespace);
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));


            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);

            CodeTypeDeclaration class1 = new CodeTypeDeclaration(rosylyn.ClassName);
            class1.IsPartial = true;

            var initMethod = new CodeMemberMethod();
            initMethod.Name = "InitializeComponent";

            AddSubViewsToDesignerCs(forView,class1, initMethod);
                                    
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

                File.WriteAllText(file.DesignerFile.FullName,sw.ToString());
            }
        }

        private void AddSubViewsToDesignerCs(View forView, CodeTypeDeclaration class1, CodeMemberMethod initMethod)
        {
            foreach (var sub in forView.Subviews)
            {
                // If the sub child has a Design (and is not an internal part of another control,
                // For example Contentview subview of Window
                if(sub.Data is Design d)
                {
                    // The user is designing this view so it needs to be persisted
                    d.ToCode(class1, initMethod);
                }

                // now recurse down the view hierarchy
                AddSubViewsToDesignerCs(sub, class1, initMethod);
            }
        }
    }
}
