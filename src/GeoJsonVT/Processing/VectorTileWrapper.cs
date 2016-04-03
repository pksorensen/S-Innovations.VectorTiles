using System;
using System.Collections.Generic;
using System.Linq;
using SInnovations.VectorTiles.GeoJsonVT.Models;

namespace SInnovations.VectorTiles.GeoJsonVT.Processing
{
    public class VectorTileWrapper
    {
        protected VectorTileClipper Clipper { get; set; }
        public VectorTileWrapper(VectorTileClipper clipper = null)
        {
            Clipper = clipper ?? new VectorTileClipper();
        }
        public List<VectorTileFeature> Wrap(List<VectorTileFeature> features, double buffer, Func<double[], double[], double, double[]> intersectX)
        {
            var merged = features;
            var left =  Clipper.Clip(features,  1, -1 - buffer, buffer,      0, intersectX, -1, 2);//Left world copy;
            var right = Clipper.Clip(features,  1,  1 - buffer, 2 + buffer,  0, intersectX, -1, 2); //Right world copy;

            if (left.Any() || right.Any())
            {
                merged = Clipper.Clip(features, 1, -buffer, 1 + buffer, 0, intersectX, -1, 2);//Center world copy;

                if (left.Any())     merged = ShiftFeatureCoords(left, 1).Concat(merged).ToList(); //merge left into center;
                if (right.Any())    merged = merged.Concat(ShiftFeatureCoords(right, -1)).ToList(); //merge right into center
                
            }

            return merged;


        }

        private List<VectorTileFeature> ShiftFeatureCoords(List<VectorTileFeature> features, double offset)
        {
            var newFeatures = new List<VectorTileFeature>();

            for (var i = 0; i < features.Count; i++)
            {
                var feature = features[i];
                var type = feature.Type;


                var newGeometry = new List<VectorTileGeometry>();
                for (var j = 0; j < feature.Geometry.Length; j++)
                {
                    newGeometry.Add(ShiftCoords(feature.Geometry[j], offset));
                }


                newFeatures.Add(new VectorTileFeature
                {
                    Geometry = newGeometry.ToArray(),
                    Type = type,
                    Tags = feature.Tags,
                    Min = new[] { feature.Min[0] + offset, feature.Min[1] },
                    Max = new[] { feature.Max[0] + offset, feature.Max[1] }
                });
            }

            return newFeatures;
        }

        private VectorTileGeometry ShiftCoords(VectorTileGeometry points, double offset)
        {
            var newPoints = new VectorTileGeometry();
            newPoints.Area = points.Area;
            newPoints.Distance = points.Distance;

            for (var i = 0; i < points.Count; i++)
            {
                newPoints.Add(new[] { points[i][0] + offset, points[i][1], points[i][2] });
            }

            return newPoints;
        }
    }
}
