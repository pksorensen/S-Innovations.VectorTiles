namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class MultiLineString : MultiLineStringPolygonGeometry
    {
        public override string Type { get; } = GeoJsonMultiLineStringType;
    }
}
