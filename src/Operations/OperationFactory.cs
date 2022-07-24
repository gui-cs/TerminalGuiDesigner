using Terminal.Gui;
using TerminalGuiDesigner.ToCode;

namespace TerminalGuiDesigner.Operations
{
    public class OperationFactory
    {
        private Func<Property, object?> _valueGetter;

        public OperationFactory(Func<Property,object?> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        public IEnumerable<IOperation> CreateOperations(Design[] selected, MouseEvent? m, Design? rightClicked, out string name)
        {
            List<IOperation> toReturn = new();

            // user right clicked something that isn't part of the current multiselection
            if(rightClicked != null && !selected.Contains(rightClicked))
            {
                // give them options for the thing they right clicked
                name = rightClicked.FieldName;
                foreach (var op in CreateOperations(m, rightClicked))
                    toReturn.Add(op);
            }
            else
            if(selected.Length == 1)
            {
                name = selected[0].FieldName;
                foreach (var op in CreateOperations(m, selected[0]))
                    toReturn.Add(op);

            }
            else
            if (selected.Length > 0)
            {
                // TODO: Multi selections!
                name = $"{selected.Length} Items";
                foreach(var op in CreateOperations(m, selected[0]))
                {
                    // TODO: this is wrong we should be doing 
                    // multi set properties here not just throwing them away
                    if(op is not SetPropertyOperation)
                    {
                        toReturn.Add(op);
                    }

                }
            }
            else
            {
                name = "";
            }

            if(SelectionManager.Instance.Selected.Any())
            {
                toReturn.Add(new CopyOperation(SelectionManager.Instance.Selected.ToArray()));
            }
            else if(rightClicked != null)
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


            foreach(var prop in d.GetDesignableProperties().OrderBy(p => p.GetHumanReadableName()))
            {
                yield return new SetPropertyOperation(d, prop, _valueGetter);
            }
        }
    }
}