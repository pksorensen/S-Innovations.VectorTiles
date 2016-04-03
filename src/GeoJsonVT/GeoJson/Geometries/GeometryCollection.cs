namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class GeometryCollection : GeoJsonGeometry
    {
        public override string Type { get; } = GeoJsonGeometryCollectionType;

        public GeoJsonGeometry[] Geometries { get; set; }
    }
}
