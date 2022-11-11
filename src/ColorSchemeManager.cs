using System.Collections.ObjectModel;
using System.Reflection;
using Terminal.Gui;
using TerminalGuiDesigner;

namespace TerminalGuiDesigner;

/// <summary>
/// A user defined <see cref="ColorScheme"/> and its name as defined
/// by the user.  The <see cref="Name"/> will be used as Field name
/// in the class code generated so must not contain illegal characters/spaces
/// </summary>
public class NamedColorScheme
{
    public string Name { get; set; }

    public ColorScheme Scheme { get; set; }

    public NamedColorScheme(string name, ColorScheme scheme)
    {
        this.Name = name;
        this.Scheme = scheme;
    }

    public NamedColorScheme(string name)
    {
        this.Name = name;
        this.Scheme = new ColorScheme();
    }

    public override string ToString()
    {
        return this.Name;
    }
}

public class ColorSchemeManager
{
    List<NamedColorScheme> colorSchemes = new ();

    /// <summary>
    /// All known color schemes defined by name
    /// </summary>
    public ReadOnlyCollection<NamedColorScheme> Schemes => this.colorSchemes.ToList().AsReadOnly();

    public static ColorSchemeManager Instance = new ();

    private ColorSchemeManager()
    {
    }

    public void Clear()
    {
        this.colorSchemes = new ();
    }

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
    /// <param name="viewBeingEdited">View to find color schemes in, must be the root design (i.e. <see cref="Design.IsRoot"/>)</param>
    /// <exception cref="ArgumentException"></exception>
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

    public string? GetNameForColorScheme(ColorScheme s)
    {
        var match = this.colorSchemes.Where(kvp => s.AreEqual(kvp.Scheme)).ToArray();

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
    /// <param name="name"></param>
    /// <param name="scheme"></param>
    /// <param name="rootDesign"></param>
    public void AddOrUpdateScheme(string name, ColorScheme scheme, Design rootDesign)
    {
        var oldScheme = this.colorSchemes.FirstOrDefault(c => c.Name.Equals(name));

        // if we don't currently know about this scheme
        if (oldScheme == null)
        {
            // simply record that we now know about it and exit
            this.colorSchemes.Add(new NamedColorScheme(name, scheme));
            return;
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
    }

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
    /// <see cref="NamedColorScheme.Name"/> matches <paramref name="name"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Thrown if the <paramref name="name"/> is not present in <see cref="Schemes"/></exception>
    public NamedColorScheme GetNamedColorScheme(string name)
    {
        return this.colorSchemes.FirstOrDefault(c => c.Name.Equals(name))
            ?? throw new KeyNotFoundException($"Could not find a named ColorScheme called {name}");
    }
}