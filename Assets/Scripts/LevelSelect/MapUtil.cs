using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Singletons.Static_Info;
using UnityEngine;

namespace LevelSelect
{
    public static class MapUtil
    {
        public static LevelData[] GetShortestPath(LevelData[] levels, LevelData start, Vector3 end, [CanBeNull] ICollection<int> discovered = null)
        {
            if (start.WorldPosition == end) return new LevelData[] {};
            
            var visited = new List<LevelData>();
            var queue = new SortedList<double, (int, double, List<LevelData>)>
            {
                { 0, (levels.ToList().IndexOf(start), 0, new List<LevelData> { start }) }
            };
            
            while (true)
            {
                if (queue.Count == 0) return null;
                
                var (current, distance, path) = queue.Values[0];
                queue.RemoveAt(0);

                foreach (var neighbor in levels[current].Connections)
                {
                    var neighborData = levels[neighbor];
                    
                    if (visited.Contains(neighborData)) continue;
                    visited.Add(neighborData);
                    
                    var newPath = new List<LevelData>(path) { neighborData };
                    if (neighborData.WorldPosition == end) return newPath.ToArray();
                    
                    if (discovered != null && !discovered.Contains(neighbor)) continue;
                    var newDistance = distance + Vector2.Distance(levels[current].WorldPosition, neighborData.WorldPosition);
                    queue.Add(newDistance, (neighbor, newDistance, newPath));
                }
            }
        }
        
        public static bool AreCcw(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.y-a.y) * (b.x-a.x) > (b.y-a.y) * (c.x-a.x);
        }
        
        public static bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return AreCcw(a,c,d) != AreCcw(b,c,d) && AreCcw(a,b,c) != AreCcw(a,b,d);
        }

        public static bool SegmentAndCircleIntersect(Vector2 start, Vector2 end, Vector2 point, float radius)
        {
            // simplified from https://www.splashlearn.com/math-vocabulary/distance-of-a-point-from-a-line
            var slope = (start.y - end.y) / (start.x - end.x);
            var intercept = start.y - slope * start.x;
            
            var dist = Mathf.Abs(slope * point.x - point.y + intercept) / Mathf.Sqrt(slope * slope + 1);
            
            // check if center is too close, and that it actually hits part of the line segment, not the extended line
            return dist < radius &&
                   Mathf.Min(start.x, end.x) <= point.x + radius && point.x - radius <= Mathf.Max(start.x, end.x) &&
                   Mathf.Min(start.y, end.y) <= point.y + radius && point.y - radius <= Mathf.Max(start.y, end.y);
        }
    }
}