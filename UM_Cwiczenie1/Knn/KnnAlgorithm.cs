﻿using Cwiczenie1.Entities;
using UM_Cwiczenie1.Entities;
using UM_Cwiczenie1.Knn;

namespace Cwiczenie1.Knn {
    internal class KnnAlgorithm {
        string[]? _nominalNames;
        string[]? _ordinalNames;
        string[]? _numericNames;
        string[]? _binarSNames;
        string[]? _binarANames;

        public string? Classify(IEnumerable<Entity> trainingSet, Entity testEntity, int k, MeasureType measureType = MeasureType.Euklides, NormaMinkowskiego p = NormaMinkowskiego.Euklides) {
            List<Tuple<double, Entity>> distances = new();

            Entity dummyEntity = trainingSet.First();

            _nominalNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.Nominalny).Select(x => x.Name).ToArray();
            _ordinalNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.Porzadkowy).Select(x => x.Name).ToArray();
            _numericNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.Numeryczny).Select(x => x.Name).ToArray();
            _binarSNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.BinarnySymetryczny).Select(x => x.Name).ToArray();
            _binarANames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.BinarnyAsymetryczny).Select(x => x.Name).ToArray();

            foreach (Entity trainingEntity in trainingSet) {
                double distance = CalculateDistance(trainingEntity, testEntity, measureType, p);
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

        private double CalculateDistance(Entity entity1, Entity entity2, MeasureType measureType, NormaMinkowskiego p) {
            CalculateDistanceForNominalAttributes(entity1, entity2, out double nominalDistance, out int nominalDelta);
            CalculateDistanceForOrdinalAttributes(entity1, entity2, out double oridnalDistance, out int ordinalDelta, measureType, p);
            CalculateDistanceForNumericAttributes(entity1, entity2, out double numericDistance, out int numericDelta, measureType, p);
            CalculateDistanceForBinarySymetrical(entity1, entity2, out double binarySDistance, out int binarySDelta);
            CalculateDistanceForBinaryAsymetrical(entity1, entity2, out double binaryADistance, out int binaryADelta);

            if (numericDistance > 1 || oridnalDistance > 1) {
                System.Diagnostics.Debug.WriteLine("One of distance wasn't normalized!");
            }

            return ((nominalDelta * nominalDistance) + (ordinalDelta * oridnalDistance) + (numericDelta * numericDistance) + (binarySDelta * binarySDistance) + (binaryADelta * binaryADistance))
                / (double)(nominalDelta + ordinalDelta + numericDelta + binarySDelta + binaryADelta);
        }

        private void CalculateDistanceForBinaryAsymetrical(Entity entity1, Entity entity2, out double binaryADistance, out int binaryADelta) {
            int q2 = 0;
            int r2 = 0;
            int s2 = 0;
            int binaryAEmpty = 0;
            foreach (string name in _binarANames!) {
                bool value1 = Convert.ToBoolean(entity1.Attributes.FirstOrDefault(x => x.Name == name)?.Value);
                bool value2 = Convert.ToBoolean(entity2.Attributes.FirstOrDefault(x => x.Name == name)?.Value);

                if (value1 && value2) q2++;
                else if (value1 && !value2) r2++;
                else if (!value1 && value2) s2++;
                else binaryAEmpty++;
            }

            binaryADistance = Math.Round((r2 + s2) / (double)((q2 + r2 + s2) == 0 ? 1 : (q2 + r2 + s2)), 3);
            binaryADelta = _binarANames.Length - binaryAEmpty;
        }

        private void CalculateDistanceForBinarySymetrical(Entity entity1, Entity entity2, out double binarySDistance, out int binarySDelta) {
            int q1 = 0;
            int r1 = 0;
            int s1 = 0;
            int t1 = 0;
            foreach (string name in _binarSNames!) {
                bool value1 = Convert.ToBoolean(entity1.Attributes.First(x => x.Name == name)?.Value?.ToString()?.Replace("1", "true").Replace("0", "false"));
                bool value2 = Convert.ToBoolean(entity2.Attributes.First(x => x.Name == name)?.Value?.ToString()?.Replace("1", "true").Replace("0", "false"));

                if (value1 && value2) q1++;
                else if (value1 && !value2) r1++;
                else if (!value1 && value2) s1++;
                else t1++;
            }

            binarySDistance = Math.Round((r1 + s1) / (double)(q1 + r1 + s1 + t1), 3);
            binarySDelta = _binarSNames.Length;
        }

        private void CalculateDistanceForNumericAttributes(Entity entity1, Entity entity2, out double numericDistance, out int numericDelta, MeasureType measureType, NormaMinkowskiego p) {
            numericDistance = 0;
            numericDelta = 0;
            foreach (string name in _numericNames!) {
                MyAttribute attr1 = entity1.Attributes.First(x => x.Name == name);
                MyAttribute attr2 = entity2.Attributes.First(x => x.Name == name);
                if (attr1 != null && attr2 != null) {
                    numericDistance += DistanceForMeasureType(new List<double> { (Convert.ToDouble(attr1.Value?.ToString()?.Replace(".", ",")) - attr1.Min) / (attr1.Max - attr1.Min) }, new List<double> { (Convert.ToDouble(attr2.Value?.ToString()?.Replace(".", ",")) - attr1.Min) / (attr1.Max - attr1.Min) }, measureType, p);
                    numericDelta++;
                }
            }
            numericDistance = Math.Round(numericDistance / DividerValueForMeasureType(numericDelta, measureType, p), 3);
        }

        private void CalculateDistanceForOrdinalAttributes(Entity entity1, Entity entity2, out double oridnalDistance, out int ordinalDelta, MeasureType measureType, NormaMinkowskiego p) {
            List<double> oridnalValuesEntity1 = new();
            List<double> oridnalValuesEntity2 = new();

            foreach (string name in _ordinalNames!) {
                MyAttribute attr1 = entity1.Attributes.First(x => x.Name == name);
                MyAttribute attr2 = entity2.Attributes.First(x => x.Name == name);
                if (attr1 != null && attr2 != null) {
                    oridnalValuesEntity1.Add((Convert.ToDouble(attr1.Value?.ToString()?.Replace(".", ",")) - 1) / (Convert.ToDouble(attr1.ValuesNum) - 1));
                    oridnalValuesEntity2.Add((Convert.ToDouble(attr2.Value?.ToString()?.Replace(".", ",")) - 1) / (Convert.ToDouble(attr2.ValuesNum) - 1));
                }
            }

            oridnalDistance = Math.Round(DistanceForMeasureType(oridnalValuesEntity1, oridnalValuesEntity2, measureType, p) / DividerValueForMeasureType(oridnalValuesEntity1.Count, measureType, p), 3);
            ordinalDelta = oridnalValuesEntity1.Count;
        }

        private void CalculateDistanceForNominalAttributes(Entity entity1, Entity entity2, out double nominalDistance, out int nominalDelta) {
            int nominalMatch = 0;
            foreach (string name in _nominalNames!) {
                if (entity1.Attributes.FirstOrDefault(x => x.Name == name)?.Value == entity2.Attributes.FirstOrDefault(x => x.Name == name)?.Value) nominalMatch++;
            }

            nominalDistance = Math.Round(Convert.ToDouble(_nominalNames.Length - nominalMatch) / Convert.ToDouble(_nominalNames.Length), 3);
            nominalDelta = nominalMatch;
        }

        private static double DistanceForMeasureType(List<double> valuesEntity1, List<double> valuesEntity2, MeasureType measureType, NormaMinkowskiego p) {
            double distance = 0;
            switch (measureType) {
                case MeasureType.Euklides:
                    distance = EuklidesDistance(valuesEntity1, valuesEntity2);
                    break;
                case MeasureType.Manhattan:
                    distance = ManhattanDistance(valuesEntity1, valuesEntity2);
                    break;
                case MeasureType.Minkowski:
                    distance = MinkowskiDistance(valuesEntity1, valuesEntity2, p);
                    break;
                case MeasureType.Chebyshev:
                    distance = ChebyshevDistance(valuesEntity1, valuesEntity2);
                    break;
            }
            return distance;
        }

        private static double DividerValueForMeasureType(double value, MeasureType measureType, NormaMinkowskiego p) {
            double divider = 0;
            switch (measureType) {
                case MeasureType.Euklides:
                    divider = Math.Sqrt(value);
                    break;
                case MeasureType.Manhattan:
                    divider = Math.Abs(value);
                    break;
                case MeasureType.Minkowski:
                    divider = Math.Pow(value, 1.0 / (double)p);
                    break;
                case MeasureType.Chebyshev:
                    divider = Math.Abs(value);
                    break;
            }
            return divider;
        }

        private static double EuklidesDistance(List<double> valuesEntity1, List<double> valuesEntity2) {
            if (valuesEntity1.Count != valuesEntity2.Count)
                throw new ArgumentException("Vectors must have the same length");

            double result = 0;
            for (int i = 0; i < valuesEntity1.Count; i++) {
                result += Math.Pow(valuesEntity1[i] - valuesEntity2[i], 2);
            }

            return Math.Sqrt(result);
        }

        private static double ManhattanDistance(List<double> valuesEntity1, List<double> valuesEntity2) {
            if (valuesEntity1.Count != valuesEntity2.Count)
                throw new ArgumentException("Vectors must have the same length");

            double distance = 0;
            for (int k = 0; k < valuesEntity1.Count; k++) {
                distance += Math.Abs(valuesEntity1[k] - valuesEntity2[k]);
            }
            return distance;
        }

        private static double MinkowskiDistance(List<double> valuesEntity1, List<double> valuesEntity2, NormaMinkowskiego p) {
            if (valuesEntity1.Count != valuesEntity2.Count)
                throw new ArgumentException("Vectors must have the same length"); double distance = 0;

            for (int k = 0; k < valuesEntity1.Count; k++) {
                distance += Math.Pow(Math.Abs(valuesEntity1[k] - valuesEntity2[k]), (int)p);
            }
            distance = Math.Pow(distance, 1.0 / (int)p);
            return distance;
        }

        private static double ChebyshevDistance(List<double> valuesEntity1, List<double> valuesEntity2) {
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
    }
}