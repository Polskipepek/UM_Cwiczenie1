using Cwiczenie1.Entities;
using Cwiczenie1.Knn;
using UM_Cwiczenie1.Entities;
using UM_Cwiczenie1.Entities.Mappers;
using UM_Cwiczenie1.Knn;

Console.WriteLine("Hello, Uczenie Maszynowe!");
Console.WriteLine("Cwiczenie 1!");
Console.WriteLine();

if (!File.Exists($"{Environment.CurrentDirectory}/Data/HearthDiseaseDataSet.csv")) {
    Console.WriteLine("File with data was not found.");
    return;
}
var data = TableToHearthEntitiesMapper.Map(DataReader.ReadData($"{Environment.CurrentDirectory}/Data/HearthDiseaseDataSet.csv", ";"));

int numRow = GetInputNumber("Enter number of test rows ", 1, data.Count());

List<Entity> testSet = data.ToList();
List<Entity> trainingSet = new();
KnnAlgorithm kNNInstance = new();
Random rnd = new();

for (int i = 0; i < numRow; i++) {
    int random = rnd.Next(testSet.Count);
    trainingSet.Add(testSet[random]);
    testSet.RemoveAt(random);
}

while (true) {
    int correctPredictions = 0;
    int function = GetKeyNumber("1. Custom K.\r\n2. Best K Finder.\r\nEnter function index: ", 1, 2, true);

    switch (function) {
        case 1:
            int k = GetInputNumber("Enter value for k", 1, data.Count());
            int measureType = GetKeyNumber("1. Euklides\r\n2. Manhattan\r\n3. Minkowski\r\n4. Chebyshev\r\nEnter measure type index:", 1, 4);
            int minkowskiNorm = 2;
            if (measureType == (int)MeasureType.Minkowski) minkowskiNorm = GetKeyNumber("1. Manhattan\r\n2. Euklides\r\nEnter Minkowski norm index:", 1, 2);

            foreach (Entity testInstance in testSet) {
                var predictedClass = kNNInstance.Classify(trainingSet, testInstance, k, (MeasureType)measureType, (NormaMinkowskiego)minkowskiNorm);
                testInstance.PredictedAttribute = predictedClass ?? "";

                if (predictedClass == testInstance.DecisionAttribute) correctPredictions++;
            }

            PrintPredictions(k, testSet, correctPredictions, testSet.Select(e => e.PredictedAttribute).ToList());

            var cm = CalculateConfusionMatrix(testSet, out string[] cmLabels);
            PrintConfusionMatrix(cmLabels, cm);
            break;
        case 2:
            Console.WriteLine("Best K Finder");
            int maxK = GetInputNumber("Enter max K:", 1, data.Count(), false);
            int subFunction = GetKeyNumber("1. Custom Measure.\r\n2. Best Measure Finder (It'll take a while).\r\nEnter function index: ", 1, 2, true);

            int globalBestK = -1;
            double globalBestAcc = -1d;
            long globalElapedMs = 0;

            switch (subFunction) {
                case 1:
                    measureType = GetKeyNumber("1. Euklides\r\n2. Manhattan\r\n3. Minkowski\r\n4. Chebyshev\r\nEnter measure type index:", 1, 4);
                    globalBestK = OptimalKfinder.FindBestK(trainingSet, testSet, 1, maxK, out globalBestAcc, out globalElapedMs, (MeasureType)measureType);
                    break;
                case 2:
                    foreach (MeasureType measure in Enum.GetValues(typeof(MeasureType))) {
                        int localBestK = OptimalKfinder.FindBestK(trainingSet, testSet, 1, maxK, out double localbestAcc, out long elapsedMs, measure);
                        if (measure == MeasureType.Minkowski) {
                            CheckIfKBetter(ref globalBestK, ref globalBestAcc, ref globalElapedMs, localBestK, localbestAcc, elapsedMs);
                            localBestK = OptimalKfinder.FindBestK(trainingSet, testSet, 1, maxK, out localbestAcc, out elapsedMs, measure, NormaMinkowskiego.Manhattan);

                        }
                        CheckIfKBetter(ref globalBestK, ref globalBestAcc, ref globalElapedMs, localBestK, localbestAcc, elapsedMs);
                    }
                    break;
                default:
                    continue;
            }

            PrintBestK(globalBestK, globalBestAcc, globalElapedMs, 1, maxK);
            break;
    }

    Console.ReadKey();
    Console.WriteLine();
}

static void PrintBestK(int bestK, double bestAcc, long elapsedMs, int minK = 1, int maxK = 10, MeasureType measureType = MeasureType.Euklides, NormaMinkowskiego minkowskiego = NormaMinkowskiego.Manhattan) {
    Console.WriteLine();
    Console.WriteLine($"Best K is: {bestK} with {bestAcc:P}% - it took {elapsedMs / 1000} seconds to find. (1-{maxK} range)");
    Console.WriteLine($"Measure Type: {measureType}{(measureType == MeasureType.Minkowski ? $", Norma Minkowskiego: {minkowskiego}" : ".")}");
}

static double[,] CalculateConfusionMatrix(List<Entity> testSet, out string[] cmLabels) {
    cmLabels = testSet.Select(x => x.DecisionAttribute).Distinct().ToArray();
    return ConfusionMatrix.Calculate(testSet);
}

static void PrintConfusionMatrix(string[] cmLabels, double[,] cm) {
    Console.WriteLine();
    Console.WriteLine("Confision Matrix");
    Console.WriteLine(string.Join("\t", cmLabels.Prepend("X")));
    for (int i = 0; i < cmLabels.Length; i++) {
        Console.Write($"{cmLabels[i]}:\t");
        for (int j = 0; j < cmLabels.Length; j++) {
            Console.Write($"{cm[i, j]:P}\t");
        }
        Console.WriteLine();
    }
    Console.WriteLine();
}

static void PrintPredictions(int k, List<Entity> testSet, int correctPredictions, List<string> predictions) {
    double accuracy = (double)correctPredictions / testSet.Count;
    Console.WriteLine($"Predictions: for k={k}");
    Console.WriteLine($"Accuracy: {accuracy:P}");
    Console.WriteLine();

    foreach (var c in predictions.GroupBy(c => c)) {
        Console.WriteLine($"Predicted: {c.Key} - {predictions.Where(p => p.Equals(c.Key)).Count()} times");
    }
}

static int GetInputNumber(string title, int min, int max, bool clearAfter = true) {
    int numRow = -1;
    Console.WriteLine(title);
    while (!int.TryParse(Console.ReadLine(), out numRow) || numRow < min || numRow > max) {
        Console.Clear();
        Console.WriteLine("You entered an invalid number");
        Console.Write(title);
    }
    if (clearAfter) Console.Clear();
    return numRow;
}

static int GetKeyNumber(string title, int min, int max, bool clearAfter = true) {
    int numRow = -1;
    Console.WriteLine(title);
    while (!int.TryParse(Console.ReadKey().KeyChar.ToString(), out numRow) || numRow < min || numRow > max) {
        Console.Clear();
        Console.WriteLine("You entered an invalid number");
        Console.Write(title);
    }
    if (clearAfter) Console.Clear();
    return numRow;
}

static void CheckIfKBetter(ref int globalBestK, ref double globalBestAcc, ref long globalElapedMs, int localBestK, double localbestAcc, long elapsedMs) {
    globalElapedMs += elapsedMs;
    if (localbestAcc > globalBestAcc) {
        globalBestK = localBestK;
        globalBestAcc = localbestAcc;
    }
}