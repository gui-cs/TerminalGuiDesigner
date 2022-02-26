using System.Reflection;
using NLog;
using Terminal.Gui;

namespace TerminalGuiDesigner;

public class CodeToView
{
    private readonly Assembly _assembly;
    ILogger logger = LogManager.GetCurrentClassLogger();

    public FileInfo SourceFile { get; }

    public const string ExpectedExtension = ".Designer.cs";

    public CodeToView(FileInfo sourceFile)
    {
        SourceFile = sourceFile;

        logger.Info($"Evaluating {sourceFile}...");

        ValidateSourceFile();

        // Find the csproj that the file belongs to
        var csproj = GetCsProjFile(sourceFile.Directory ?? throw new ArgumentNullException("File had no known Directory"));
        var name = Path.GetFileNameWithoutExtension(csproj.Name);
            
        if(csproj.Directory == null)
        {
            throw new Exception($"csproj file {csproj.FullName} had no known Directory");
        }

        // find the bin dll that the project builds
        var binary = 
            csproj.Directory.GetFiles(name+".dll",SearchOption.AllDirectories).FirstOrDefault()
            ?? csproj.Directory.GetFiles(name+".exe",SearchOption.AllDirectories).FirstOrDefault()
            ?? csproj.Directory.GetFiles(name,SearchOption.AllDirectories).FirstOrDefault();

                
        if(binary == null)
        {
            throw new Exception($"Found csproj file {csproj.FullName} but no binary in any subdirectories, perhaps the project has not been built yet?");
        }

        // Load that assembly
        logger.Info($"Found Assembly {binary.FullName} for source file {sourceFile}...");
        _assembly = Assembly.LoadFile(binary.FullName);
    }

    private void ValidateSourceFile()
    {
        if(!SourceFile.Exists)
        {
            throw new Exception($"File {SourceFile.FullName} did not exist");
        }

        if(!SourceFile.Name.EndsWith(ExpectedExtension))
        {
            throw new Exception($"Expected file to have suffix {ExpectedExtension} but it was {SourceFile.Name}");
        }
    }

    public CodeToView(FileInfo sourceFile,Assembly assembly)
    {
        _assembly = assembly;
        SourceFile = sourceFile;
            
        ValidateSourceFile();
    }

    private FileInfo GetCsProjFile(DirectoryInfo dir)
    {
        return  dir.GetFiles("*.csproj").FirstOrDefault()
        ?? GetCsProjFile(dir.Parent 
        ?? throw new Exception("Could not find csproj file in source files directory or any parent directory"));
    }

    internal Design<View> CreateInstance()
    {
        var expectedClassName = SourceFile.Name.Replace(ExpectedExtension,"");

        var instances = _assembly.GetTypes().Where(t=>t.Name.Equals(expectedClassName)).ToArray();

        if(instances.Length == 0)
        {
            throw new Exception($"Could not find a Type called {expectedClassName} in Assembly {_assembly.Location}");
        }

        if(instances.Length > 1){

            throw new Exception($"Found {instances.Length} Types called {expectedClassName} in Assembly {_assembly.Location}");
        }
        
        View view;

        try
        {
            view = Activator.CreateInstance(instances[0]) as View 
                ?? throw new Exception($"Activator.CreateInstance returned null or class in {SourceFile.Name} was not a View");
        }
        catch(Exception ex)
        {
            throw new Exception($"Could not create instance of {instances[0].FullName}",ex);
        }

        var toReturn = new Design<View>("root", view);
        toReturn.CreateSubControlDesigns();

        return toReturn;
    }
}
