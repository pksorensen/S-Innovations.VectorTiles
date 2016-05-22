using System.Collections.Generic;
using SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries;

namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson
{
    public class GeoJsonFeature : GeoJsonObject
    {
        public override string Type { get; } = FeatureType;
        public GeometryObject Geometry { get; set; }
        public Dictionary<string, object> Properties { get; set; }

    }
}
