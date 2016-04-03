using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SInnovations.VectorTiles.GeoJsonVT.GeoJson;
using SInnovations.VectorTiles.GeoJsonVT.Models;
using SInnovations.VectorTiles.GeoJsonVT.Processing;

namespace SInnovations.VectorTiles.GeoJsonVT
{
    public class GeoJsonVTStackItem
    {

        public GeoJsonVTTileCoord Coord { get; internal set; }
        public List<GeoJsonVTFeature> Features { get; internal set; }
    }
    public class GeoJsonVTTile
    {
        public List<GeoJsonVTFeature> Features { get; set; } = new List<GeoJsonVTFeature>();

        public int NumPoints { get; set; } = 0;
        public int NumSimplified { get; set; } = 0;


        public List<GeoJsonVTFeature> Source { get; internal set; }

        public int Z2;
        public int Y;
        public int X;

        public bool Transformed { get; set; }


        public double[] min { get; set; } = new double[] { 2, 1 };
        public double[] max { get; set; } = new double[] { -1, 0 };


        public static GeoJsonVTTile CreateTile(List<GeoJsonVTFeature> features, int z2, int tx, int ty, double tolerance, bool noSimplify)
        {
            var tile = new GeoJsonVTTile();

            tile.Z2 = z2;
            tile.X = tx;
            tile.Y = ty;
            for (var i = 0; i < features.Count; i++)
            {
                tile.AddFeature(features[i], tolerance, noSimplify);

                var min = features[i].Min;
                var max = features[i].Max;

                if (min[0] < tile.min[0]) tile.min[0] = min[0];
                if (min[1] < tile.min[1]) tile.min[1] = min[1];
                if (max[0] > tile.max[0]) tile.max[0] = max[0];
                if (max[1] > tile.max[1]) tile.max[1] = max[1];
            }
            return tile;
        }

        private void AddFeature(GeoJsonVTFeature feature, double tolerance, bool noSimplify)
        {
            var geom = feature.Geometry;
            var type = feature.Type;
            var simplified = new List<GeoJsonVTPointCollection>();

            var sqTolerance = tolerance * tolerance;
            // i, j, ring, p;

            if (type == 1)
            {
                var first = new GeoJsonVTPointCollection(); simplified.Add(first);
                var points = geom[0];
                for (var i = 0; i < points.Count; i++)
                {
                    first.Add(points[i]);
                    NumPoints++;
                    NumSimplified++;
                }

            }
            else
            {

                // simplify and transform projected coordinates for tile geometry
                for (var i = 0; i < geom.Length; i++)
                {
                    var ring = geom[i];

                    // filter out tiny polylines & polygons
                    if (!noSimplify && ((type == 2 && ring.Distance < tolerance) ||
                                        (type == 3 && ring.Area < sqTolerance)))
                    {
                        NumPoints += ring.Count;
                        continue;
                    }

                    var simplifiedRing = new GeoJsonVTPointCollection();

                    for (var j = 0; j < ring.Count; j++)
                    {
                        var p = ring[j];
                        // keep points with importance > tolerance
                        if (noSimplify || p[2] > sqTolerance)
                        {
                            simplifiedRing.Add(p);
                            NumSimplified++;
                        }
                        NumPoints++;
                    }

                    simplified.Add(simplifiedRing);
                }
            }

            if (simplified.Count > 0)
            {
                Features.Add(new GeoJsonVTFeature
                {
                    Geometry = simplified.ToArray(),
                    Type = type,
                    Tags = feature.Tags
                });
            }
        }
    }
    public static class Extensions
    {
        public static bool HasAny(this List<GeoJsonVTFeature> features)
        {
            if (features == null)
                return false;
            return features.Any();
        }
        public static bool NotNull(this GeoJsonVTTile tile)
        {
            if (tile == null)
                return false;
            return true;
        }
        public static bool IsNull(this GeoJsonVTTile tile)
        {
            if (tile == null)
                return true;
            return false;
        }
        public static bool NoSource(this GeoJsonVTTile tile)
        {
            return tile.IsNull() || tile.Source == null;
        }
    }
    public class GeoJsonVT
    {
        protected GeoJsonVTConverter Converter { get; set; }
        protected GeoJsonVTWrapper Wrapper { get; set; }
        protected GeoJsonVTClipper Clipper { get; set; }
        protected GeoJsonVTTransformer Transformer { get; set; }

        public GeoJsonVTOptions Options { get; set; }
        public Dictionary<string, GeoJsonVTTile> Tiles { get; set; }
        public List<GeoJsonVTTileCoord> TileCoords { get; set; }

