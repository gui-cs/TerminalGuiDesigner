using System.Collections.ObjectModel;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;

namespace TerminalGuiDesigner;

/// <summary>
/// Tracks usage of <see cref="Scheme"/> in designed views.
/// Each <see cref="Scheme"/> that the user has created or are
/// supplied by the designer out of the box is modeled by <see cref="NamedScheme"/>.
/// This class hosts the collection of all <see cref="NamedScheme"/>.
/// </summary>
public class SchemeManager
{
    private readonly List<NamedScheme> Schemes = new();

    private SchemeManager()
    {
    }

    /// <summary>
    /// Gets the Singleton instance of <see cref="SchemeManager"/>.
    /// </summary>
    public static SchemeManager Instance { get; } = new();

    /// <summary>
    /// Gets all known named color schemes defined in editor.
    /// </summary>
    public ReadOnlyCollection<NamedScheme> Schemes => this.Schemes.ToList().AsReadOnly();

    /// <summary>
    /// Clears all <see cref="NamedScheme"/> tracked by manager.
    /// </summary>
    public void Clear()
    {
        this.Schemes.Clear();
    }

    /// <summary>
    /// Makes <see cref="SchemeManager"/> forget about <paramref name="toDelete"/>.
    /// Note that this does not remove it from any users (to do that use
    /// <see cref="DeleteSchemeOperation"/> instead).
    /// </summary>
    /// <param name="toDelete"><see cref="NamedScheme"/> to forget about.</param>
    public void Remove(NamedScheme toDelete)
    {
        // match on name as instances may change e.g. due to Undo/Redo etc
        var match = this.Schemes.FirstOrDefault(s => s.Name.Equals(toDelete.Name));

        if (match != null)
        {
            this.Schemes.Remove(match);
        }
    }

    /// <summary>
    /// Populates <see cref="Schemes"/> based on the private Scheme instances declared in the
    /// Designer.cs file of the <paramref name="viewBeingEdited"/>.  Does not clear any existing known
    /// schemes.
    /// </summary>
    /// <param name="viewBeingEdited">View to find color schemes in, must be the root design (i.e. <see cref="Design.IsRoot"/>).</param>
    /// <exception cref="ArgumentException">Thrown if passed a non-root <see cref="Design"/>.</exception>
    public void FindDeclaredSchemes(Design viewBeingEdited)
    {
        if (!viewBeingEdited.IsRoot)
        {
            throw new ArgumentException("Expected to only be passed the root view");
        }

        var view = viewBeingEdited.View;

        // find all fields in class
        var schemes = view.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(t => t.FieldType == typeof(Scheme));

        foreach (var f in schemes)
        {
            var val = f.GetValue(view) as Scheme;

            if (val != null && !this.Schemes.Any(s => s.Name.Equals(f.Name)))
            {
                this.Schemes.Add(new NamedScheme(f.Name, val));
            }
        }
    }

    /// <summary>
    /// Returns the <see cref="NamedScheme.Name"/> for <paramref name="s"/>
    /// if it is in the collection of known <see cref="Schemes"/>.
    /// </summary>
    /// <param name="s">A <see cref="Scheme"/> to look up.</param>
    /// <returns>The name of the scheme or null if it is not known.</returns>
    public string? GetNameForScheme(Scheme s)
    {
        var match = this.Schemes.Where(kvp => s.Equals(kvp.Scheme)).ToArray();

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
    /// <param name="name">The user generated name for the <see cref="Scheme"/>.
    /// Will become <see cref="NamedScheme.Name"/>.</param>
    /// <param name="scheme">The new <see cref="Scheme"/> color values to use.</param>
    /// <param name="rootDesign">The topmost <see cref="Design"/> the user is editing (see <see cref="Design.GetRootDesign"/>).</param>
    /// <returns>A reference to the <see cref="Scheme"/> that was added or updated.</returns>
    public Scheme AddOrUpdateScheme(string name, Scheme scheme, Design rootDesign)
    {
        // if we don't currently know about this scheme
        if (this.Schemes.FirstOrDefault(c => c.Name.Equals(name)) is not { } oldScheme)
        {
            // simply record that we now know about it and exit
            NamedScheme newScheme = new (name, scheme);
            this.Schemes.Add(newScheme);
            return newScheme.Scheme;
        }

        // we know about this color already and people may be using it!
        foreach (var old in rootDesign.GetAllDesigns())
        {
            // if view uses the scheme that is being replaced (value not reference equality)
            if (old.UsesScheme(oldScheme.Scheme))
            {
                // use the new one instead (for the presented View in the GUI and the known state)
                old.View.Scheme = old.State.OriginalScheme = scheme;
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
        var match = this.Schemes.FirstOrDefault(c => c.Name.Equals(oldName));

        if (match != null)
        {
            match.Name = newName;
        }
    }

    /// <summary>
    /// Returns the <see cref="NamedScheme"/> from <see cref="Schemes"/> where
    /// <see cref="NamedScheme.Name"/> matches <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name to look up.</param>
    /// <returns>The scheme if found or null.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the <paramref name="name"/> is not present in <see cref="Schemes"/>.</exception>
    public NamedScheme GetNamedScheme(string name)
    {
        return this.Schemes.FirstOrDefault(c => c.Name.Equals(name))
            ?? throw new KeyNotFoundException($"Could not find a named Scheme called {name}");
    }
}