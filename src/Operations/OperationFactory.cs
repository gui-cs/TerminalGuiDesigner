using Terminal.Gui;

namespace TerminalGuiDesigner.Operations
{
    public class OperationFactory
    {
        private PropertyValueGetterDelegate _valueGetter;

        public OperationFactory(PropertyValueGetterDelegate valueGetter)
        {
            _valueGetter = valueGetter;
        }

        public IEnumerable<IOperation> CreateOperations(Design[] selected, MouseEvent? m, Design? rightClicked, out string name)
        {
            List<IOperation> toReturn = new();

            // user right clicked something that isn't part of the current multiselection
            if (rightClicked != null && !selected.Contains(rightClicked))
            {
                // give them options for the thing they right clicked
                name = rightClicked.FieldName;
                foreach (var op in CreateOperations(m, rightClicked))
                {
                    toReturn.Add(op);
                }
            }
            else
            if (selected.Length == 1)
            {
                name = selected[0].FieldName;
                foreach (var op in CreateOperations(m, selected[0]))
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
                        props.Add(new SetPropertyOperation(all.Select(v => v.Design).ToArray(), propertyName, _valueGetter));
                    }
                }

                toReturn.AddRange(props.OrderBy(p => p.ToString()));
            }
            else
            {
                name = "";
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
                d.GetExtraOperations(d.View.ScreenToClient(m.Value.X, m.Value.Y));

            foreach (var extra in ops.Where(c => !c.IsImpossible))
            {
                yield return extra;
            }

            foreach (var prop in d.GetDesignableProperties().OrderBy(p => p.GetHumanReadableName()))
            {
                yield return new SetPropertyOperation(d, prop, _valueGetter);
            }
        }
    }
}