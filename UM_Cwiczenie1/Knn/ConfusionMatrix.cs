namespace UM_Cwiczenie1.Knn {
    public static class ConfusionMatrix {
        public static double[,] Calculate(string[] trueLabels, string[] predictedLabels) {
            var labels = trueLabels.Distinct().OrderBy(l => l).ToArray();
            int m = labels.Length;

            var matrix = new double[m, m];

            for (int i = 0; i < trueLabels.Length; i++) {
                int actualIndex = Array.IndexOf(labels, trueLabels[i]);
                int predictedIndex = Array.IndexOf(labels, predictedLabels[i]);
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
