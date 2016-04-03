namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class MultiPolygon : GeometryObject
    {

        public override string Type { get; } = GeoJsonMultiPolygonType;
        public double[][][][] Coordinates { get; set; }
    }
}
