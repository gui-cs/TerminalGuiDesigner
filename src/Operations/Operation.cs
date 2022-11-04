using System.Text.RegularExpressions;

namespace TerminalGuiDesigner.Operations;

public abstract class Operation : IOperation
{
    public bool IsImpossible { get; protected set; }

    public bool SupportsUndo { get; protected set; } = true;

    public Guid UniqueIdentifier { get; } = Guid.NewGuid();

    public override string ToString()
    {
        return this.GetOperationName();
    }

    protected string GetOperationName()
    {
        string name = this.GetType().Name;

        return name.EndsWith("Operation") ?
            name.Substring(0, name.Length - "Operation".Length) :
            name;
    }

    public static string PascalCaseStringToHumanReadable(string pascalCaseString)
    {
        // Deal with legacy property names by replacing underscore with a space
        pascalCaseString = pascalCaseString.Replace("_", " ");

        // There are two clauses in this Regex
        // Part1: [A-Z][A-Z]*(?=[A-Z][a-z]|\b) - looks for any series of uppercase letters that have a ending uppercase then lowercase OR end of line charater: https://regex101.com/r/mCqVk6/2
        // Part2: [A-Z](?=[a-z])               - looks for any single  of uppercase letters followed by a lower case letter: https://regex101.com/r/hdSCqH/1
        // This is then made into a single group that is matched and we add a space on front during the replacement.
        pascalCaseString = Regex.Replace(pascalCaseString, @"([A-Z][A-Z]*(?=[A-Z][a-z]|\b)|[A-Z](?=[a-z]))", " $1");

        // Remove any double mutliple white space
        // Because this matched the first capital letter in a string with Part2 of our regex above we should TRIM to remove the white space.
        pascalCaseString = Regex.Replace(pascalCaseString, @"\s\s+", " ").Trim();

        return pascalCaseString;
    }

    public abstract bool Do();

    public abstract void Undo();

    public abstract void Redo();

    /// <summary>
    /// Clears <see cref="SelectionManager.Selected"/> without respecting
    /// <see cref="SelectionManager.LockSelection"/>
    /// </summary>
    /// <param name="selection"></param>
    protected void ForceSelectionClear()
    {
        var before = SelectionManager.Instance.LockSelection;
        SelectionManager.Instance.LockSelection = false;
        SelectionManager.Instance.Clear();
        SelectionManager.Instance.LockSelection = before;
    }

    /// <summary>
    /// Changes <see cref="SelectionManager.Selected"/> without respecting
    /// <see cref="SelectionManager.LockSelection"/>
    /// </summary>
    /// <param name="selection"></param>
    protected void ForceSelection(params Design[] selection)
    {
        var before = SelectionManager.Instance.LockSelection;
        SelectionManager.Instance.LockSelection = false;
        SelectionManager.Instance.SetSelection(selection);
        SelectionManager.Instance.LockSelection = before;
    }
}
