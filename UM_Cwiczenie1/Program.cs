using Cwiczenie1.Entities;
using Cwiczenie1.Entities.Mappers;
using Cwiczenie1.KnnAlgorithm;
using UM_Cwiczenie1.Entities;
using UM_Cwiczenie1.Entities.Mappers;

Console.WriteLine("Hello, Uczenie Maszynowe!");
Console.WriteLine("Cwiczenie 1!");
Console.WriteLine();

while (true) {
    if (!File.Exists($"{Environment.CurrentDirectory}/Data/HearthDiseaseDataSet.csv")) {
        Console.WriteLine("File with data was not found.");
        return;
    }

    var data = TableToHearthEntitiesMapper.Map(DataReader.ReadData($"{Environment.CurrentDirectory}/Data/HearthDiseaseDataSet.csv", ";"));

    int k = -1;
    Console.WriteLine("Select k:");
    while (!int.TryParse(Console.ReadLine(), out k) || k < 1 || k > data.Count()) {
        Console.Clear();
        Console.WriteLine("You entered an invalid number");
        Console.Write("Enter number of k ");
    }
    Console.Clear();

    var kNNInstance = new KnnAlgorithm();


    int numRow = -1;
    Console.WriteLine("Select number of test rows:");
    while (!int.TryParse(Console.ReadLine(), out numRow) || numRow < 1 || numRow > data.Count()) {
        Console.Clear();
        Console.WriteLine("You entered an invalid number");
        Console.Write("Enter number of test rows ");
    }
    Console.Clear();

    List<Entity> testSet = data.ToList();
    List<Entity> trainingSet = new();
    Random rnd = new();
    for (int i = 0; i < numRow; i++) {
        int random = rnd.Next(testSet.Count);
        trainingSet.Add(testSet[random]);
        testSet.RemoveAt(random);
    }

    int correctPredictions = 0;
    List<string> predictions = new();

    foreach (Entity testInstance in testSet) {
        var predictedClass = kNNInstance.Classify(trainingSet, testInstance, k);
        predictions.Add(predictedClass);
    }
    double accuracy = (double)correctPredictions / testSet.Count;
    //Console.WriteLine("Accuracy: " + accuracy.ToS2tring("P"));
    Console.WriteLine($"Predictions: for k={k}");
    foreach (var c in predictions.GroupBy(c => c)) {
        Console.WriteLine($"Predicted: {c.Key} - {predictions.Where(p => p.Equals(c.Key)).Count()} times");
    }
    Console.ReadLine();
    Console.WriteLine();
}

static IEnumerable<Entity> GetData(ConsoleKey key) {
    if (key == ConsoleKey.D2) {
        var data = DataReader.ReadData($"{Environment.CurrentDirectory}/Data/winequality-red.csv", ";");
        return TableToEntitiesMapper.Map(data);
    } else if (key == ConsoleKey.D3) {
        var data = DataReader.ReadData($"{Environment.CurrentDirectory}/Data/winequality-white.csv", ";");
        return TableToEntitiesMapper.Map(data);
    } else if (key == ConsoleKey.D4) {
        var data = DataReader.ReadData($"{Environment.CurrentDirectory}/Data/mushrooms.csv", ";");
        return TableToEntitiesMapper.Map(data);
    } else if (key == ConsoleKey.D0) {
        Console.Clear();
        Console.WriteLine("Decision attribute column name needs to be named \'decision\'");
        Console.WriteLine("Enter full path to the file. c:/users/alfa/desktop/dummydata.csv");
        var path = Console.ReadLine();
        Console.WriteLine("Enter delimeter type: (for example: \',\', \';\')");
        var delimeter = Console.ReadKey();
        var data = DataReader.ReadData(path, delimeter.KeyChar.ToString());
        return TableToEntitiesMapper.Map(data);
    } else if (key == ConsoleKey.D1) {
        var data = DataReader.ReadData($"{Environment.CurrentDirectory}/Data/example.csv", ";");
        return TableToEntitiesMapper.Map(data);
    } else {
        return null;
    }
}

static void NormalizeAttribute(IEnumerable<Entity> entities, string prop) {
    List<double> values = new();
    foreach (var attribute in entities.SelectMany(e => e.Attributes).Where(a => a.Name == prop)) {
        values.Add((attribute.Value as IConvertible).ToDouble(null));
    }

    double min = values.Min();
    double max = values.Max();

    foreach (Entity entity in entities) {
        MyAttribute? attribute = entity.Attributes.FirstOrDefault(a => a.Name == prop);
        if (attribute != null && attribute.Value is double value) {
            double normalizedValue = (value - min) / (max - min);
            entity.Attributes.Remove(attribute);
            entity.Attributes.Add(new MyAttribute(prop, normalizedValue,/*TODO*/ AttributeType.Nominalny));
        }
    }
}
