﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.VectorTiles.GeoJsonVT.Models
{
    public struct VectorTileCoord
    {
        public VectorTileCoord(int z, int x, int y) { Z = z; Y = y; X = x; }
        public int Z;
        public int Y;
        public int X;

        public IEnumerable<VectorTileCoord> GetChildCoordinate()
        {
            yield return new VectorTileCoord(Z + 1, X * 2, Y * 2);
            yield return new VectorTileCoord(Z + 1, X * 2, Y * 2 + 1);
            yield return new VectorTileCoord(Z + 1, X * 2 + 1, Y * 2);
            yield return new VectorTileCoord(Z + 1, X * 2 + 1, Y * 2 + 1);
        }


        public string ToID()
        {
            return ((((1 << Z) * Y + X) * 32) + Z).ToString();
        }

        public static string ToID(int z,int x, int y)
        {
            return ((((1 << z) * y + x) * 32) + z).ToString();
        }

      
    }
}
