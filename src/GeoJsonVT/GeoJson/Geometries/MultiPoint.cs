namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class MultiPoint : GeometryObject
    {

        public override string Type { get; } = GeoJsonMultiPointType;
        public double[][] Coordinates { get; set; }
    }
}
