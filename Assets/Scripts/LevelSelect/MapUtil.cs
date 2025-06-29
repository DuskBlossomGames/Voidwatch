using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Singletons.Static_Info;
using UnityEngine;

namespace LevelSelect
{
    public static class MapUtil
    {
        public static Vector3[] GetShortestPath(LevelData[] levels, LevelData start, Vector3 end, [CanBeNull] ICollection<int> discovered = null)
        {
            if (start.WorldPosition == end) return new Vector3[] {};
            
            var visited = new List<Vector3>();
            var queue = new SortedList<double, (int, double, List<Vector3>)>
            {
                { 0, (levels.ToList().IndexOf(start), 0, new List<Vector3> { start.WorldPosition }) }
            };
            
            while (true)
            {
                if (queue.Count == 0) return null;
                
                var (current, distance, path) = queue.Values[0];
                queue.RemoveAt(0);

                foreach (var neighbor in levels[current].Connections)
                {
                    var neighborData = levels[neighbor];
                    var position = neighborData.WorldPosition;
                    
                    if (visited.Contains(position)) continue;
                    visited.Add(position);
                    
                    var newPath = new List<Vector3>(path) { position };
                    if (position == end) return newPath.ToArray();
                    
                    if (discovered != null && !discovered.Contains(neighbor)) continue;
                    var newDistance = distance + Vector2.Distance(levels[current].WorldPosition, position);
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