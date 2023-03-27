using Cwiczenie1.Entities;

namespace UM_Cwiczenie1.Knn {
    public static class ConfusionMatrix {
        public static double[,] Calculate(List<Entity> entities) {
            var labels = entities.Select(e => e.DecisionAttribute).Distinct().ToArray();
            int m = labels.Length;

            var matrix = new double[m, m];

            for (int i = 0; i < entities.Count; i++) {
                int actualIndex = Array.IndexOf(labels, entities[i].DecisionAttribute);
                int predictedIndex = Array.IndexOf(labels, entities[i].PredictedAttribute);
                matrix[actualIndex, predictedIndex]++;
            }

            for (int i = 0; i < m; i++) {
                double sum = 0;
                for (int j = 0; j < m; j++) {
                    sum += matrix[i, j];
                }
                for (int j = 0; j < m; j++) {
                    matrix[i, j] /= sum;
                }
            }

            return matrix;
        }
    }
}
