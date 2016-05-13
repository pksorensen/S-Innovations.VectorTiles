using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SInnovations.VectorTiles.GeoJsonVT;
using SInnovations.VectorTiles.GeoJsonVT.GeoJson;
using SInnovations.VectorTiles.GeoJsonVT.Logging;
using SInnovations.VectorTiles.GeoJsonVT.Models;
using SInnovations.VectorTiles.GeoJsonVT.Processing;
using SInnovations.VectorTiles.GeoJsonVT.Streaming;

namespace GeoJsonVT.Streaming
{
    public class StreamingStackItem
    {

        public VectorTileCoord Coord { get; internal set; }
        public VectorTileFeature Feature { get; internal set; }

        public int ParentSplitCount { get; set; }
        public List<StreamingStackItem> Childs { get; } = new List<StreamingStackItem>();

        internal StreamingStackItem AddChild(StreamingStackItem streamingStackItem)
        {
            Childs.Add(streamingStackItem);
            return streamingStackItem;
        }
    }
    public class StreamingOptions : GeoJsonVectorTilesOptions
    {
        public Action<VectorTileCoord> OnFeatureNoSplit { get; set; }
        public Action<StreamingStackItem> OnNoSingleSplit { get; set; }
        public Action<VectorTileCoord> OnSingleSplit { get; set; }
    }

    public class FileSystemTileStore : List<VectorTileCoord>,  ITileStore
    {
        private HashSet<string> _coords = new HashSet<string>();
         
        private LRUCache<string, VectorTile> _tiles;
        public ICollection<VectorTileCoord> TileCoords { get { return this; } }

        private string _path;
        public FileSystemTileStore( string path , int inMemCapacity)
        {
            _path = path;
            _tiles = new LRUCache<string, VectorTile>(new LRUCacheOptions { Capacity = inMemCapacity });
        }

         
        public bool Contains(string id)
        {
            return _coords.Contains(id);
        }

        public VectorTile Get(string id)
        {
            var cached = _tiles.Get(id);
            if (cached == null)
            {
               
                var path = Path.Combine(_path, $"tmp{id}.json");
                return JsonConvert.DeserializeObject<VectorTile>(File.ReadAllText(path));

            }

            return cached;
        }

        public VectorTile Set(string id, VectorTile value)
        {
            _coords.Add(value.TileCoord.ToID());

            var removed = _tiles.Add(id, value);
            if(removed != null)
            {
                var data = JObject.FromObject(removed);
                var path = Path.Combine(_path, $"tmp{removed.TileCoord.ToID()}.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, data.ToString());
            }
            return value;
        }
    }
    public class StreamingGeoJsonVectorTiles : GeoJsonVectorTiles<StreamingOptions>
    {

        public StreamingGeoJsonVectorTiles(StreamingOptions options = null) : base(options?? new StreamingOptions())
        {

          //  Tiles = new Dictionary<string, VectorTile>();
          //  TileCoords = new List<VectorTileCoord>();
        }


