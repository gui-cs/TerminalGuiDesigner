using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner;

/// <summary>
/// Handles tracking and building commands for when multiple
/// <see cref="View"/> are selected at once within
/// the editor.
/// </summary>
public class SelectionManager
{
    private readonly List<Design> selection = new();
    private ColorScheme? selectedScheme;

    private SelectionManager()
    {
    }

    /// <summary>
    /// Gets the Singleton instance of <see cref="SelectionManager"/>.
    /// </summary>
    public static SelectionManager Instance { get; } = new();

    /// <summary>
    /// Gets the collection of all the views currently multi selected.
    /// </summary>
    public IReadOnlyCollection<Design> Selected => this.selection.AsReadOnly();

    /// <summary>
    /// Gets or Sets a value indicating whether to prevent changes to the current <see cref="Selected"/>
    /// collection (e.g. if running a modal dialog / context menu).
    /// </summary>
    // BUG: Thread-safety
    // This is not a valid synchronization method and is prone to a bunch of different issues at runtime.
    public bool LockSelection { get; set; }

    /// <summary>
    /// Gets or Sets the color scheme to assign to controls that have been selected.
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

    /// <summary>
    /// Changes the selection without respecting <see cref="LockSelection"/>.
    /// </summary>
    /// <param name="designs">One or more <see cref="Design"/> that you want to select.</param>
    public void ForceSetSelection(params Design[] designs)
    {
        this.SetSelection(false, designs);
    }

    /// <summary>
    /// Sets the current <see cref="Selected"/> <see cref="Design"/> collection to <paramref name="designs"/>.
    /// Does nothing if <see cref="LockSelection"/> is true.
    /// </summary>
    /// <param name="designs">One or more <see cref="Design"/> that you want to select.</param>
    public void SetSelection(params Design[] designs)
    {
        this.SetSelection(true, designs);
    }

    /// <summary>
    /// Clears <see cref="Selected"/>.
    /// </summary>
    /// <param name="respectLock">True to respect the locked/unlocked status of the manager.</param>
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
    /// or nothing is selected.
    /// </summary>
    /// <returns>Returns the only entry in <see cref="Selected"/> or null if no selection or
    /// ongoing multi-selection.</returns>
    public Design? GetSingleSelectionOrNull()
    {
        if (this.selection.Count == 1)
        {
            return this.selection[0];
        }

        return null;
    }

    /// <summary>
    /// Returns the container of the currently selected item (if a single item is selected).
    /// </summary>
    /// <returns>If a single <see cref="Design"/> is selected this returns its nearest designed
    /// container (ignoring internal artifacts of Terminal.Gui e.g. ContentView).</returns>
    public Design? GetMostSelectedContainerOrNull()
    {
        return this.GetSingleSelectionOrNull()?.View.GetNearestContainerDesign();
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
        this.selection.Clear();
        this.selection.AddRange(designs.Distinct().Where(d => d is { IsRoot: false }));

        foreach (var d in this.selection)
        {
            // since the view is selected mark it so
            d.View.ColorScheme = this.SelectedScheme;
        }
    }
}
