using Cwiczenie1.Entities;
using Cwiczenie1.Knn;
using UM_Cwiczenie1.Entities.Mappers;
using UM_Cwiczenie1.Knn;

Console.WriteLine("Hello, Uczenie Maszynowe!");
Console.WriteLine("Cwiczenie 1!");
Console.WriteLine();

while (true) {
    if (!File.Exists($"{Environment.CurrentDirectory}/Data/HearthDiseaseDataSet.csv")) {
        Console.WriteLine("File with data was not found.");
        return;
    }
    var data = TableToHearthEntitiesMapper.Map(DataReader.ReadData($"{Environment.CurrentDirectory}/Data/HearthDiseaseDataSet.csv", ";"));

    int k = GetInputNumber("Enter value for k", 1, data.Count());
    int numRow = GetInputNumber("Enter number of test rows ", 1, data.Count());

    List<Entity> testSet = data.ToList();
    List<Entity> trainingSet = new();
    KnnAlgorithm kNNInstance = new();
    Random rnd = new();
    int correctPredictions = 0;

    for (int i = 0; i < numRow; i++) {
        int random = rnd.Next(testSet.Count);
        trainingSet.Add(testSet[random]);
        testSet.RemoveAt(random);
    }

    foreach (Entity testInstance in testSet) {
        var predictedClass = kNNInstance.Classify(trainingSet, testInstance, k);
        testInstance.PredictedAttribute = predictedClass;

        if (predictedClass == testInstance.DecisionAttribute) correctPredictions++;
    }

    PrintPredictions(k, testSet, correctPredictions, testSet.Select(e => e.PredictedAttribute).ToList());

    var cm = CalculateConfusionMatrix(testSet, out string[] cmLabels);
    PrintConfusionMatrix(cmLabels, cm);


    Console.WriteLine("Best K Finder");
    int maxK = GetInputNumber("Enter max K:", 1, data.Count(), false);
    var bestK = OptimalKfinder.FindBestK(trainingSet, testSet, 1, maxK, out double bestAcc, out long elapsedMs);
    PrintBestK(bestK, bestAcc, elapsedMs, 1, maxK);

    Console.ReadLine();
    Console.WriteLine();
}

static void PrintBestK(int bestK, double bestAcc, long elapsedMs, int minK = 1, int maxK = 10) {
    Console.WriteLine();
    Console.WriteLine($"Best K is: {bestK} with {bestAcc:P}% - it took {elapsedMs / 1000} seconds to find. (1-10 range)");
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
    Console.WriteLine($"Accuracy: {accuracy:P}%");
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