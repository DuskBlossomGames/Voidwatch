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
            // grab sprites, then continue
            Addressables.LoadAssetsAsync<Sprite>(spriteLabel, null).Completed += handle =>
            {
                if (data.CurrentPlanet == -1) GenerateGalaxy(handle.Result);
                
                RenderGalaxy();
            };
        }
        
        // TODO: in the name of all that is holy, add some variables lmao
        private void GenerateGalaxy(IList<Sprite> sprites)
        {
            data.CurrentPlanet = 0;
        
            var levels = new List<LevelData>();
            while (true)
            {
                // position represents position in a theoretical discrete grid of planets
                Vector2 position;
                do
                {
                    position = new Vector2(Random.Range(-3, 4), Random.Range(-3, 4));
                } while (levels.Any(l => l.Position == position));
                
                levels.Add(new LevelData
                {
                    Difficulty = Random.Range(3, 50),
                    Sprite = sprites[Random.Range(0, sprites.Count)],
                    Position = position
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
                        !connections.Any(c => Intersect(levels[i].Position, levels[level].Position, levels[c.Item1].Position, levels[c.Item2].Position)))
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
            
            data.Levels = levels.ToArray();
            data.Connections = connections.ToArray();
        }

        private void RenderGalaxy()
        {
            var realPositions = new List<Vector2>();
            foreach (var level in data.Levels)
            {
                var position = planetPrefab.transform.localPosition +
                               (Vector3)(
                                   level.Position * planetPrefab.transform.localScale * 3f +
                                   Random.insideUnitCircle * planetPrefab.transform.localScale / 2);
                realPositions.Add(position);
                
                var planet = Instantiate(planetPrefab, position, Quaternion.identity);
                planet.GetComponent<SpriteRenderer>().sprite = level.Sprite;
            }

            foreach (var connection in data.Connections)
            {
                var line = new GameObject("LineRenderer").AddComponent<LineRenderer>();

                line.material = lineMaterial;
                line.startColor = line.endColor = Color.red;
                line.startWidth = line.endWidth = 1.5f;
                
                line.SetPosition(0, realPositions[connection.Item1]);
                line.SetPosition(1, realPositions[connection.Item2]);
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
    }
}