        public void ProcessStream(Stream data)
        {
            var z2 = 1 << Options.MaxZoom;//2^z
            var convertTransform = new TransformManyBlock<GeoJsonObject, VectorTileFeature>(geojson => Converter.Convert(geojson, Options.Tolerance / (z2 * Options.Extent)));
            var wrapProcess = new TransformManyBlock<VectorTileFeature, VectorTileFeature>(feature => Wrapper.Wrap(feature, Options.Buffer / Options.Extent, IntersectX));

            var tileProcess = new ActionBlock<VectorTileFeature>((feature) =>
            {



            });


        }
        public void TileFaature(GeoJsonFeature feature)
        {
            var z2 = 1 << Options.MaxZoom;//2^z
            var a = Converter.Convert(feature, Options.Tolerance / (z2 * Options.Extent));
            var b = Wrapper.Wrap(a, Options.Buffer / Options.Extent, IntersectX);

            b.ForEach(c => TileFeature(c, new VectorTileCoord()));

        }
        public void TileFeature(VectorTileFeature feature1, VectorTileCoord startCoord, int? cz = null, int? cx = null, int? cy = null)
        {
            var stack = new Stack<StreamingStackItem>();
            stack.Push(new StreamingStackItem { Feature = feature1, Coord = startCoord });
            

            

            int? solid = null;

            while (stack.Count > 0)
            {
                var item = stack.Pop();

                var x = item.Coord.X;
                var y = item.Coord.Y;
                var z = item.Coord.Z;

                var z2 = 1 << z;
                var id = item.Coord.ToID();
                VectorTile tile = Tiles.Contains(id) ? Tiles.Get(id) : null;



                if (tile == null)
                {


                    tile = Tiles.Set(id,new VectorTile() { Z2 = z2, X = x, Y = y });
                    //tile.Z2 = z2;
                    //tile.X = x;
                    //tile.Y = y;

                    Tiles.TileCoords.Add(new VectorTileCoord(z, x, y));

                }

                var tileTolerance = z == Options.MaxZoom ? 0 : Options.Tolerance / (z2 * Options.Extent);
                tile.Add(item.Feature, tileTolerance, z == Options.MaxZoom);




                // stop tiling if we reached base zoom or our target tile zoom
                if (z == Options.MaxZoom)
                {
                    Options.OnFeatureNoSplit?.Invoke(item.Coord);

                    continue;
                }

                // stop tiling if it's not an ancestor of the target tile
                if (cz.HasValue)
                {
                    if (z == cz.Value)
                        continue;

                    var m = 1 << (cz.Value - z);
                    if (x != (int)Math.Floor((double)cx.Value / m) || y != (int)Math.Floor((double)cy.Value / m))
                        continue;
                }

                // stop tiling if the tile is solid clipped square
                if (!Options.SolidChildren && IsClippedSquare(item.Feature, tile.Z2, tile.X, tile.Y, Options.Extent, Options.Buffer))
                {
                    if (cz.HasValue) solid = z; // and remember the zoom if we're drilling down

                    Options.OnFeatureNoSplit?.Invoke(item.Coord);

                    continue;
                }

                // if we slice further down, no need to keep source geometry
                tile.Source = null;

                // values we'll use for clipping
                var k1 = 0.5 * Options.Buffer / Options.Extent;
                var k2 = 0.5 - k1;
                var k3 = 0.5 + k1;
                var k4 = 1 + k1;
                VectorTileFeature tl = null, bl = null, tr = null, br = null;


                var left = Clipper.Clip(item.Feature, z2, x - k1, x + k3, 0, IntersectX, tile.min[0], tile.max[0]);
                var right = Clipper.Clip(item.Feature, z2, x + k2, x + k4, 0, IntersectX, tile.min[0], tile.max[0]);

                if (left != null)
                {
                    tl = Clipper.Clip(left, z2, y - k1, y + k3, 1, intersectY, tile.min[1], tile.max[1]);
                    bl = Clipper.Clip(left, z2, y + k2, y + k4, 1, intersectY, tile.min[1], tile.max[1]);
                }

                if (right != null)
                {
                    tr = Clipper.Clip(right, z2, y - k1, y + k3, 1, intersectY, tile.min[1], tile.max[1]);
                    br = Clipper.Clip(right, z2, y + k2, y + k4, 1, intersectY, tile.min[1], tile.max[1]);
                }

                var count = (tl != null ? 1 : 0) + (bl != null ? 1 : 0) + (tr != null ? 1 : 0) + (br != null ? 1 : 0);

                //   if (debug > 1) console.timeEnd('clipping');

                if (tl != null) stack.Push( item.AddChild( new StreamingStackItem { ParentSplitCount = Math.Max(item.ParentSplitCount, count), Feature = tl, Coord = new VectorTileCoord(z + 1, x * 2, y * 2) }));
                if (bl != null) stack.Push( item.AddChild( new StreamingStackItem { ParentSplitCount = Math.Max(item.ParentSplitCount, count), Feature = bl, Coord = new VectorTileCoord(z + 1, x * 2, y * 2 + 1) }));
                if (tr != null) stack.Push( item.AddChild( new StreamingStackItem { ParentSplitCount = Math.Max(item.ParentSplitCount, count), Feature = tr, Coord = new VectorTileCoord(z + 1, x * 2 + 1, y * 2) }));
                if (br != null) stack.Push( item.AddChild( new StreamingStackItem { ParentSplitCount = Math.Max(item.ParentSplitCount, count), Feature = br, Coord = new VectorTileCoord(z + 1, x * 2 + 1, y * 2 + 1) }));

                
                if(Options.OnSingleSplit != null && count == 1)
                {
                    Options.OnSingleSplit(item.Coord);
                }

                if (Options.OnNoSingleSplit != null && count > 1)
                {
                    Options.OnNoSingleSplit(item);
                }

                if ( Options.OnFeatureNoSplit != null && count == 0)
                {
                    Options.OnFeatureNoSplit(item.Coord);
                }
                

            }


        }

        protected bool IsClippedSquare(VectorTileFeature feature, int z2, int x, int y, double extent, double buffer)
        {
            if (feature.Type != 3 || feature.Geometry.Length > 1) return false;

            var len = feature.Geometry[0].Count;
            if (len != 5) return false;

            for (var i = 0; i < len; i++)
            {
                var p = Transformer.TransformPoint(feature.Geometry[0][i], extent, z2, x, y);
                if ((p[0] != -buffer && p[0] != extent + buffer) ||
                    (p[1] != -buffer && p[1] != extent + buffer)) return false;
            }

            return true;
        }

    }
}