        public static double[] IntersectX(double[] a, double[] b, double x)
        {
            return new[] { x, (x - a[0]) * (b[1] - a[1]) / (b[0] - a[0]) + a[1], 1 };
        }
        public static double[] intersectY(double[] a, double[] b, double y)
        {
            return new[] { (y - a[1]) * (b[0] - a[0]) / (b[1] - a[1]) + a[0], y, 1 };
        }


        public GeoJsonVT(GeoJsonVTOptions options = null, GeoJsonVTConverter converter = null, GeoJsonVTWrapper wrapper = null, GeoJsonVTClipper clipper = null, GeoJsonVTTransformer transformer=null)
        {
            Converter = converter ?? new GeoJsonVTConverter();
            Clipper = clipper ?? new GeoJsonVTClipper();
            Wrapper = wrapper ?? new GeoJsonVTWrapper(Clipper);
            Options = options ?? new GeoJsonVTOptions();
            Transformer = transformer ?? new GeoJsonVTTransformer();
        }

      
        public void ProcessData(GeoJsonObject data)
        {
            var z2 = 1 << Options.MaxZoom;//2^z
            var features = Converter.Convert(data, Options.Tolerance / (z2 * Options.Extent));

            Tiles = new Dictionary<string, GeoJsonVTTile>();
            TileCoords = new List<GeoJsonVTTileCoord>();

            features = Wrapper.Wrap(features, Options.Buffer / Options.Extent, IntersectX);

            // start slicing from the top tile down
            if (features.Count > 0) SplitTile(features, new GeoJsonVTTileCoord());

        }

        public int? SplitTile(List<GeoJsonVTFeature> startfeatures, GeoJsonVTTileCoord startCoord, int? cz = null, int? cx =null, int? cy = null)
        {
            var stack = new Stack<GeoJsonVTStackItem>();
            stack.Push(new GeoJsonVTStackItem { Features = startfeatures, Coord = startCoord });
            

            int? solid = null;

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                var features = item.Features;
                var x = item.Coord.X;
                var y = item.Coord.Y;
                var z = item.Coord.Z;

                var z2 = 1 << z;
                var id = ToID(z, x, y);
                GeoJsonVTTile tile = Tiles.ContainsKey(id) ? Tiles[id] : null;

               
                
                if (tile == null)
                {
                    var tileTolerance = z == Options.MaxZoom ? 0 : Options.Tolerance / (z2 * Options.Extent);

                    tile = Tiles[id] = GeoJsonVTTile.CreateTile(features, z2, x, y, tileTolerance, z == Options.MaxZoom);
                    TileCoords.Add(new GeoJsonVTTileCoord(z,x,y));

                }

                // save reference to original geometry in tile so that we can drill down later if we stop now
                tile.Source = features;

                // if it's the first-pass tiling
                if (!cz.HasValue)
                {
                    // stop tiling if we reached max zoom, or if the tile is too simple
                    if (z == Options.IndexMaxZoom || tile.NumPoints <= Options.IndexMaxPoints) continue;

                    // if a drilldown to a specific tile
                }
                else
                {
                    
                    // stop tiling if we reached base zoom or our target tile zoom
                    if (z == Options.MaxZoom) continue;

                    // stop tiling if it's not an ancestor of the target tile
                    if (cz.HasValue)
                    {
                        if (z == cz.Value) continue;

                        var m = 1 << (cz.Value - z);
                        if (x != (int)Math.Floor((double)cx.Value / m) || y != (int)Math.Floor((double)cy.Value / m)) continue;
                    }
                }

                // stop tiling if the tile is solid clipped square
                if (!Options.SolidChildren && IsClippedSquare(tile, Options.Extent, Options.Buffer))
                {
                    if (cz.HasValue) solid = z; // and remember the zoom if we're drilling down
                    continue;
                }

                // if we slice further down, no need to keep source geometry
                tile.Source = null;

                //  if (debug > 1) console.time('clipping');

                // values we'll use for clipping
                var k1 = 0.5 * Options.Buffer / Options.Extent;
                var k2 = 0.5 - k1;
                var k3 = 0.5 + k1;
                var k4 = 1 + k1;
                List<GeoJsonVTFeature> tl, bl, tr, br, left, right;

                tl = bl = tr = br = null;

                left = Clipper.Clip(features, z2, x - k1, x + k3, 0, IntersectX, tile.min[0], tile.max[0]);
                right = Clipper.Clip(features, z2, x + k2, x + k4, 0, IntersectX, tile.min[0], tile.max[0]);

                if (left.HasAny())
                {
                    tl = Clipper.Clip(left, z2, y - k1, y + k3, 1, intersectY, tile.min[1], tile.max[1]);
                    bl = Clipper.Clip(left, z2, y + k2, y + k4, 1, intersectY, tile.min[1], tile.max[1]);
                }

                if (right.HasAny())
                {
                    tr = Clipper.Clip(right, z2, y - k1, y + k3, 1, intersectY, tile.min[1], tile.max[1]);
                    br = Clipper.Clip(right, z2, y + k2, y + k4, 1, intersectY, tile.min[1], tile.max[1]);
                }

                //   if (debug > 1) console.timeEnd('clipping');

                if (tl.HasAny()) stack.Push(new GeoJsonVTStackItem { Features = tl, Coord = new GeoJsonVTTileCoord(z + 1, x * 2,     y * 2    ) });
                if (bl.HasAny()) stack.Push(new GeoJsonVTStackItem { Features = bl, Coord = new GeoJsonVTTileCoord(z + 1, x * 2,     y * 2 + 1) });
                if (tr.HasAny()) stack.Push(new GeoJsonVTStackItem { Features = tr, Coord = new GeoJsonVTTileCoord(z + 1, x * 2 + 1, y * 2    ) });
                if (br.HasAny()) stack.Push(new GeoJsonVTStackItem { Features = br, Coord = new GeoJsonVTTileCoord(z + 1, x * 2 + 1, y * 2 + 1) });
            }

