namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class GeoJsonMultipoint : GeoJsonGeometry
    {

        public override string Type { get; } = GeoJsonMultiPointType;
        public double[][] Coordinates { get; set; }
    }
}
