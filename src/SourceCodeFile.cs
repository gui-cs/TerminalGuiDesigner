namespace TerminalGuiDesigner;

public class SourceCodeFile
{
    public FileInfo CsFile { get; }

    public FileInfo DesignerFile { get; }

    /// <summary>
    /// The name of the InitializeComponent() method.  i.e. "InitializeComponent"
    /// </summary>
    public const string InitializeComponentMethodName = "InitializeComponent";

    public const string ExpectedExtension = ".Designer.cs";

    /// <summary>
    /// Declares a new pair of files (e.g. MyClass.cs and MyClass.Designer.cs) which
    /// may or may not both exist yet
    /// </summary>
    /// <param name="file">Either source file of the pair (e.g. either MyClass.cs or MyClass.Designer.cs)</param>
    public SourceCodeFile(FileInfo file)
    {
        if (file.Name.EndsWith(ExpectedExtension))
        {
            this.CsFile = this.GetCsFile(file);
            this.DesignerFile = file;
        }
        else
        {
            this.CsFile = file;
            this.DesignerFile = this.GetDesignerFile(file);
        }
    }

    /// <summary>
    /// Declares a new pair of files (e.g. MyClass.cs and MyClass.Designer.cs) which
    /// may or may not both exist yet
    /// </summary>
    /// <param name="file">Either source file of the pair (e.g. either MyClass.cs or MyClass.Designer.cs)</param>
    public SourceCodeFile(string path)
        : this(new FileInfo(path))
    {
    }

    /// <summary>
    /// Returns the .Designer.cs file for the given class file.
    /// Returns a reference even if that file does not exist
    /// </summary>
    /// <returns></returns>
    private FileInfo GetDesignerFile(FileInfo csFile)
    {
        const string expectedCsExtension = ".cs";

        if (!csFile.Name.EndsWith(expectedCsExtension))
        {
            throw new ArgumentException($"Expected file {csFile.FullName} to have {expectedCsExtension} extension");
        }

        string name = csFile.FullName;
        var designerfile = name.Substring(0, name.Length - expectedCsExtension.Length) + ExpectedExtension;

        return new FileInfo(designerfile);
    }

    /// <summary>
    /// Returns the class file for a given .Designer.cs file
    /// </summary>
    /// <param name="designerFile"></param>
    /// <returns></returns>
    public FileInfo GetCsFile(FileInfo designerFile)
    {
        if (!designerFile.Name.EndsWith(ExpectedExtension))
        {
            throw new ArgumentException($"Expected {designerFile} to end with {ExpectedExtension}");
        }

        // chop off the .Designer.cs bit
        var filename = designerFile.FullName;
        filename = filename.Substring(0, filename.Length - ExpectedExtension.Length);
        filename += ".cs";

        return new FileInfo(filename);
    }
}
