namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class Polygon : MultiLineStringPolygonGeometry
    {
        public override string Type { get; } = GeoJsonPolygonType;

    }
}
