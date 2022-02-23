using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
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
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public View GenerateNewWindow(FileInfo csFilePath, string namespaceName)
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
            
            var className = Path.GetFileNameWithoutExtension(csFilePath.Name);
            var designerFile = new FileInfo(Path.Combine(csFilePath.Directory.FullName,className + CodeToView.ExpectedExtension));

            CodeTypeDeclaration class1 = new CodeTypeDeclaration(className);
            class1.IsPartial = true;
            class1.BaseTypes.Add(new CodeTypeReference("Window"));

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
            w.Add(new Label("Hello World"));

            GenerateDesignerCs(w, designerFile, namespaceName);
            return w;
        }

        private void GenerateDesignerCs(View forView, FileInfo designerFile, string namespaceName)
        {
            var ns = new CodeNamespace(namespaceName);
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

            var className = Path.GetFileNameWithoutExtension(designerFile.Name.Replace(CodeToView.ExpectedExtension,""));

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(ns);

            CodeTypeDeclaration class1 = new CodeTypeDeclaration(className);
            class1.IsPartial = true;

            var method = new CodeMemberMethod();
            method.Name = "InitializeComponent";

            // foreach subview
            var designLabel = new DesignLabel("myLabel",new Label("Test String"));
                        
            
            // Create a private field for it
            var field = new CodeMemberField();
            field.Name = "myLabel";
            field.Type = new CodeTypeReference(typeof(Label));

            // Construct it
            var constructLhs = new CodeFieldReferenceExpression();
            constructLhs.FieldName = "this.myLabel";
            var constructRhs = new CodeObjectCreateExpression(typeof(Label));
            var constructAssign = new CodeAssignStatement();
            constructAssign.Left = constructLhs;
            constructAssign.Right = constructRhs;
            method.Statements.Add(constructAssign);

            // TODO: Hydrate it
            var setTextLhs = new CodeFieldReferenceExpression();
            setTextLhs.FieldName = "this.myLabel.Text";
            var setTextRhs = new CodePrimitiveExpression();
            setTextRhs.Value = "Hello World";            
            var setTextAssign = new CodeAssignStatement();
            setTextAssign.Left = setTextLhs;
            setTextAssign.Right = setTextRhs;
            method.Statements.Add(setTextAssign);

            // Add it to the view 
            var callAdd = new CodeMethodInvokeExpression();
            callAdd.Method.TargetObject = new CodeThisReferenceExpression();
            callAdd.Method.MethodName = "Add";
            callAdd.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),"myLabel"));
            method.Statements.Add(callAdd);

            class1.Members.Add(method);
            class1.Members.Add(field);

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
    }
}
