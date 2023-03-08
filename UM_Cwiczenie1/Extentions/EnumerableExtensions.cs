namespace Cwiczenie1.Extentions {
    public static class EnumerableExtensions {
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k) {
            return k == 0 ? new[] { Array.Empty<T>() } :
               elements.SelectMany((e, i) =>
                  elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }
    }
}
