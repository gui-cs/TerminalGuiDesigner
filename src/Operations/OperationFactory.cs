using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.Operations;

/// <summary>
/// Factory class for determining and creating all <see cref="Operation"/> subclasses that can
/// be performed on a given <see cref="Design"/>.
/// </summary>
public class OperationFactory
{
    private PropertyValueGetterDelegate valueGetter;

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationFactory"/> class.
    /// </summary>
    /// <param name="valueGetter">Delegate for getting new <see cref="Property"/> values.  This
    /// will be passed to created operations e.g. <see cref="SetPropertyOperation"/>.</param>
    public OperationFactory(PropertyValueGetterDelegate valueGetter)
    {
        this.valueGetter = valueGetter;
    }

    /// <summary>
    /// Creates and returns all <see cref="Operations"/> that can be performed on the given <paramref name="selected"/>
    /// set of <see cref="Design"/> given the mouse state.
    /// </summary>
    /// <param name="selected">All <see cref="Design"/> that are currently selected (see <see cref="SelectionManager"/>).</param>
    /// <param name="m"><see cref="MouseEvent"/> (if any) that prompted this request e.g. if user right clicks a <see cref="Design"/>.</param>
    /// <param name="rightClicked">If <paramref name="m"/> is populated then this should be the <see cref="Design"/> wrapper for
    /// the <see cref="View"/> that the mouse was over at the time it was clicked (see <see cref="ViewExtensions.HitTest(View, MouseEvent, out bool, out bool, View[])"/>.</param>
    /// <param name="name">String that represents what the returned <see cref="IOperation"/> act upon e.g. "myLabel" or "8 objects".</param>
    /// <returns>Collection of all <see cref="IOperation"/> that can be offered to user as runnable given the current selection.</returns>
    public IEnumerable<IOperation> CreateOperations(Design[] selected, MouseEvent? m, Design? rightClicked, out string name)
    {
        List<IOperation> toReturn = new();

        // user right clicked something that isn't part of the current multi-selection
        if (rightClicked != null && !selected.Contains(rightClicked))
        {
            // give them options for the thing they right clicked
            name = rightClicked.FieldName;
            foreach (var op in this.CreateOperations(m, rightClicked))
            {
                toReturn.Add(op);
            }
        }
        else
        if (selected.Length == 1)
        {
            name = selected[0].FieldName;
            foreach (var op in this.CreateOperations(m, selected[0]))
            {
                toReturn.Add(op);
            }
        }
        else
        if (selected.Length > 1)
        {
            var props = new List<SetPropertyOperation>();

            name = $"{selected.Length} Items";

            // for each property name
            var propGroup =
                selected.SelectMany(d => d.GetDesignableProperties())
                .GroupBy(p => p.PropertyInfo.Name);

            foreach (var g in propGroup)
            {
                var propertyName = g.Key;

                var all = g.ToList();

                // if all views in the collection have this Property declared designable on them
                if (all.Count == selected.Length)
                {
                    // create an operation to change them all at once
                    props.Add(new SetPropertyOperation(all.Select(v => v.Design).ToArray(), propertyName, this.valueGetter));
                }
            }

            toReturn.AddRange(props.OrderBy(p => p.ToString()));
        }
        else
        {
            name = string.Empty;
        }

        if (SelectionManager.Instance.Selected.Any())
        {
            toReturn.Add(new CopyOperation(SelectionManager.Instance.Selected.ToArray()));
        }
        else if (rightClicked != null)
        {
            toReturn.Add(new CopyOperation(rightClicked));
        }

        return toReturn;
    }

    private IEnumerable<IOperation> CreateOperations(MouseEvent? m, Design d)
    {
        var ops = m == null ?
            d.GetExtraOperations() :
            d.GetExtraOperations(d.View.ScreenToView(m.X, m.Y));

        foreach (var extra in ops.Where(c => !c.IsImpossible))
        {
            yield return extra;
        }

        foreach (var prop in d.GetDesignableProperties().OrderBy(p => p.GetHumanReadableName()))
        {
            yield return new SetPropertyOperation(d, prop, this.valueGetter);
        }
    }
}