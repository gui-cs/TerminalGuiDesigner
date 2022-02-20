using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Terminal.Gui;

namespace TerminalGuiDesigner
{
    internal class CodeGenerator
    {
        public View GenerateNewWindow(FileInfo csFilePath)
        {
            if(csFilePath.Name.EndsWith(DeCompiler.ExpectedExtension))
            {
                throw new ArgumentException($@"{nameof(csFilePath)} should be a class file not the designer file e.g. c:\MyProj\MyWindow1.cs");
            }

            throw new NotImplementedException();
        }

        private string GenerateDesignerCs(View forView, FileInfo designerFile)
        {
            var samples = new CodeNamespace("Samples");
            samples.Imports.Add(new CodeNamespaceImport("System"));
            samples.Imports.Add(new CodeNamespaceImport("Terminal.Gui"));

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(samples);

            CodeTypeDeclaration class1 = new CodeTypeDeclaration("Class1");
            class1.IsPartial = true;

            samples.Types.Add(class1);

            CSharpCodeProvider provider = new CSharpCodeProvider();

            using (var sw = new StringWriter())
            {
                IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");

                // Generate source code using the code provider.
                provider.GenerateCodeFromCompileUnit(compileUnit, tw,
                    new CodeGeneratorOptions());

                tw.Close();


                return sw.ToString();
            }
        }
    }
}
