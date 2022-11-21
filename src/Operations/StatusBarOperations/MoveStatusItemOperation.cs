using Terminal.Gui;
using TerminalGuiDesigner.Operations.Generics;

namespace TerminalGuiDesigner.Operations.StatusBarOperations
{
    public class MoveStatusItemOperation : MoveOperation<StatusBar, StatusItem>
    {
        public MoveStatusItemOperation(Design design, StatusItem toMove, int adjustment)
            :base(
                 (v)=>v.Items,
                 (v,a)=>v.Items = a,
                 (e)=>e.Title?.ToString() ?? Operation.Unnamed,
                 design,
                 toMove,
                 adjustment)
        {
        }
    }
}
