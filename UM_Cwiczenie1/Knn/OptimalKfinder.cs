using Cwiczenie1.Entities;
using Cwiczenie1.Knn;

namespace UM_Cwiczenie1.Knn {
    public class OptimalKfinder {

        public static int FindBestK(List<Entity> trainingSet, List<Entity> testInstance, int kMin, int kMax, out double bestAccuracy) {
            int bestK = kMin;
            bestAccuracy = 0;

            for (int k = kMin; k <= kMax; k++) {
                double accuracy = CrossValidate(trainingSet, testInstance, k);

                if (accuracy > bestAccuracy) {
                    bestK = k;
                    bestAccuracy = accuracy;
                }
            }

            return bestK;
        }

        private static double CrossValidate(List<Entity> trainSet, List<Entity> testSet, int k) {
            var classifier = new KnnAlgorithm();
            double sumAccuracy = 0;

            foreach (var testEntity in testSet) {
                int numCorrect = 0;

                for (int i = 0; i < testSet.Count; i++) {
                    string? predictedOutput = classifier.Classify(trainSet, testSet[i], k);

                    if (predictedOutput == testSet[i].DecisionAttribute) {
                        numCorrect++;
                    }
                }

                double accuracy = (double)numCorrect / testSet.Count;

                sumAccuracy += accuracy;
            }

            return sumAccuracy / testSet.Count;
        }
    }
}
