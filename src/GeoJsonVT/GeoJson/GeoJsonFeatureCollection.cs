namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson
{
    public class GeoJsonFeatureCollection : GeoJsonObject
    {
        public override string Type { get; } = FeatureCollectionType;

        public GeoJsonFeature[] Features { get; set; }
    }
}
