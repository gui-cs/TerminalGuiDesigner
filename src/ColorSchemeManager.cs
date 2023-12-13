using System.Collections.ObjectModel;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner;

/// <summary>
/// Tracks usage of <see cref="ColorScheme"/> in designed views.
/// Each <see cref="ColorScheme"/> that the user has created or are
/// supplied by the designer out of the box is modeled by <see cref="NamedColorScheme"/>.
/// This class hosts the collection of all <see cref="NamedColorScheme"/>.
/// </summary>
public class ColorSchemeManager
{
    private readonly List<NamedColorScheme> colorSchemes = new();

    private ColorSchemeManager()
    {
    }

    /// <summary>
    /// Gets the Singleton instance of <see cref="ColorSchemeManager"/>.
    /// </summary>
    public static ColorSchemeManager Instance { get; } = new();

    /// <summary>
    /// Gets all known named color schemes defined in editor.
    /// </summary>
    public ReadOnlyCollection<NamedColorScheme> Schemes => this.colorSchemes.ToList().AsReadOnly();

    /// <summary>
    /// Clears all <see cref="NamedColorScheme"/> tracked by manager.
    /// </summary>
    public void Clear()
    {
        this.colorSchemes.Clear();
    }

    /// <summary>
    /// Makes <see cref="ColorSchemeManager"/> forget about <paramref name="toDelete"/>.
    /// Note that this does not remove it from any users (to do that use
    /// <see cref="DeleteColorSchemeOperation"/> instead).
    /// </summary>
    /// <param name="toDelete"><see cref="NamedColorScheme"/> to forget about.</param>
    public void Remove(NamedColorScheme toDelete)
    {
        // match on name as instances may change e.g. due to Undo/Redo etc
        var match = this.colorSchemes.FirstOrDefault(s => s.Name.Equals(toDelete.Name));

        if (match != null)
        {
            this.colorSchemes.Remove(match);
        }
    }

    /// <summary>
    /// Populates <see cref="Schemes"/> based on the private ColorScheme instances declared in the
    /// Designer.cs file of the <paramref name="viewBeingEdited"/>.  Does not clear any existing known
    /// schemes.
    /// </summary>
    /// <param name="viewBeingEdited">View to find color schemes in, must be the root design (i.e. <see cref="Design.IsRoot"/>).</param>
    /// <exception cref="ArgumentException">Thrown if passed a non root <see cref="Design"/>.</exception>
    public void FindDeclaredColorSchemes(Design viewBeingEdited)
    {
        if (!viewBeingEdited.IsRoot)
        {
            throw new ArgumentException("Expected to only be passed the root view");
        }

        var view = viewBeingEdited.View;

        // find all fields in class
        var schemes = view.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(t => t.FieldType == typeof(ColorScheme));

        foreach (var f in schemes)
        {
            var val = f.GetValue(view) as ColorScheme;

            if (val != null && !this.colorSchemes.Any(s => s.Name.Equals(f.Name)))
            {
                this.colorSchemes.Add(new NamedColorScheme(f.Name, val));
            }
        }
    }

    /// <summary>
    /// Returns the <see cref="NamedColorScheme.Name"/> for <paramref name="s"/>
    /// if it is in the collection of known <see cref="Schemes"/>.
    /// </summary>
    /// <param name="s">A <see cref="ColorScheme"/> to look up.</param>
    /// <returns>The name of the scheme or null if it is not known.</returns>
    public string? GetNameForColorScheme(ColorScheme s)
    {
        var match = this.colorSchemes.Where(kvp => s.Equals(kvp.Scheme)).ToArray();

        if (match.Length > 0)
        {
            return match[0].Name;
        }

        // no match
        return null;
    }

    /// <summary>
    /// Updates the named scheme to use the new colors in <paramref name="scheme"/>.  This
    /// will also update all Views in <paramref name="rootDesign"/> which currently use the
    /// named scheme.
    /// </summary>
    /// <param name="name">The user generated name for the <see cref="ColorScheme"/>.
    /// Will become <see cref="NamedColorScheme.Name"/>.</param>
    /// <param name="scheme">The new <see cref="ColorScheme"/> color values to use.</param>
    /// <param name="rootDesign">The topmost <see cref="Design"/> the user is editing (see <see cref="Design.GetRootDesign"/>).</param>
    /// <returns>A reference to the <see cref="ColorScheme"/> that was added or updated.</returns>
    public ColorScheme AddOrUpdateScheme(string name, ColorScheme scheme, Design rootDesign)
    {
        // if we don't currently know about this scheme
        if (this.colorSchemes.FirstOrDefault(c => c.Name.Equals(name)) is not { } oldScheme)
        {
            // simply record that we now know about it and exit
            NamedColorScheme newColorScheme = new (name, scheme);
            this.colorSchemes.Add(newColorScheme);
            return newColorScheme.Scheme;
        }

        // we know about this color already and people may be using it!
        foreach (var old in rootDesign.GetAllDesigns())
        {
            // if view uses the scheme that is being replaced (value not reference equality)
            if (old.UsesColorScheme(oldScheme.Scheme))
            {
                // use the new one instead (for the presented View in the GUI and the known state)
                old.View.ColorScheme = old.State.OriginalScheme = scheme;
            }
        }

        oldScheme.Scheme = scheme;
        return scheme;
    }

    /// <summary>
    /// Renames the known <see cref="Schemes"/> that is called <paramref name="oldName"/> to
    /// <paramref name="newName"/> if the name exists in <see cref="Schemes"/>.
    /// </summary>
    /// <param name="oldName">The name to change.</param>
    /// <param name="newName">The value to change it to.</param>
    public void RenameScheme(string oldName, string newName)
    {
        var match = this.colorSchemes.FirstOrDefault(c => c.Name.Equals(oldName));

        if (match != null)
        {
            match.Name = newName;
        }
    }

    /// <summary>
    /// Returns the <see cref="NamedColorScheme"/> from <see cref="Schemes"/> where
    /// <see cref="NamedColorScheme.Name"/> matches <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name to look up.</param>
    /// <returns>The scheme if found or null.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the <paramref name="name"/> is not present in <see cref="Schemes"/>.</exception>
    public NamedColorScheme GetNamedColorScheme(string name)
    {
        return this.colorSchemes.FirstOrDefault(c => c.Name.Equals(name))
            ?? throw new KeyNotFoundException($"Could not find a named ColorScheme called {name}");
    }
}