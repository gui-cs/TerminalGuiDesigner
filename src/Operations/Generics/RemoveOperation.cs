using Terminal.Gui;

namespace TerminalGuiDesigner.Operations.Generics
{
    public class RemoveOperation<T1, T2> : GenericArrayElementOperation<T1, T2>
        where T1 : View
    {
        private readonly int idx;

        public RemoveOperation(
            ArrayGetterDelegate<T1, T2> arrayGetter,
            ArraySetterDelegate<T1, T2> arraySetter,
            StringGetterDelegate<T2> stringGetter,
            Design design,
            T2 toRemove)
            : base(arrayGetter, arraySetter, stringGetter, design, toRemove)
        {
            this.idx = Array.IndexOf(arrayGetter(this.View), toRemove);
        }

        /// <inheritdoc/>
        public override void Undo()
        {
            // its not there anyways
            if (this.OperateOn == null)
            {
                return;
            }

            var current = this.ArrayGetter(this.View).Cast<T2>().ToList();
            current.Insert(this.idx, this.OperateOn);
            this.ArraySetter(this.View, current.ToArray());
            this.SetNeedsDisplay();
        }

        /// <inheritdoc/>
        public override void Redo()
        {
            this.Do();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Remove {typeof(T2).Name} '{this.StringGetter(this.OperateOn)}'";
        }

        /// <inheritdoc/>
        protected override bool DoImpl()
        {
            return this.Remove(this.OperateOn);
        }
    }
}
