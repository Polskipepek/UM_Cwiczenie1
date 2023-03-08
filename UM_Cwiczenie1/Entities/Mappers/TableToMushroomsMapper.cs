using System.Data;

namespace Cwiczenie1.Entities.Mappers {
    internal class TableToMushroomsMapper {
        public static IEnumerable<Mushroom> Map(DataTable dt) {
            var mushrooms = new List<Mushroom>();

            foreach (DataRow row in dt.Rows) {
                var mushroom = new Mushroom {
                    CapDiameter = float.Parse(row["cap-diameter"].ToString()),
                    CapShape = char.Parse(row["cap-shape"].ToString()),
                    CapSurface = char.Parse(row["cap-surface"].ToString()),
                    CapColor = char.Parse(row["cap-color"].ToString()),
                    BruiseBleed = char.Parse(row["does-bruise-bleed"].ToString()),
                    GillAttachment = char.Parse(row["gill-attachment"].ToString()),
                    GillSpacing = char.Parse(row["gill-spacing"].ToString()),
                    GillColor = char.Parse(row["gill-color"].ToString()),
                    StemHeight = float.Parse(row["stem-height"].ToString()),
                    StemWidth = float.Parse(row["stem-width"].ToString()),
                    StemRoot = char.Parse(row["stem-root"].ToString()),
                    StemSurface = char.Parse(row["stem-surface"].ToString()),
                    StemColor = char.Parse(row["stem-color"].ToString()),
                    VeilType = char.Parse(row["veil-type"].ToString()),
                    VeilColor = char.Parse(row["veil-color"].ToString()),
                    HasRing = char.Parse(row["has-ring"].ToString()),
                    RingType = char.Parse(row["ring-type"].ToString()),
                    SporePrintColor = char.Parse(row["spore-print-color"].ToString()),
                    Habitat = char.Parse(row["habitat"].ToString()),
                    Season = char.Parse(row["season"].ToString()),
                    Edible = char.Parse(row["edible"].ToString())
                };

                mushrooms.Add(mushroom);
            }

            return mushrooms;
        }
    }
}
