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
            // user right clicked something that isn't part of the current multiselection
            if(rightClicked != null && !selected.Contains(rightClicked))
            {
                // give them options for the thing they right clicked
                name = rightClicked.FieldName;
                return CreateOperations(m, rightClicked);
            }

            if(selected.Length == 1)
            {
                name = selected[0].FieldName;
                return CreateOperations(m, selected[0]);
            }
            if (selected.Length > 0)
            {
                // TODO: Multi selections!
                name = $"{selected.Length} Items";
                return CreateOperations(m, selected[0])
                    // TODO: this is wrong we should be doing 
                    // multi set properties here not just throwing them away
                    .Where(o=>o is not SetPropertyOperation);
            }
            else
            {
                name = "Comming Soon";
                return Enumerable.Empty<IOperation>();
            }
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