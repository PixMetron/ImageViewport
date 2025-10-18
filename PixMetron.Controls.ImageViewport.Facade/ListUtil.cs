namespace PixMetron.Controls.ImageViewport.Facade
{
    internal static class ListUtil
    {
        public static void StableSort<T>(this List<T> list, Comparison<T> cmp)
        {
            // List<T>.Sort 本身是非稳定的；这里做个稳定排序（简单实现）
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