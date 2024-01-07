using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalGuiDesigner.UI;
using YamlDotNet.Core.Tokens;

namespace UnitTests.Investigations
{
    abstract class Investigation
    {
        protected abstract string Cs { get; }
        protected abstract string DesignerCs { get; }
        public SourceCodeFile SourceCode { get; private set; }

        protected abstract void MakeAssertions(Design rootDesign);


        [Test]
        public void PerformTest()
        {
            var designerFile = new FileInfo(
                Path.Combine(
                TestContext.CurrentContext.WorkDirectory, this.GetType().Name + ".Designer.cs")
                );

            File.WriteAllText(designerFile.FullName, DesignerCs);

            var csFile = new FileInfo(
                Path.Combine(
                TestContext.CurrentContext.WorkDirectory, this.GetType().Name + ".cs")
                );

            File.WriteAllText(csFile.FullName, DesignerCs);

            var decompiler = new CodeToView(SourceCode = new SourceCodeFile(designerFile));
            var d = decompiler.CreateInstance();
            MakeAssertions(d);
        }
    }
}
