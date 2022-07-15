using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace TerminalGuiDesigner;

/// <summary>
/// Handles tracking and building commands for when multiple
/// <see cref="View"/> are selected at once within
/// the editor
/// </summary>
public class MultiSelectionManager
{
    List<Design> selection = new();

    /// <summary>
    /// Collection of all the views currently multi selected
    /// </summary>
    public IReadOnlyCollection<Design> Selected => selection.AsReadOnly();

    Dictionary<Design, ColorScheme> oldSchemes = new();
    
    /// <summary>
    /// The color scheme to assign to controls that have been 
    /// multi selected
    /// </summary>
    private ColorScheme SelectedScheme { get; set; }

    public static MultiSelectionManager Instance = new();

    private MultiSelectionManager()
    {
        SelectedScheme = new ColorScheme()
        {
            Normal = new Attribute(Color.BrightGreen, Color.Green),
            Focus = new Attribute(Color.BrightYellow, Color.Green),
            Disabled = new Attribute(Color.BrightGreen, Color.Green),
            HotFocus = new Attribute(Color.BrightYellow, Color.Green),
            HotNormal = new Attribute(Color.BrightGreen, Color.Green),
        };
    }

    public ColorScheme? GetOriginalColorScheme(Design design)
    {
        if (oldSchemes.ContainsKey(design))
            return oldSchemes[design];

        return null;
    }

    public void SetSelection(params Design[] designs)
    {
        // reset anything that was previously selected
        Clear();

        // create a new selection based on these
        selection = new List<Design>(designs.Distinct());

        foreach(var d in selection)
        {
            // record the old color scheme so we can get reset it
            // later when it is no longer selected
            oldSchemes.Add(d, d.View.ColorScheme);

            // since the view is selected mark it so
            d.View.ColorScheme = SelectedScheme;
        }
    }
    public void Clear()
    {
        selection.Clear();

        // reset old color schemes so views don't still look selected
        foreach(var kvp in oldSchemes)
        {
            kvp.Key.View.ColorScheme = kvp.Value;
        }
        oldSchemes.Clear();
    }
}
