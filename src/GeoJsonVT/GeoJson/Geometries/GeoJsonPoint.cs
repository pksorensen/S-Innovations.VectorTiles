namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class GeoJsonPoint : GeoJsonGeometry
    {

        public override string Type { get; } = GeoJsonPointType;
        public double[] Coordinates { get; set; }
    }
}
