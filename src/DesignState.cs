using Terminal.Gui;
using TerminalGuiDesigner.UI;

namespace TerminalGuiDesigner;

/// <summary>
/// Describes state based changes and custom callbacks on a <see cref="Design"/>
/// e.g. <see cref="OriginalScheme"/>.
/// </summary>
public class DesignState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesignState"/> class. Registers
    /// events on the <see cref="Design.View"/> that track entering, redrawing etc
    /// as well as capturing initial state e.g. <see cref="OriginalScheme"/>.
    /// </summary>
    /// <param name="design">The parent that this class will hold state for.  You
    /// should set it's <see cref="Design.State"/> property to the new instance.
    /// </param>
    public DesignState(Design design)
    {
        this.Design = design;
        this.OriginalScheme = this.Design.View.GetExplicitColorScheme();
        this.Design.View.DrawContent += this.DrawContent;
        this.Design.View.Enter += this.Enter;
    }

    /// <summary>
    /// Gets or Sets the explicitly defined <see cref="ColorScheme"/> that the user wants for their <see cref="View"/>.
    /// This is what is used when writing to code/showing properties in editor.  This may differ from the
    /// actual color the <see cref="View"/> currently has within the editor (e.g. if it is selected and
    /// has a temporary color indicating it is selected - see <see cref="SelectionManager.SelectedScheme"/>).
    /// </summary>
    public ColorScheme? OriginalScheme { get; set; }

    /// <summary>
    /// Gets the parent <see cref="Design"/> that this class stores state for.
    /// </summary>
    public Design Design { get; }

    private void Enter(View.FocusEventArgs obj)
    {
        // when tabbing or clicking into this View when nothing complicated is going on (e.g. Ctrl+Click multi select)
        if (SelectionManager.Instance.Selected.Count <= 1)
        {
            // set the selection to the View that has focus
            SelectionManager.Instance.SetSelection(this.Design);
        }
    }

    private void DrawContent(Rect r)
    {
        if (this.Design.View.IsBorderlessContainerView() && Editor.ShowBorders)
        {
            this.DrawBorderlessViewFrame(r);
        }
    }

    private void DrawBorderlessViewFrame(Rect r)
    {
        bool isSelected = SelectionManager.Instance.Selected.Contains(this.Design);

        var color = isSelected ?
            SelectionManager.Instance.SelectedScheme.Normal :
            this.Design.View.ColorScheme.Normal;

        Application.Driver.SetAttribute(color);

        var v = this.Design.View;

        for (int x = 0; x < r.Width; x++)
        {
            for (int y = 0; y < r.Height; y++)
            {
                if (y == 0 || y == r.Height - 1 || x == 0 || x == r.Width - 1)
                {
                    var rune = (y == r.Height - 1 && x == r.Width - 1 && isSelected) ? '╬' : '.';
                    v.AddRune(x, y, rune);
                }
            }
        }
    }
}