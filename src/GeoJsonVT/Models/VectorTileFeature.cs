using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SInnovations.VectorTiles.GeoJsonVT.Models.Converters;

namespace SInnovations.VectorTiles.GeoJsonVT.Models
{
    
   // [JsonConverter(typeof(GeoJsonVTFeatureConverter))]
    public class VectorTileFeature
    {
        public VectorTileGeometry[] Geometry { get; set; }
        public int Type { get; set; }
        public Dictionary<string, object> Tags { get; set; }

     //   [JsonIgnore]
        public double[] Min { get; set; } = new double[] { 2, 1 };
     //   [JsonIgnore]
        public double[] Max { get; set; } = new double[] { -1, 0 };


    }

   
}
