using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SInnovations.VectorTiles.GeoJsonVT.GeoJson.Geometries;

namespace SInnovations.VectorTiles.GeoJsonVT.GeoJson
{
    [JsonConverter(typeof(GeoJsonObjectConverter))]
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
            return typeof(GeoJsonObject)== objectType || typeof(GeometryObject) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            

            GeoJsonObject value = null;
            if (!CanConvert(objectType))
            {
                value = Activator.CreateInstance(objectType) as GeoJsonObject;
                serializer.Populate(reader, value);
            }
            else
            {

                JToken token = JToken.Load(reader);
                if (token.Type == JTokenType.Null)
                    return null;

                var jObject = token as JObject;
                var type = jObject.SelectToken("type")?.ToString() ?? jObject.SelectToken("Type")?.ToString();

                switch (type)
                {
                    case GeoJsonObject.FeatureCollectionType:
                        value = new GeoJsonFeatureCollection(); break;
                    //  return jObject.ToObject<GeoJsonFeatureCollection>(serializer);
                    case GeoJsonObject.FeatureType:
                        value = new GeoJsonFeature(); break;
                    // return jObject.ToObject<GeoJsonFeature>(serializer);
                    case GeoJsonObject.GeoJsonGeometryCollectionType:
                        value = new GeometryCollection(); break;
                    //  return jObject.ToObject<GeometryCollection>(serializer);
                    case GeoJsonObject.GeoJsonLineStringType:
                        value = new LineString(); break;
                    // return jObject.ToObject<GeoJsonLineString>(serializer);
                    case GeoJsonObject.GeoJsonMultiLineStringType:
                        value = new MultiLineString(); break;
                    // return jObject.ToObject <GeoJsonMultiLineString>(serializer);
                    case GeoJsonObject.GeoJsonMultiPointType:
                        value = new MultiPoint(); break;
                    //  return jObject.ToObject<GeoJsonMultipoint>(serializer);
                    case GeoJsonObject.GeoJsonMultiPolygonType:
                        value = new MultiPolygon(); break;
                    //  return jObject.ToObject<GeoJsonMultiPolygon>(serializer);
                    case GeoJsonObject.GeoJsonPointType:
                        value = new Point(); break;
                    //  return jObject.ToObject<GeoJsonPoint>(serializer);
                    case GeoJsonObject.GeoJsonPolygonType:
                        value = new Polygon(); break;
                    //  return jObject.ToObject<GeoJsonPolygon>(serializer);
                    default:

                        throw new Exception("Unkown type");
                }
                serializer.Populate(jObject.CreateReader(), value);
            }
          
            return value;
           
        }

        public override bool CanWrite { get; } = false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
