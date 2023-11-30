using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace Level_Select
{
    public class GenerateMap : MonoBehaviour
    {
        public Material lineMaterial;
        public GameObject planetPrefab;
        public AssetLabelReference spriteLabel;
        public LevelSelectData data;
        
        private void Start()
        {
            data.OnPopulate += RenderGalaxy;
            
            // grab sprites, then continue
            Addressables.LoadAssetsAsync<Sprite>(spriteLabel, null).Completed += handle =>
            {
                // only re-generate if it doesn't already exist
                if (data.CurrentPlanet == -1) GenerateGalaxy(handle.Result);
            };
        }
        
        // TODO: in the name of all that is holy, add some variables lmao
        private void GenerateGalaxy(IList<Sprite> sprites)
        {
            data.CurrentPlanet = 0;
        
            var levels = new List<LevelData>();
            var usedGridPositions = new List<Vector2>();
            
            var planetScale = planetPrefab.transform.localScale;
            while (true)
            {
                // position represents position in a theoretical discrete grid of planets
                Vector2 position;
                do
                {
                    position = new Vector2(Random.Range(-3, 4), Random.Range(-3, 4));
                } while (usedGridPositions.Contains(position));
                usedGridPositions.Add(position);
                
                levels.Add(new LevelData
                {
                    Difficulty = Random.Range(3, 50),
                    Sprite = sprites[Random.Range(0, sprites.Count)],
                    WorldPosition = planetPrefab.transform.localPosition +
                               (Vector3) (position * planetScale * 2.25f + // make every grid space 2 and a quarter planets wide
                                          Random.insideUnitCircle * planetScale / 2) // offset by up to half a planet
                });
                
                if (Random.value < 1/(1+Mathf.Exp(5-levels.Count/2.5f))) break;
            }
            
            var connections = new List<Tuple<int, int>>();
            var connectionsPerLevel = levels.ConvertAll(_ => new List<int>());
            for (var level = 0; level < levels.Count; level++)
            {
                var connectionsLeft = Random.Range(1, 4) - connectionsPerLevel[level]?.Count;

                var validConnections = levels
                    .Select((_, i) => i)
                    .Where(i => 
                        i != level &&
                        connectionsPerLevel[i].Count < 3 &&
                        !connectionsPerLevel[level].Contains(i) &&
                        !connections.Any(c => Intersect(levels[i].WorldPosition, levels[level].WorldPosition, levels[c.Item1].WorldPosition, levels[c.Item2].WorldPosition)) &&
                        !levels.Any(l => l != levels[i] && l != levels[level] && SegmentAndCircleIntersect(levels[i].WorldPosition, levels[level].WorldPosition, l.WorldPosition, Mathf.Max(planetScale.x, planetScale.y))))
                    .ToList();
                
                while (connectionsLeft > 0 && validConnections.Count > 0)
                {
                    var connectionIdx = Random.Range(0, validConnections.Count);
                    var connection = validConnections[connectionIdx];
                    
                    connections.Add(new Tuple<int, int>(level, connection));
                    
                    connectionsPerLevel[level].Add(connection);
                    connectionsPerLevel[connection].Add(level);
                    
                    validConnections.RemoveAt(connectionIdx);

                    connectionsLeft--;
                }
            }
            
            data.PopulateData(levels.ToArray(), connections.ToArray());
        }

        private void RenderGalaxy()
        {
            foreach (var level in data.Levels)
            {
                var planet = Instantiate(planetPrefab, level.WorldPosition, Quaternion.identity);
                planet.GetComponent<SpriteRenderer>().sprite = level.Sprite;
            }
            
            foreach (var connection in data.Connections)
            {
                var line = new GameObject("LineRenderer").AddComponent<LineRenderer>();

                line.material = lineMaterial;
                line.startColor = line.endColor = Color.red;
                line.startWidth = line.endWidth = 3f;
                
                line.SetPosition(0, data.Levels[connection.Item1].WorldPosition);
                line.SetPosition(1, data.Levels[connection.Item2].WorldPosition);
            }
        }
        
        private static bool AreCcw(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.y-a.y) * (b.x-a.x) > (b.y-a.y) * (c.x-a.x);
        }
        
        private static bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return AreCcw(a,c,d) != AreCcw(b,c,d) && AreCcw(a,b,c) != AreCcw(a,b,d);
        }

        private static bool SegmentAndCircleIntersect(Vector2 start, Vector2 end, Vector2 point, float radius)
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