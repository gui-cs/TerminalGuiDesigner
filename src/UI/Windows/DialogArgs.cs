using System.Text;

namespace TerminalGuiDesigner.UI.Windows;

/// <summary>
/// Arguments for describing the theming and text that should appear in a modal
/// 'select something' style operation.
/// </summary>
public class DialogArgs
{
    /// <summary>
    /// Gets or Sets what text should appear in the window area of the dialog or the initial
    /// header text (in the case of console output).
    /// </summary>
    public string? WindowTitle { get; set; }

    /// <summary>
    /// Gets or Sets the text that indicates what the user should be doing (i.e. user help to
    /// remind them what is going on).
    /// </summary>
    public string? TaskDescription { get; set; }

    /// <summary>
    /// Gets or Sets the final line of text before user entered input e.g. the label on
    /// a text box in which the user must enter the choice.
    /// </summary>
    public string? EntryLabel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple lines are allowed in user input
    /// (only applies if <see cref="DialogArgs"/> is used to get a string from user).
    /// </summary>
    public bool MultiLine { get; set; }

    /// <summary>
    /// True to make newlines toggleable (e.g. for Label).
    /// </summary>
    public bool ToggleableMultiLine { get; set; } = true;

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrEmpty(this.WindowTitle))
        {
            sb.AppendLine($"Title:{this.WindowTitle}");
        }

        if (!string.IsNullOrEmpty(this.TaskDescription))
        {
            sb.AppendLine($"Task:{this.TaskDescription}");
        }

        if (!string.IsNullOrEmpty(this.EntryLabel))
        {
            sb.AppendLine($"Label:{this.EntryLabel}");
        }

        if (sb.Length == 0)
        {
            return "Undefined DialogArgs";
        }

        return sb.ToString();
    }
}
