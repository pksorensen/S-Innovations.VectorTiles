namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class LineString : GeometryObject
    {
        public override string Type { get; } = GeoJsonLineStringType;
        public double[][] Coordinates { get; set; }
    }
}
