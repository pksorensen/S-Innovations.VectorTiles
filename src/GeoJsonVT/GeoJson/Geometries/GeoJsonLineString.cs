namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class GeoJsonLineString : GeoJsonGeometry
    {
        public override string Type { get; } = GeoJsonLineStringType;
        public double[][] Coordinates { get; set; }
    }
}
