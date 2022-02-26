using System.Text;

namespace TerminalGuiDesigner;

/// <summary>
/// Cross UI platform (winforms, console, terminal gui) arguments for describing the theming and text
/// that should appear in a modal 'select something' style operation in <see cref="IBasicActivateItems"/> 
/// (the user interface abstraction layer)
/// </summary>
public class DialogArgs
{
    /// <summary>
    /// What text should appear in the window area of the dialog or the initial
    /// header text (in the case of console output)
    /// </summary>
    public string WindowTitle { get; set; }

    /// <summary>
    /// Text that indicates what the user should be doing (i.e. user help to
    /// remind them what is going on)
    /// </summary>
    public string TaskDescription { get; set; }


    /// <summary>
    /// The final line of text before user entered input e.g. the label on
    /// a text box in which the user must enter the choice
    /// </summary>
    public string EntryLabel { get; set; }

    /// <summary>
    /// If there is only one valid choice can this be automatically selected without
    /// blocking or showing any feedback to the user.
    /// </summary>
    public bool AllowAutoSelect { get; set; }

    /// <summary>
    /// <para>
    /// If the user interface component allows filtering/searching then you can provide
    /// a string here which will indicate what the initial search text should be set to
    /// (if any).  
    /// </para>
    /// 
    /// <para>
    /// This is the value for inside the search text not the caption.
    /// </para>
    /// </summary>
    public string InitialSearchText { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrEmpty(WindowTitle))
            sb.AppendLine($"Title:{WindowTitle}");

        if (!string.IsNullOrEmpty(TaskDescription))
            sb.AppendLine($"Task:{TaskDescription}");

        if (!string.IsNullOrEmpty(EntryLabel))
            sb.AppendLine($"Label:{EntryLabel}");

        if (sb.Length == 0)
        {
            return "Undefined DialogArgs";
        }

        return sb.ToString();
    }
}
