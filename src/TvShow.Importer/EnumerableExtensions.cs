namespace TvShow.Importer
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> InBatches<T>(
            this IEnumerable<T> source,
            int batchSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batchSize == batch.Count)
                {
                    yield return batch.ToList();
                    batch.Clear();
                }
            }

            if (batch.Any())
            {
                yield return batch.ToList();
            }
        }
    }
}
