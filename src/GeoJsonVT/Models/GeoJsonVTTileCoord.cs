using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.VectorTiles.GeoJsonVT.Models
{
    public struct GeoJsonVTTileCoord
    {
        public GeoJsonVTTileCoord(int z, int x, int y) { Z = z; Y = y; X = x; }
        public int Z;
        public int Y;
        public int X;

        public IEnumerable<GeoJsonVTTileCoord> GetChildCoordinate()
        {
            yield return new GeoJsonVTTileCoord(Z + 1, X * 2, Y * 2);
            yield return new GeoJsonVTTileCoord(Z + 1, X * 2, Y * 2 + 1);
            yield return new GeoJsonVTTileCoord(Z + 1, X * 2 + 1, Y * 2);
            yield return new GeoJsonVTTileCoord(Z + 1, X * 2 + 1, Y * 2 + 1);
        }
    }
}
