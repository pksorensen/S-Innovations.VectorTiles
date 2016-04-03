namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries
{
    public class GeometryCollection : GeometryObject
    {
        public override string Type { get; } = GeoJsonGeometryCollectionType;

        public GeometryObject[] Geometries { get; set; }
    }
}
