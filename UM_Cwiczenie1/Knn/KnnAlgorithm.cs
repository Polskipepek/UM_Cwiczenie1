using Cwiczenie1.Entities;
using UM_Cwiczenie1.Entities;

namespace Cwiczenie1.Knn {
    internal class KnnAlgorithm {
        string[] _nominalNames;
        string[] _ordinalNames;
        string[] _numericNames;
        string[] _binarSNames;
        string[] _binarANames;

        public string? Classify(IEnumerable<Entity> trainingSet, Entity testEntity, int k) {
            List<Tuple<double, Entity>> distances = new();

            Entity dummyEntity = trainingSet.First();

            _nominalNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.Nominalny).Select(x => x.Name).ToArray();
            _ordinalNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.Porzadkowy).Select(x => x.Name).ToArray();
            _numericNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.Numeryczny).Select(x => x.Name).ToArray();
            _binarSNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.BinarnySymetryczny).Select(x => x.Name).ToArray();
            _binarANames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.BinarnyAsymetryczny).Select(x => x.Name).ToArray();

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
            CalculateDistanceForNominalAttributes(entity1, entity2, out double nominalDistance, out int nominalOmega);
            CalculateDistanceForOrdinalAttributes(entity1, entity2, out double oridnalDistance, out int ordinalOmega);
            CalculateDistanceForNumericAttributes(entity1, entity2, out double numericDistance, out int numericOmega);
            CalculateDistanceForBinarySymetrical(entity1, entity2, out double binarySDistance, out int binarySOmega);
            CalculateDistanceForBinaryAsymetrical(entity1, entity2, out double binaryADistance, out int binaryAOmega);

            return ((nominalOmega * nominalDistance) + (ordinalOmega * oridnalDistance) + (numericOmega * numericDistance) + (binarySOmega * binarySDistance) + (binaryAOmega * binaryADistance))
                / (double)(nominalOmega + ordinalOmega + numericOmega + binarySOmega + binaryAOmega);
        }

        private void CalculateDistanceForBinaryAsymetrical(Entity entity1, Entity entity2, out double binaryADistance, out int binaryAOmega) {
            int q2 = 0;
            int r2 = 0;
            int s2 = 0;
            int binaryAEmpty = 0;
            foreach (string name in _binarANames) {
                bool value1 = Convert.ToBoolean(entity1.Attributes.FirstOrDefault(x => x.Name == name)?.Value);
                bool value2 = Convert.ToBoolean(entity2.Attributes.FirstOrDefault(x => x.Name == name)?.Value);

                if (value1 && value2) q2++;
                else if (value1 && !value2) r2++;
                else if (!value1 && value2) s2++;
                else binaryAEmpty++;
            }

            binaryADistance = Math.Round((r2 + s2) / (double)((q2 + r2 + s2) == 0 ? 1 : (q2 + r2 + s2)), 3);
            binaryAOmega = _binarANames.Length - binaryAEmpty;
        }

        private void CalculateDistanceForBinarySymetrical(Entity entity1, Entity entity2, out double binarySDistance, out int binarySOmega) {
            int q1 = 0;
            int r1 = 0;
            int s1 = 0;
            int t1 = 0;
            foreach (string name in _binarSNames) {
                bool value1 = Convert.ToBoolean(entity1.Attributes.FirstOrDefault(x => x.Name == name)?.Value?.ToString().Replace("1", "true").Replace("0", "false"));
                bool value2 = Convert.ToBoolean(entity2.Attributes.FirstOrDefault(x => x.Name == name)?.Value?.ToString().Replace("1", "true").Replace("0", "false"));

                if (value1 && value2) q1++;
                else if (value1 && !value2) r1++;
                else if (!value1 && value2) s1++;
                else t1++;
            }

            binarySDistance = Math.Round((r1 + s1) / (double)(q1 + r1 + s1 + t1), 3);
            binarySOmega = _binarSNames.Length;
        }

        private void CalculateDistanceForNumericAttributes(Entity entity1, Entity entity2, out double numericDistance, out int numericOmega) {
            numericDistance = 0;
            numericOmega = 0;
            foreach (string name in _numericNames) {
                MyAttribute attr1 = entity1.Attributes.FirstOrDefault(x => x.Name == name);
                MyAttribute attr2 = entity2.Attributes.FirstOrDefault(x => x.Name == name);
                if (attr1 != null && attr2 != null) {
                    numericDistance += EuklidesDistance(new List<double> { Convert.ToDouble(attr1.Value?.ToString().Replace(".", ",")) }, new List<double> { Convert.ToDouble(attr2.Value?.ToString().Replace(".", ",")) })
                        / (attr1.Max - attr1.Min);
                    numericOmega++;
                }
            }
            //podobnie jak w [1] nie wiem czy to dobrze, że dzielę tutaj (inaczej mogę wyjść powyżej wartości 1)
            numericDistance = Math.Round(numericDistance / Math.Sqrt(numericOmega), 3);
        }

        private void CalculateDistanceForOrdinalAttributes(Entity entity1, Entity entity2, out double oridnalDistance, out int ordinalOmega) {
            List<double> oridnalValuesEntity1 = new();
            List<double> oridnalValuesEntity2 = new();

            //normalizacja
            double oridnalDistance2 = 0;
            foreach (string name in _ordinalNames) {
                MyAttribute attr1 = entity1.Attributes.FirstOrDefault(x => x.Name == name);
                MyAttribute attr2 = entity2.Attributes.FirstOrDefault(x => x.Name == name);
                if (attr1 != null && attr2 != null) {
                    oridnalValuesEntity1.Add((Convert.ToDouble(attr1.Value?.ToString().Replace(".", ",")) - 1) / (Convert.ToDouble(attr1.ValuesNum) - 1));
                    oridnalValuesEntity2.Add((Convert.ToDouble(attr2.Value?.ToString().Replace(".", ",")) - 1) / (Convert.ToDouble(attr2.ValuesNum) - 1));
                }
            }

            oridnalDistance = Math.Round(EuklidesDistance(oridnalValuesEntity1, oridnalValuesEntity2) / Math.Sqrt(oridnalValuesEntity1.Count), 3);
            ordinalOmega = oridnalValuesEntity1.Count;
        }

        private void CalculateDistanceForNominalAttributes(Entity entity1, Entity entity2, out double nominalDistance, out int nominalOmega) {
            int nominalMatch = 0;
            foreach (string name in _nominalNames) {
                if (entity1.Attributes.FirstOrDefault(x => x.Name == name)?.Value == entity2.Attributes.FirstOrDefault(x => x.Name == name)?.Value) nominalMatch++;
            }

            nominalDistance = Math.Round(Convert.ToDouble(_nominalNames.Length - nominalMatch) / Convert.ToDouble(_nominalNames.Length), 3);
            nominalOmega = nominalMatch;
        }

        private double EuklidesDistance(List<double> valuesEntity1, List<double> valuesEntity2) {
            if (valuesEntity1.Count != valuesEntity2.Count)
                throw new ArgumentException("Vectors must have the same length");

            double result = 0;
            for (int i = 0; i < valuesEntity1.Count; i++) {
                result += Math.Pow(valuesEntity1[i] - valuesEntity2[i], 2);
            }

            return Math.Sqrt(result);
        }

        private double ManhattanDistance(List<double> valuesEntity1, List<double> valuesEntity2) {
            if (valuesEntity1.Count != valuesEntity2.Count)
                throw new ArgumentException("Vectors must have the same length");

            double distance = 0;
            for (int k = 0; k < valuesEntity1.Count; k++) {
                distance += Math.Abs(valuesEntity1[k] - valuesEntity2[k]);
            }
            return distance;
        }

        private double MinkowskiDistance(List<double> valuesEntity1, List<double> valuesEntity2, NormaMinkowskiego p) {
            if (valuesEntity1.Count != valuesEntity2.Count)
                throw new ArgumentException("Vectors must have the same length"); double distance = 0;

            for (int k = 0; k < valuesEntity1.Count; k++) {
                distance += Math.Pow(Math.Abs(valuesEntity1[k] - valuesEntity2[k]), (int)p);
            }
            distance = Math.Pow(distance, 1.0 / (int)p);
            return distance;
        }

        private double ChebyshevDistance(List<double> valuesEntity1, List<double> valuesEntity2) {
            if (valuesEntity1.Count != valuesEntity2.Count)
                throw new ArgumentException("Vectors must have the same length");

            double maxDiff = 0.0;

            for (int i = 0; i < valuesEntity1.Count; i++) {
                double diff = Math.Abs(valuesEntity1[i] - valuesEntity2[i]);
                if (diff > maxDiff)
                    maxDiff = diff;
            }

            return maxDiff;
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