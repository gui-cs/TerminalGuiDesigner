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
        public Design GenerateNewWindow(FileInfo csFilePath, string namespaceName, out FileInfo designerFile)
        {
            if(csFilePath.Name.EndsWith(CodeToView.ExpectedExtension))
            {
                throw new ArgumentException($@"{nameof(csFilePath)} should be a class file not the designer file e.g. c:\MyProj\MyWindow1.cs");
            }
            string indent = "    ";

            var ns = new CodeNamespace(namespaceName);
            ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);
            
            designerFile = GetDesignerFile(csFilePath, out string className);

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

                File.WriteAllText(csFilePath.FullName, sw.ToString());
            }

            var w = new Window();
            var lbl = new Label("Hello World");
            lbl.Data = "label1"; // field name in the class
            w.Add(lbl);

            var design = new Design("root", w);
            design.CreateSubControlDesigns();

            GenerateDesignerCs(w, designerFile);

            return design;
        }
        /// <summary>
        /// Returns the .Designer.cs file for the given class file.
        /// Returns a reference even if that file does not exist
        /// </summary>
        /// <param name="csFile"></param>
        /// <returns></returns>
        public FileInfo GetDesignerFile(FileInfo csFile, out string className)
        {
            className = Path.GetFileNameWithoutExtension(csFile.Name);
            return new FileInfo(Path.Combine(csFile.Directory.FullName, className + CodeToView.ExpectedExtension));
        }

        /// <summary>
        /// Returns the class file for a given .Designer.cs file
        /// </summary>
        /// <param name="designerFile"></param>
        /// <returns></returns>
        public FileInfo GetCsFile(FileInfo designerFile)
        {
            if (!designerFile.Name.EndsWith(CodeToView.ExpectedExtension))
                throw new ArgumentException($"Expected {designerFile} to end with {CodeToView.ExpectedExtension}");

            // chop off the .Designer.cs bit
            var filename = designerFile.FullName;
            filename = filename.Substring(0, filename.Length - CodeToView.ExpectedExtension.Length);
            filename += ".cs";

            return new FileInfo(filename);
            
        }
        public void GenerateDesignerCs(View forView, FileInfo designerFile)
        {
            var ns = new CodeNamespace(GetNamespace(GetCsFile(designerFile)));
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

            var className = Path.GetFileNameWithoutExtension(designerFile.Name.Replace(CodeToView.ExpectedExtension,""));

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);

            CodeTypeDeclaration class1 = new CodeTypeDeclaration(className);
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

                File.WriteAllText(designerFile.FullName,sw.ToString());
            }
        }

        private string GetNamespace(FileInfo csFile)
        {
            var csFileCode = File.ReadAllText(csFile.FullName);
            var regexFindNamespace = new Regex(@"namespace ([\w\.]+)");

            var match = regexFindNamespace.Matches(csFileCode).FirstOrDefault();
            if(match == null)
            {
                throw new Exception($"Could not find namespace directive in {csFile}");
            }

            return match.Groups[1].Value;
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
