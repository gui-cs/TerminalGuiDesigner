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
    List<Design> selection = new();

    /// <summary>
    /// Collection of all the views currently multi selected
    /// </summary>
    public IReadOnlyCollection<Design> Selected => selection.AsReadOnly();

    Dictionary<Design, ColorScheme?> oldSchemes = new();

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
            if (selectedScheme == null)
            {
                return selectedScheme = new ColorScheme()
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
                return selectedScheme;
            }
        }
        set
        {
            selectedScheme = value;
        }
    }


    public static SelectionManager Instance = new();
    private ColorScheme selectedScheme;


    public ColorScheme? GetOriginalExplicitColorScheme(Design design)
    {
        if (oldSchemes.ContainsKey(design))
            return oldSchemes[design];

        return null;
    }

    /// <summary>
    /// Changes the selection without respecting <see cref="LockSelection"/>
    /// </summary>
    /// <param name="designs"></param>
    public void ForceSetSelection(params Design[] designs)
    {
        SetSelection(false,designs);
    }

    /// <summary>
    /// Sets the current <see cref="Selected"/> <see cref="Design"/> collection to <paramref name="designs"/>.
    /// Does nothing if <see cref="LockSelection"/> is true.
    /// </summary>
    /// <param name="designs"></param>
    public void SetSelection(params Design[] designs)
    {
        SetSelection(true, designs);
    }
    private void SetSelection(bool respectLock, Design[] designs)
    {
        if (LockSelection && respectLock)
            return;

        // reset anything that was previously selected
        Clear(respectLock);

        // create a new selection based on these
        selection = new List<Design>(designs.Distinct());

        foreach (var d in selection)
        {
            // record the old color scheme so we can get reset it
            // later when it is no longer selected
            oldSchemes.Add(d, d.View.GetExplicitColorScheme());

            // since the view is selected mark it so
            d.View.ColorScheme = SelectedScheme;
        }
    }

    /// <summary>
    /// Updates the cached known ColorScheme (prior to selection).  Use this method
    /// if you are making changes to the ColorScheme of an actively selected object
    /// </summary>
    public void UpdateKnownScheme(Design design, ColorScheme? colorScheme)
    {
        if(oldSchemes.ContainsKey(design))
        {
            oldSchemes[design] = colorScheme;
        }
    }

    public void Clear(bool respectLock = true)
    {
        if (LockSelection && respectLock)
            return;

        selection.Clear();

        // reset old color schemes so views don't still look selected
        foreach (var kvp in oldSchemes)
        {
            kvp.Key.View.ColorScheme = kvp.Value;
        }
        oldSchemes.Clear();
    }

    /// <summary>
    /// If there is only one element in <see cref="Selected"/> then this is returned
    /// otherwise returns null.  Returns null if there is a multi selection going on
    /// or nothing is selected
    /// </summary>
    /// <returns></returns>
    public Design? GetSingleSelectionOrNull()
    {
        if(selection.Count == 1)
            return selection[0];
        
        return null;
    }

    /// <summary>
    /// Returns the container of the currently selected item (if a single item is selected)
    /// </summary>
    /// <returns></returns>
    public Design? GetMostSelectedContainerOrNull()
    {
        return GetSingleSelectionOrNull()?.View.GetNearestContainerDesign();
    }
}
