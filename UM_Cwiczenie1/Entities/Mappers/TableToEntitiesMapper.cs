using System.Data;

namespace Cwiczenie1.Entities.Mappers {
    public static class TableToEntitiesMapper {
        public static IEnumerable<Entity> Map(DataTable table) {
            List<Entity> entities = new();
            foreach (DataRow row in table.Rows) {
                var entity = new Entity();
                foreach (DataColumn column in table.Columns) {
                    if (column.ColumnName.Equals("decision") || column.ColumnName.Equals("\"decision\"")) continue;
                    entity.Attributes.Add(new MyAttribute() { Name = column.ColumnName, Value = row[column.ColumnName] });
                }
                try {
                    entity.DecisionAttribute = row[table.Columns["decision"]].ToString();
                } catch (Exception) {
                    entity.DecisionAttribute = row[table.Columns.Count - 1].ToString() ?? string.Empty;
                }
                entities.Add(entity);
            }
            return entities;
        }
    }
}