            return solid;

        }
        public GeoJsonVTTile GetTile(GeoJsonVTTileCoord coord)
        {
            return GetTile(coord.Z, coord.X, coord.Y);
        }
        public GeoJsonVTTile GetTile(int z,int x,int y)
        {
            var options = this.Options;
            var extent = options.Extent;
            var debug = options.Debug;

            var z2 = 1 << z;
            x = ((x % z2) + z2) % z2; // wrap tile x coordinate

            var id = ToID(z, x, y);
            if (Tiles.ContainsKey(id)) return Transformer.TransformTile(Tiles[id], extent);

            //  if (debug > 1) console.log('drilling down to z%d-%d-%d', z, x, y);

            var z0 = z;
            var x0 = x;
            var y0 = y;
            GeoJsonVTTile parent=null;

            while (parent.IsNull() && z0 > 0)
            {
                z0--;
                x0 = (int)Math.Floor(x0 / 2.0);
                y0 = (int)Math.Floor(y0 / 2.0);
                var tileId = ToID(z0, x0, y0);
                parent = Tiles.ContainsKey(tileId) ? Tiles[tileId] : null;
            }

            if (parent.NoSource()) return null;

            // if we found a parent tile containing the original geometry, we can drill down from it
         //   if (debug > 1) console.log('found parent tile z%d-%d-%d', z0, x0, y0);

            // it parent tile is a solid clipped square, return it instead since it's identical
            if (IsClippedSquare(parent, extent, options.Buffer)) return Transformer.TransformTile(parent, extent);

           // if (debug > 1) console.time('drilling down');
            var solid = SplitTile(parent.Source, new GeoJsonVTTileCoord(z0, x0, y0), z, x, y);
         //   if (debug > 1) console.timeEnd('drilling down');

            // one of the parent tiles was a solid clipped square
            if (solid.HasValue)
            {
                double m = 1 << (z - solid.Value);
                id = ToID(solid.Value,(int) Math.Floor(x / m), (int)Math.Floor(y / m));
            }

            return Tiles.ContainsKey(id) ? Transformer.TransformTile(this.Tiles[id], extent) : null;
        }






        private bool IsClippedSquare(GeoJsonVTTile tile, double extent, double buffer)
        {
            var features = tile.Source;
            if (features.Count != 1) return false;

            var feature = features[0];
            if (feature.Type != 3 || feature.Geometry.Length > 1) return false;

            var len = feature.Geometry[0].Count;
            if (len != 5) return false;

            for (var i = 0; i < len; i++)
            {
                var p = Transformer.TransformPoint(feature.Geometry[0][i], extent, tile.Z2, tile.X, tile.Y);
                if ((p[0] != -buffer && p[0] != extent + buffer) ||
                    (p[1] != -buffer && p[1] != extent + buffer)) return false; 
            }

            return true;
        }

        string ToID(int z, int x, int y)
        {
            return ((((1 << z) * y + x) * 32) + z).ToString();
        }
    }


}
