using Cwiczenie1.Entities;
using UM_Cwiczenie1.Entities;

namespace Cwiczenie1.KnnAlgorithm {
    internal class KnnAlgorithm {
        string[] _nominalNames;
        string[] _ordinalNames;
        string[] _numericNames;
        string[] _binarSNames;
        string[] _binarANames;

        public string? Classify(IEnumerable<Entity> trainingSet, Entity testEntity, int k) {
            List<Tuple<double, Entity>> distances = new();

            Entity dummyEntity=trainingSet.First();

            _nominalNames = dummyEntity.Attributes.Where(x => x.AttributeType == AttributeType.Nominalny).Select(x=>x.Name).ToArray();
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
            double sum = 0;

            //nominalne
            int nominalMatch = 0;
            foreach(string name in _nominalNames)
            {
                if(entity1.Attributes.FirstOrDefault(x=>x.Name==name)?.Value == entity2.Attributes.FirstOrDefault(x => x.Name == name)?.Value) nominalMatch++;
            }

            double nominalDistance = Math.Round(Convert.ToDouble(_nominalNames.Length - nominalMatch) / Convert.ToDouble(_nominalNames.Length), 3);
            int nominalOmega = nominalMatch;

            //porządkowe
            List<double> oridnalValuesEntity1 = new List<double>();
            List<double> oridnalValuesEntity2 = new List<double>();

            //normalizacja
            double oridnalDistance2 = 0;
            foreach (string name in _ordinalNames)
            {
                MyAttribute attr1 = entity1.Attributes.FirstOrDefault(x => x.Name == name);
                MyAttribute attr2 = entity2.Attributes.FirstOrDefault(x => x.Name == name);
                if(attr1 != null && attr2 != null)
                {
                    oridnalValuesEntity1.Add((Convert.ToDouble(attr1.Value?.ToString().Replace(".", ",")) - 1) / (Convert.ToDouble(attr1.ValuesNum) - 1));
                    oridnalValuesEntity2.Add((Convert.ToDouble(attr2.Value?.ToString().Replace(".", ",")) - 1) / (Convert.ToDouble(attr2.ValuesNum) - 1));
                }
            }

            //[1] tutaj trzeba pomyśleć nad tym wzorem Euklidesa, kiedy są znormalizowane dane, bo....sqrt(1+1)>1 :/
            double oridnalDistance = Math.Round(EuklidesDistance(oridnalValuesEntity1, oridnalValuesEntity2)/Math.Sqrt(oridnalValuesEntity1.Count()), 3);
            int ordinalOmega = oridnalValuesEntity1.Count();

            //numeryczne
            double numericDistance = 0;
            int numericOmega = 0;
            foreach (string name in _numericNames)
            {
                MyAttribute attr1 = entity1.Attributes.FirstOrDefault(x => x.Name == name);
                MyAttribute attr2 = entity2.Attributes.FirstOrDefault(x => x.Name == name);
                if (attr1 != null && attr2 != null)
                {
                    numericDistance += EuklidesDistance(new List<double> {Convert.ToDouble(attr1.Value?.ToString().Replace(".", ",")) }, new List<double> {Convert.ToDouble(attr2.Value?.ToString().Replace(".", ",")) })
                        / (attr1.Max- attr1.Min);
                    numericOmega++;
                }
            }
            //podobnie jak w [1] nie wiem czy to dobrze, że dzielę tutaj (inaczej mogę wyjść powyżej wartości 1)
            numericDistance= Math.Round(numericDistance /Math.Sqrt(numericOmega), 3);


            //binarne symetryczne
            int q1 = 0;
            int r1 = 0;
            int s1 = 0;
            int t1 = 0;
            foreach (string name in _binarSNames)
            {
                bool value1 = Convert.ToBoolean(entity1.Attributes.FirstOrDefault(x => x.Name == name)?.Value?.ToString().Replace("1", "true").Replace("0", "false"));
                bool value2 = Convert.ToBoolean(entity2.Attributes.FirstOrDefault(x => x.Name == name)?.Value?.ToString().Replace("1", "true").Replace("0", "false"));

                if (value1 && value2) q1++;
                else if (value1 && !value2) r1++;
                else if (!value1 && value2) s1++;
                else t1++;
            }

            double binarySDistance = Math.Round((r1 + s1) / (double) (q1 + r1 + s1 + t1), 3);
            int binarySOmega = _binarSNames.Count();

            //binarne asymetryczne
            int q2 = 0;
            int r2 = 0;
            int s2 = 0;
            int binaryAEmpty = 0;
            foreach (string name in _binarANames)
            {
                bool value1 = Convert.ToBoolean(entity1.Attributes.FirstOrDefault(x => x.Name == name)?.Value);
                bool value2 = Convert.ToBoolean(entity2.Attributes.FirstOrDefault(x => x.Name == name)?.Value);

                if (value1 && value2) q2++;
                else if (value1 && !value2) r2++;
                else if (!value1 && value2) s2++;
                else binaryAEmpty++;
            }

            double binaryADistance = Math.Round((r2 + s2) / (double)((q2 + r2 + s2)==0?1:(q2 + r2 + s2)), 3);
            int binaryAOmega = _binarANames.Count() - binaryAEmpty;

            return ((nominalOmega * nominalDistance) + (ordinalOmega * oridnalDistance) + (numericOmega * numericDistance) + (binarySOmega * binarySDistance) + (binaryAOmega * binaryADistance)) 
                / (double)(nominalOmega + ordinalOmega + numericOmega + binarySOmega + binaryAOmega);

            /*
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
            */
        }

        private double EuklidesDistance(List<double> valuesEntity1, List<double> valuesEntity2)
        {
            if (valuesEntity1.Count != valuesEntity2.Count) return -1;
            double result = 0;
            for (int i = 0; i < valuesEntity1.Count; i++)
            {
                result += Math.Pow(valuesEntity1[i] - valuesEntity2[i], 2);
            }
            return Math.Sqrt(result);
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