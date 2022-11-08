namespace TerminalGuiDesigner
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns <paramref name="name"/> such that it is unique and does not collide with any entries
        /// in <paramref name="inScope"/> collection.
        /// </summary>
        /// <param name="name">A string to make unique.</param>
        /// <param name="inScope">Comparison to use or null for default (<see cref="StringComparer.InvariantCulture"/>).</param>
        /// <param name="comparer">Comparer for matching against collection (e.g. case sensitive or not).</param>
        /// <returns>The same string if it is unique or a modified version (e.g. with numerical suffix).</returns>
        public static string MakeUnique(this string name, IEnumerable<string> inScope, StringComparer? comparer = null)
        {
            comparer ??= StringComparer.InvariantCulture;

            // in case it is single iteration
            var set = new HashSet<string>(inScope, comparer);

            if (!set.Contains(name, comparer))
            {
                return name;
            }

            // name is already used, add a number
            int number = 2;

            while (set.Contains(name + number, comparer))
            {
                // menu2 is taken, try menu3 etc
                number++;
            }

            // found a unique one
            return name + number;
        }
    }
}
