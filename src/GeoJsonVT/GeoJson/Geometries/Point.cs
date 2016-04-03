namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class Point : GeometryObject
    {

        public override string Type { get; } = GeoJsonPointType;
        public double[] Coordinates { get; set; }
    }
}
