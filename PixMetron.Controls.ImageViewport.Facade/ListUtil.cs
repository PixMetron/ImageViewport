namespace PixMetron.Controls.ImageViewport.Facade
{
    /// <summary>
    /// Provides utility methods for list operations.
    /// </summary>
    internal static class ListUtil
    {
        /// <summary>
        /// Performs a stable sort on the list using the specified comparison function.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to sort.</param>
        /// <param name="cmp">The comparison function to use for sorting.</param>
        /// <remarks>
        /// Since List&lt;T&gt;.Sort does not guarantee stability, this method implements a stable sort
        /// by preserving the original index order for equal elements.
        /// </remarks>
        public static void StableSort<T>(this List<T> list, Comparison<T> cmp)
        {
            // List<T>.Sort is not guaranteed to be stable, so we implement stable sort (simple implementation)
            var indexed = new List<(T item, int idx)>(list.Count);
            for (int i = 0; i < list.Count; i++) indexed.Add((list[i], i));
            indexed.Sort((a, b) =>
            {
                int c = cmp(a.item, b.item);
                return (c != 0) ? c : a.idx.CompareTo(b.idx);
            });
            for (int i = 0; i < indexed.Count; i++) list[i] = indexed[i].item;
        }
    }
}