using System;
using System.Collections.Generic;
using System.Linq;
using Scriptable_Objects;
using Static_Info;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Util;
using Random = UnityEngine.Random;
using static Static_Info.LevelSelectData;

namespace LevelSelect
{
    public class GenerateMap : MonoBehaviour
    {
        public Material lineMaterial;
        public GameObject planetPrefab;
        public AssetLabelReference spriteLabel;
        public Sprite hiddenSprite, spaceStationSprite, entranceSprite;
        public MiniPlayerController playerMini;
        public MapController mapController;

        public int mapGridSize;
        public int minLevels, avgLevels, expScale;
        public int minPlanetConnections, maxPlanetConnections;
        public int minBossStartDist, minBossShopDist;
        public int minElites, maxElites;
        public int minEligibleBosses;
        
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.LeftBracket))
            {
                for (var i = 1; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
                LevelSelectDataInstance.RevealAll();
                RenderGalaxy(true);
            }
        }

        private void Start()
        {
            Addressables.LoadAssetsAsync<Sprite>(spriteLabel, null).Completed += handle =>
            {
                // only re-generate if it doesn't already exist
                Debug.LogFormat("CurrentPlanet = {0}", LevelSelectDataInstance.CurrentPlanet);
                if (LevelSelectDataInstance.CurrentPlanet == -1) GenerateGalaxy(handle.Result);

                playerMini.SetOrbitRadius(planetPrefab.transform.localScale.x / 2 * 1.5f);
                RenderGalaxy();
                mapController.Instantiate();
            };
        }
        
        private void GenerateGalaxy(IList<Sprite> sprites)
        {
            var levels = new List<LevelData>();
            var usedGridPositions = new List<Vector2>();

            var planetScale = planetPrefab.transform.localScale;
            var planetRadius = Mathf.Max(planetScale.x, planetScale.y) / 2;

            while (true)
            {
                // position represents position in a theoretical discrete grid of planets
                Vector2 position;
                do
                {
                    position = new Vector2(Random.Range(-mapGridSize, mapGridSize+1), Random.Range(-mapGridSize, mapGridSize+1));
                } while (usedGridPositions.Contains(position));
                usedGridPositions.Add(position);

                levels.Add(new LevelData
                {
                    Type = levels.Count == 0 ? LevelType.Entrance : LevelType.Normal,
                    Sprite = levels.Count == 0 ? entranceSprite : sprites[Random.Range(0, sprites.Count)],
                    HiddenSprite = hiddenSprite,
                    Connections = new List<int>(),
                    WorldPosition = planetPrefab.transform.localPosition +
                               (Vector3) (position * planetScale * 2.25f + // make every grid space 2 and a quarter planets wide
                                          Random.insideUnitCircle * planetScale / 2), // offset by up to half a planet

                    Name = "Planet Name",
                    LoreText = "Lore text goes here, idk how long it'll be but decently I would think. Maybe a bit more? How about a teeeensy bit more."
                });

                if (levels.Count >= minLevels && Random.value < 1/(1+Mathf.Exp(expScale*(avgLevels-levels.Count)))) break;
            }

            var connections = new List<Tuple<int, int>>();
            for (var level = 0; level < levels.Count; level++)
            {
                var connectionsLeft = level == 0 ? 1 : Random.Range(minPlanetConnections, maxPlanetConnections+1) - levels[level].Connections.Count;

                var validConnections = levels
                    .Select((_, i) => i)
                    .Where(i =>
                        i != level && i != 0 &&
                        levels[i].Connections.Count < maxPlanetConnections &&
                        !levels[level].Connections.Contains(i) &&
                        !connections.Any(c => MapUtil.Intersect(levels[i].WorldPosition, levels[level].WorldPosition, levels[c.Item1].WorldPosition, levels[c.Item2].WorldPosition)) &&
                        !levels.Any(l => l != levels[i] && l != levels[level] && MapUtil.SegmentAndCircleIntersect(levels[i].WorldPosition, levels[level].WorldPosition, l.WorldPosition, planetRadius*2)))
                    .ToList();

                while (connectionsLeft > 0 && validConnections.Count > 0)
                {
                    var connectionIdx = Random.Range(0, validConnections.Count);
                    var other = validConnections[connectionIdx];

                    connections.Add(new Tuple<int, int>(level, other));

                    levels[level].Connections.Add(other);
                    levels[other].Connections.Add(level);

                    validConnections.RemoveAt(connectionIdx);

                    connectionsLeft--;
                }
            }
            
            for (var level = 1; level < levels.Count; level++)
            {
                if (levels[level].Connections.Count >= minPlanetConnections) continue;

                var validConnections = levels
                    .Select((_, i) => i)
                    .Where(i =>
                        i != level && i != 0 &&
                        levels[i].Connections.Count > 0 && levels[i].Connections.Count < maxPlanetConnections &&
                        !levels[level].Connections.Contains(i) &&
                        !connections.Any(c => MapUtil.Intersect(levels[i].WorldPosition, levels[level].WorldPosition,
                            levels[c.Item1].WorldPosition, levels[c.Item2].WorldPosition)) &&
                        !levels.Any(l =>
                            l != levels[i] && l != levels[level] && MapUtil.SegmentAndCircleIntersect(
                                levels[i].WorldPosition, levels[level].WorldPosition, l.WorldPosition,
                                planetRadius * 2))).ToList();

                for (var i = 0; i < minPlanetConnections - levels[level].Connections.Count; i++)
                {
                    if (validConnections.Count == 0) break;
                    
                    var other = validConnections[Random.Range(0, validConnections.Count-1)];
                    validConnections.Remove(other);
                    
                    connections.Add(new Tuple<int, int>(level, other));

                    levels[level].Connections.Add(other);
                    levels[other].Connections.Add(level);
                }

                if (levels[level].Connections.Count < minPlanetConnections)
                {
                    // couldn't be fixed :( we must redo regeneration
                    GenerateGalaxy(sprites);
                    return;
                }
            }
            
            int stationIdx;
            do
            {
                stationIdx = Random.Range(1, levels.Count - 1);
            } while (levels[stationIdx].Connections.Contains(0));

            levels[stationIdx].Type = LevelType.SpaceStation;
            levels[stationIdx].Sprite = spaceStationSprite;
            
            // make the furthest planet be the boss
            var eligibleBosses = new List<int>();
            for (var i = 1; i < levels.Count; i++)
            {
                var distStart = MapUtil.GetShortestPath(levels.ToArray(), levels[i], levels[0].WorldPosition)?.Length ?? 0;
                var distShop = MapUtil.GetShortestPath(levels.ToArray(), levels[i], levels[stationIdx].WorldPosition)?.Length ?? 0;
                
                if (distStart >= minBossStartDist && distShop >= minBossShopDist) eligibleBosses.Add(i);
            }

            if (eligibleBosses.Count < minEligibleBosses)
            {
                GenerateGalaxy(sprites);
                return;
            }

            var bossPlanet = eligibleBosses[Random.Range(0, eligibleBosses.Count)];
            levels[bossPlanet].IsBoss = true;
            
            for (var i = 0; i < Random.Range(minElites, maxElites); i++)
            {
                var eliteIdx = Random.Range(1, levels.Count-2);
                if (eliteIdx >= Mathf.Min(stationIdx, bossPlanet)) eliteIdx++;
                if (eliteIdx >= Mathf.Max(stationIdx, bossPlanet)) eliteIdx++;

                levels[eliteIdx].Type = LevelType.Elite;
            }

            LevelSelectDataInstance.PopulateData(levels.ToArray(), connections.ToArray());
        }

        private void RenderGalaxy(bool revealAll = false)
        {
            var revealed = new HashSet<int>();
            foreach (var connection in LevelSelectDataInstance.Connections)
            {
                var reveal = revealAll || LevelSelectDataInstance.VisitedPlanets.Contains(connection.Item1) ||
                    LevelSelectDataInstance.VisitedPlanets.Contains(connection.Item2);

                if (reveal)
                {
                    revealed.Add(connection.Item1);
                    revealed.Add(connection.Item2);
                }

                var line = new GameObject("LineRenderer").AddComponent<LineRenderer>();
                line.transform.SetParent(transform);

                line.material = lineMaterial;
                line.startColor = line.endColor = reveal ? Color.red : new Color(0.2f, 0.2f, 0.2f);
                line.startWidth = line.endWidth = 3f;

                line.SetPosition(0, LevelSelectDataInstance.Levels[connection.Item1].WorldPosition);
                line.SetPosition(1, LevelSelectDataInstance.Levels[connection.Item2].WorldPosition);
            }

            foreach (var (level, idx) in LevelSelectDataInstance.Levels.Select((l,i) => (l,i)))
            {
                var planetObj = Instantiate(planetPrefab, level.WorldPosition, Quaternion.identity, transform);
                planetObj.SetActive(true);
                planetObj.GetComponent<SpriteRenderer>().sprite = revealed.Contains(idx)
                    ? level.Sprite
                    : level.HiddenSprite;
                
                if (revealed.Contains(idx))
                {
                    if (!LevelSelectDataInstance.VisitedPlanets.Contains(idx)) planetObj.GetComponent<Selectable>().clickable = true;
                    if (level.Type == LevelType.Elite) planetObj.transform.GetChild(0).gameObject.SetActive(true);
                }
                if (level.IsBoss) planetObj.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
}
