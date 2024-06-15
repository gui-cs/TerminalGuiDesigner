using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalGuiDesigner
{
    internal static class EnumerableExtensions
    {
        public static ObservableCollection<T> ToListObs<T>(this IEnumerable<T> e)
        {
            return new ObservableCollection<T>(e);
        }
    }
}
