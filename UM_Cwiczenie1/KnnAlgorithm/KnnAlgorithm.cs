using Cwiczenie1.Entities;

namespace Cwiczenie1.KnnAlgorithm {
    internal class KnnAlgorithm {

        public string? Classify(IEnumerable<Entity> trainingSet, Entity testEntity, int k) {
            List<Tuple<double, Entity>> distances = new();

            foreach (Entity trainingEntity in trainingSet) {
                double distance = CalculateDistance(trainingEntity, testEntity);
                distances.Add(new Tuple<double, Entity>(distance, trainingEntity));
            }

            distances.Sort((a, b) => a.Item1.CompareTo(b.Item1));

            List<Entity> nearestNeighbors = distances
                .Take(k)
                .Select(x => x.Item2)
                .ToList();

            return nearestNeighbors
                .GroupBy(x => x.DecisionAttribute)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key)
                .FirstOrDefault();
        }

        private double CalculateDistance(Entity entity1, Entity entity2) {
            double sum = 0;
            for (int i = 0; i < entity1.Attributes.Count; i++) {
                MyAttribute attr1 = entity1.Attributes[i];
                MyAttribute attr2 = entity2.Attributes[i];

                if (attr1.Name != attr2.Name) {
                    throw new ArgumentException("Attribute names do not match.");
                }

                if (attr1.Value == null || attr2.Value == null) {
                    throw new ArgumentException("Attribute values are null.");
                }

                double diff = 0;
                if (double.TryParse(attr1.Value.ToString(), out double attr1vd) || int.TryParse(attr1.Value.ToString(), out int attr1vi)) {
                    diff = Convert.ToDouble(attr1.Value) - Convert.ToDouble(attr2.Value);
                } else if (attr1.Value?.ToString().Length == 1) {
                    diff = Convert.ToChar(attr1.Value) - Convert.ToChar(attr2.Value);
                } else {
                    diff = LevenshteinDistance(attr1.Value.ToString(), attr2.Value.ToString());
                }
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        private int LevenshteinDistance(string s, string t) {
            int m = s.Length;
            int n = t.Length;
            int[,] d = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) {
                d[i, 0] = i;
            }

            for (int j = 0; j <= n; j++) {
                d[0, j] = j;
            }

            for (int j = 1; j <= n; j++) {
                for (int i = 1; i <= m; i++) {
                    if (s[i - 1] == t[j - 1]) {
                        d[i, j] = d[i - 1, j - 1];
                    } else {
                        d[i, j] = Math.Min(Math.Min(d[i - 1, j], d[i, j - 1]), d[i - 1, j - 1]) + 1;
                    }
                }
            }
            return d[m, n];
        }
    }
}