using Cwiczenie1.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UM_Cwiczenie1.Entities.Mappers
{
    public static class TableToHearthEntitiesMapper
    {
        public static IEnumerable<Entity> Map(DataTable table)
        {
            List<Entity> entities = new();
            foreach (DataRow row in table.Rows)
            {
                var entity = new Entity();
                foreach (DataColumn column in table.Columns)
                {
                    if (column.ColumnName.Equals("decision") || column.ColumnName.Equals("\"decision\"")) continue;
                    string nameCol = column.ColumnName;
                    object value = row[nameCol];
                    int valuesNum = -1;

                    int indexType = column.ColumnName.IndexOf("[");
                    AttributeType type = AttributeType.Brak;
                    if (indexType > 0)
                    {
                        //typ danych
                        string typeCol = column.ColumnName.Substring(indexType).Replace("[", "").Replace("]", "");
                        switch (typeCol.ToUpper())
                        {
                            case "NU":
                                type = AttributeType.Numeryczny;
                                break;
                            case "NO":
                                type = AttributeType.Nominalny;
                                break;
                            case "OR":
                                type = AttributeType.Porzadkowy;
                                break;
                            case "BIS":
                                type = AttributeType.BinarnySymetryczny;
                                break;
                            case "BIA":
                                type = AttributeType.BinarnyAsymetryczny;
                                break;
                        }

                        nameCol = nameCol.Substring(0, indexType);

                        //podmiana wartosci oraz dodanie ilości wartości
                        if (type == AttributeType.Porzadkowy)
                        {
                            switch (nameCol.ToLower())
                            {
                                case "cp":
                                    valuesNum = 4;
                                    switch (value.ToString())
                                    {
                                        case "angina":
                                            value = 1;
                                            break;
                                        case "abnang":
                                            value = 2;
                                            break;
                                        case "notang":
                                            value = 3;
                                            break;
                                        case "asympt":
                                            value = 4;
                                            break;
                                    }
                                    break;
                                case "slope":
                                    //może sie przydać jak jednak uznamy, że to porządkowa
                                    valuesNum = 3;
                                    switch (value.ToString())
                                    {
                                        case "up":
                                            value = 1;
                                            break;
                                        case "flat":
                                            value = 2;
                                            break;
                                        case "down":
                                            value = 3;
                                            break;
                                    }
                                    break;
                                case "ca":
                                    valuesNum = 4;
                                    value = Convert.ToDouble(value.ToString().Replace(".", ",")) + 1;
                                    break;
                                case "thal":
                                    valuesNum = 3;
                                    switch (value.ToString())
                                    {
                                        case "norm":
                                            value = 1;
                                            break;
                                        case "fix":
                                            value = 2;
                                            break;
                                        case "rev":
                                            value = 3;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                    entity.Attributes.Add(new MyAttribute(nameCol, value, type, valuesNum));
                }

                try
                {
                    entity.DecisionAttribute = row[table.Columns["decision"]].ToString();
                }
                catch (Exception)
                {
                    entity.DecisionAttribute = row[table.Columns.Count - 1].ToString() ?? string.Empty;
                }
                entities.Add(entity);
            }

            //min max
            if(entities!=null && entities.Count > 0)
            {
                Entity entity = entities[0];
                foreach(string columnName in entity.Attributes.Where(x=>x.AttributeType == AttributeType.Numeryczny).Select(x=>x.Name))
                {
                    List<double> values = entities.Select(x => Convert.ToDouble(x.Attributes.FirstOrDefault(y=>y.Name==columnName)?.Value)).ToList();
                    double min = values.Min();
                    double max = values.Max();

                    entities.ForEach(x =>
                    {
                        var attr = x.Attributes.First(y => y.Name == columnName);
                        if (attr != null)
                        {
                            attr.Min = min;
                            attr.Max = max;
                        }
                    }
                    );
                }
            }
            return entities;
        }
    }
}
