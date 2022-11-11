using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner;

/// <summary>
/// Handles tracking and building commands for when multiple
/// <see cref="View"/> are selected at once within
/// the editor
/// </summary>
public class SelectionManager
{
    List<Design> selection = new ();

    /// <summary>
    /// Collection of all the views currently multi selected
    /// </summary>
    public IReadOnlyCollection<Design> Selected => this.selection.AsReadOnly();

    /// <summary>
    /// Set to true to prevent changes to the current <see cref="Selected"/>
    /// collection (e.g. if running a modal dialog / context menu).
    /// </summary>
    public bool LockSelection { get; set; }

    /// <summary>
    /// The color scheme to assign to controls that have been
    /// multi selected
    /// </summary>
    public ColorScheme SelectedScheme
    {
        get
        {
            if (this.selectedScheme == null)
            {
                return this.selectedScheme = new ColorScheme()
                {
                    Normal = new Attribute(Color.BrightGreen, Color.Green),
                    Focus = new Attribute(Color.BrightYellow, Color.Green),
                    Disabled = new Attribute(Color.BrightGreen, Color.Green),
                    HotFocus = new Attribute(Color.BrightYellow, Color.Green),
                    HotNormal = new Attribute(Color.BrightGreen, Color.Green),
                };
            }
            else
            {
                return this.selectedScheme;
            }
        }

        set
        {
            this.selectedScheme = value;
        }
    }

    public static SelectionManager Instance = new ();
    private ColorScheme? selectedScheme;

    /// <summary>
    /// Changes the selection without respecting <see cref="LockSelection"/>
    /// </summary>
    /// <param name="designs"></param>
    public void ForceSetSelection(params Design[] designs)
    {
        this.SetSelection(false, designs);
    }

    /// <summary>
    /// Sets the current <see cref="Selected"/> <see cref="Design"/> collection to <paramref name="designs"/>.
    /// Does nothing if <see cref="LockSelection"/> is true.
    /// </summary>
    /// <param name="designs"></param>
    public void SetSelection(params Design[] designs)
    {        
        this.SetSelection(true, designs);
    }

    private void SetSelection(bool respectLock, Design[] designs)
    {
        if (this.LockSelection && respectLock)
        {
            return;
        }

        // reset anything that was previously selected
        this.Clear(respectLock);

        // create a new selection based on these
        this.selection = new List<Design>(designs.Distinct().Where(d => !d.IsRoot));

        foreach (var d in this.selection)
        {
            // since the view is selected mark it so
            d.View.ColorScheme = this.SelectedScheme;
        }
    }

    public void Clear(bool respectLock = true)
    {
        if (this.LockSelection && respectLock)
        {
            return;
        }

        var selected = this.selection.ToArray();

        this.selection.Clear();

        // reset old color schemes so views don't still look selected
        foreach (var d in selected)
        {
            d.View.ColorScheme = d.State.OriginalScheme;
        }
    }

    /// <summary>
    /// If there is only one element in <see cref="Selected"/> then this is returned
    /// otherwise returns null.  Returns null if there is a multi selection going on
    /// or nothing is selected
    /// </summary>
    /// <returns></returns>
    public Design? GetSingleSelectionOrNull()
    {
        if (this.selection.Count == 1)
        {
            return this.selection[0];
        }

        return null;
    }

    /// <summary>
    /// Returns the container of the currently selected item (if a single item is selected)
    /// </summary>
    /// <returns></returns>
    public Design? GetMostSelectedContainerOrNull()
    {
        return this.GetSingleSelectionOrNull()?.View.GetNearestContainerDesign();
    }
}
