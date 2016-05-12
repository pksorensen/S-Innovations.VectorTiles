using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SInnovations.VectorTiles.GeoJsonVT.Models.Converters
{
    public class GeoJsonVTFeatureConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(VectorTileFeature) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var feature = value as VectorTileFeature;
            writer.WriteStartObject();
            writer.WritePropertyName("geometry");
            if (feature.Type == 1)
            {
                serializer.Serialize(writer, feature.Geometry[0].ToIntArray());
            }
            else
            {
                serializer.Serialize(writer, feature.Geometry.Select(k => k.ToIntArray()));
            }
            writer.WritePropertyName("type"); writer.WriteValue(feature.Type);
            writer.WritePropertyName("tags"); serializer.Serialize(writer, feature.Tags);
            writer.WriteEndObject();
        }
    }
}
