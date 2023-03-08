using System.Data;

public static class DataReader {
    public static DataTable ReadData(string path, string delimeter = ",") {

        DataTable dt = new();
        using StreamReader sr = new(path);
        if (sr.Peek() == -1)
            throw new Exception("OutOfBoundsExeption.");

        string[] headers = sr.ReadLine()?.Split(delimeter) ?? Array.Empty<string>();
        foreach (string header in headers) {
            dt.Columns.Add(header);
        }

        while (!sr.EndOfStream) {
            string[] rows = sr.ReadLine()?.Split(delimeter) ?? Array.Empty<string>();
            DataRow dr = dt.NewRow();
            for (int i = 0; i < headers.Length; i++) {
                dr[i] = rows[i];
            }
            dt.Rows.Add(dr);
        }

        return dt;
    }
}
