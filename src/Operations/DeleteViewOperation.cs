using Terminal.Gui;
using System.Linq;

namespace TerminalGuiDesigner.Operations;

public class DeleteViewOperation : Operation
{
    private readonly View[] delete;
    private readonly View[] from;

    public DeleteViewOperation(params View[] delete)
    {
        this.delete = delete;
        this.from = delete.Select(d=>d.SuperView).ToArray();

        foreach(var del in delete)
        {
            if (del.Data is Design design)
            {
                // don't delete the root view!
                if (design.IsRoot)
                    IsImpossible = true;

                // there are view(s) that depend on us (e.g. for positioning)
                // deleting us would go very badly
                if (design.GetDependantDesigns()
                        // unless we are also deleting those too in which case its fine
                        .Any(dep => !delete.Contains(dep.View)))
                {
                    IsImpossible = true;
                }
            }
        }
        
    }

    public override bool Do()
    {
        bool removedAny = false;

        for(int i=0;i<delete.Length;i++)
        {
            if(from[i] != null)
            {
                from[i].Remove(delete[i]);
                removedAny = true;
            }
        }
        

        return removedAny;
    }

    public override void Redo()
    {
        Do();
    }

    public override void Undo()
    {
        for(int i=0;i<delete.Length;i++)
        {
            if(from[i] != null)
            {
                from[i].Add(delete[i]);
            }
        }
    }

    public override string ToString()
    {
        return "Delete";
    }
}
