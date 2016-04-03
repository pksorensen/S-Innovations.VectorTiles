namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class GeoJsonMultiPolygon : GeoJsonGeometry
    {

        public override string Type { get; } = GeoJsonMultiPolygonType;
        public double[][][][] Coordinates { get; set; }
    }
}
