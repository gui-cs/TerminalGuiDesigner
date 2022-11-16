namespace TerminalGuiDesigner;

/// <summary>
/// Describes the pair of files (.cs and .Designer.cs) that makes up the source code
/// to a root <see cref="Design"/> that is can be openned in the editor.
/// </summary>
public class SourceCodeFile
{
    /// <summary>
    /// The name of the InitializeComponent() method.  i.e. "InitializeComponent".
    /// </summary>
    public const string InitializeComponentMethodName = "InitializeComponent";

    /// <summary>
    /// The .Designer.cs extension on files.
    /// </summary>
    public const string ExpectedExtension = ".Designer.cs";

    /// <summary>
    /// Gets the .cs file (e.g. MyView.cs).
    /// </summary>
    public FileInfo CsFile { get; }

    /// <summary>
    /// Gets the .Designer.cs file (e.g. MyView.Designer.cs).
    /// </summary>
    public FileInfo DesignerFile { get; }

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
    /// Initializes a new instance of the <see cref="SourceCodeFile"/> class.
    /// Declares a new pair of files (e.g. MyClass.cs and MyClass.Designer.cs) which
    /// may or may not both exist yet.
    /// </summary>
    /// <param name="path">Either source file of the pair (e.g. either MyClass.cs or MyClass.Designer.cs).</param>
    public SourceCodeFile(string path)
        : this(new FileInfo(path))
    {
    }

    /// <summary>
    /// Returns the .Designer.cs file for the given class file.
    /// Returns a reference even if that file does not exist.
    /// </summary>
    /// <param name="csFile">The .cs file that you want to find the partner for.</param>
    /// <returns>The .Designer.cs file that matches <paramref name="csFile"/>.</returns>
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
    /// Returns the class file for a given .Designer.cs file.
    /// </summary>
    /// <param name="designerFile">The Designer.cs file that you want to find the partner for.</param>
    /// <returns>The .cs file that matches <paramref name="designerFile"/>.</returns>
    private FileInfo GetCsFile(FileInfo designerFile)
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
