using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;
using TerminalGuiDesigner.UI.Windows;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{

    public class RemoveStatusItemOperation : RemoveOperation<StatusBar, StatusItem>
    {
        public RemoveStatusItemOperation(Design design, StatusItem toRemove)
            : base(
                  (v) => v.Items,
                  (v, a) => v.Items = a,
                  (e) => e.Title?.ToString() ?? Operation.Unnamed,
                  design,
                  toRemove)
        {
        }
    }
}
