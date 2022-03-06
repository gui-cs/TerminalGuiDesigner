using System.Reflection;
using NLog;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public class CodeToView
{
    ILogger logger = LogManager.GetCurrentClassLogger();

    public SourceCodeFile SourceFile { get; }

    public CodeToView(FileInfo sourceFile)
    {
        SourceFile = new SourceCodeFile(sourceFile);

    }


    internal Design CreateInstance()
    {
        logger.Info($"About to compile {SourceFile.DesignerFile}");

        var rosyln = new RoslynCodeToView(SourceFile);
        var assembly = rosyln.CompileAssembly();

        var expectedClassName = rosyln.ClassName;

        var instances = assembly.GetTypes().Where(t=>t.Name.Equals(expectedClassName)).ToArray();

        if(instances.Length == 0)
        {
            throw new Exception($"Could not find a Type called {expectedClassName} in compiled assembly");
        }

        if(instances.Length > 1){

            throw new Exception($"Found {instances.Length} Types called {expectedClassName} in compiled assembly");
        }
        
        View view;

        try
        {
            view = Activator.CreateInstance(instances[0]) as View 
                ?? throw new Exception($"Activator.CreateInstance returned null or class in {SourceFile.DesignerFile} was not a View");
        }
        catch(Exception ex)
        {
            throw new Exception($"Could not create instance of {instances[0].FullName}",ex);
        }

        var toReturn = new Design(SourceFile,"root", view);
        toReturn.CreateSubControlDesigns();

        return toReturn;
    }
}
