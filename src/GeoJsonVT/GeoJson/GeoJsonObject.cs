using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries;

namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson
{

    public abstract class GeoJsonObject
    {
        public const string FeatureCollectionType = "FeatureCollection";
        public const string FeatureType = "Feature";
        public const string GeoJsonPointType = "Point";
        public const string GeoJsonMultiPointType = "MultiPoint";
        public const string GeoJsonPolygonType = "Polygon";
        public const string GeoJsonLineStringType = "LineString";
        public const string GeoJsonMultiLineStringType = "MultiLineString";
        public const string GeoJsonMultiPolygonType = "MultiPolygon";
        public const string GeoJsonGeometryCollectionType = "GeometryCollection";

        public abstract string Type { get; }

    }

    public class GeoJsonObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(GeoJsonObject)== objectType || typeof(GeoJsonGeometry) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var type = jObject.SelectToken("type").ToString();
            
            switch (type)
            {
                case GeoJsonObject.FeatureCollectionType:
                   return jObject.ToObject<GeoJsonFeatureCollection>(serializer);
                case GeoJsonObject.FeatureType:
                    return jObject.ToObject<GeoJsonFeature>(serializer);
                case GeoJsonObject.GeoJsonGeometryCollectionType:                   
                    return jObject.ToObject<GeometryCollection>(serializer);
                case GeoJsonObject.GeoJsonLineStringType:
                    return jObject.ToObject<GeoJsonLineString>(serializer);
                case GeoJsonObject.GeoJsonMultiLineStringType:
                    return jObject.ToObject <GeoJsonMultiLineString>(serializer);
                case GeoJsonObject.GeoJsonMultiPointType:
                    return jObject.ToObject<GeoJsonMultipoint>(serializer);
                case GeoJsonObject.GeoJsonMultiPolygonType:
                    return jObject.ToObject<GeoJsonMultiPolygon>(serializer);
                case GeoJsonObject.GeoJsonPointType:
                    return jObject.ToObject<GeoJsonPoint>(serializer);
                case GeoJsonObject.GeoJsonPolygonType:
                    return jObject.ToObject<GeoJsonPolygon>(serializer);

            }

            throw new Exception("Unkown type");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
