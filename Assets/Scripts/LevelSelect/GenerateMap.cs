using System;
using System.Collections.Generic;
using System.Linq;
using Scriptable_Objects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Util;
using Random = UnityEngine.Random;

namespace LevelSelect
{
    public class GenerateMap : MonoBehaviour
    {
        // has to have PlayerData so that it is initialized
        public PlayerData playerData;

        public Material lineMaterial;
        public GameObject planetPrefab;
        public AssetLabelReference spriteLabel;
        public LevelSelectData data;
        public Sprite hiddenSprite, spaceStationSprite,
            hiddenElite, bossSprite, entranceSprite;
        public MiniPlayerController playerMini;
        public MapController mapController;
        public Selector selector;

        private void Start()
        {
            Addressables.LoadAssetsAsync<Sprite>(spriteLabel, null).Completed += handle =>
            {
                // only re-generate if it doesn't already exist
                Debug.LogFormat("CurrentPlanet = {0}", data.CurrentPlanet);
                if (data.CurrentPlanet == -1) GenerateGalaxy(handle.Result);

                playerMini.SetOrbitRadius(planetPrefab.transform.localScale.x / 2 * 1.5f);
                RenderGalaxy();
                mapController.Instantiate();
            };
        }

        // TODO: debug
        private bool _revealMap;

        // TODO: in the name of all that is holy, add some variables lmao
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
                    position = new Vector2(Random.Range(-3, 4), Random.Range(-3, 4));
                } while (usedGridPositions.Contains(position));
                usedGridPositions.Add(position);

                // TODO: generate basically everything better
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

                if (Random.value < 1/(1+Mathf.Exp(5-levels.Count/2.5f))) break;
            }

            var connections = new List<Tuple<int, int>>();
            for (var level = 0; level < levels.Count; level++)
            {
                var connectionsLeft = Random.Range(1, 4) - levels[level].Connections.Count;

                var validConnections = levels
                    .Select((_, i) => i)
                    .Where(i =>
                        i != level &&
                        levels[i].Connections.Count < 3 &&
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

            // idk if this is a great way to do it but *should* work...
            List<int> pathless = new();
            for (var level = 1; level < levels.Count; level++)
            {
                if (MapUtil.GetShortestPath(levels.ToArray(), levels[level], levels[0].WorldPosition) == null) pathless.Add(level);
            }
            foreach (var level in pathless)
            {
                if (MapUtil.GetShortestPath(levels.ToArray(), levels[level], levels[0].WorldPosition) != null) continue;

                var validConnections = levels
                    .Select((_, i) => i)
                    .Where(i =>
                        i != level &&
                        !pathless.Contains(i) &&
                        !connections.Any(c => MapUtil.Intersect(levels[i].WorldPosition, levels[level].WorldPosition,
                            levels[c.Item1].WorldPosition, levels[c.Item2].WorldPosition)) &&
                        !levels.Any(l =>
                            l != levels[i] && l != levels[level] && MapUtil.SegmentAndCircleIntersect(
                                levels[i].WorldPosition, levels[level].WorldPosition, l.WorldPosition,
                                planetRadius * 2))).ToList();

                if (validConnections.Any(i => levels[i].Connections.Count < 3))
                {
                    validConnections = validConnections.Where(i => levels[i].Connections.Count < 3).ToList();
                }

                if (validConnections.Count == 0) continue;
                var other = validConnections[Random.Range(0, validConnections.Count-1)];
                connections.Add(new Tuple<int, int>(level, other));

                levels[level].Connections.Add(other);
                levels[other].Connections.Add(level);
            }

            // TODO: just for now, to make sure the above works fine
            for (var level = 1; level < levels.Count; level++)
            {
                if (MapUtil.GetShortestPath(levels.ToArray(), levels[level], levels[0].WorldPosition) == null)
                {
                    Debug.LogError("!! PATHLESS EXIST !!");
                    _revealMap = true;
                }
            }

            // TODO: do level type generation better
            var furthestPlanet = 0;
            var furthestPlanetDist = 0;
            for (var i = 1; i < levels.Count; i++)
            {
                var dist = MapUtil.GetShortestPath(levels.ToArray(), levels[i], levels[0].WorldPosition)?.Length ?? 0;

                if (dist > furthestPlanetDist)
                {
                    furthestPlanetDist = dist;
                    furthestPlanet = i;
                }
            }

            levels[furthestPlanet].Type = LevelType.Boss;
            levels[furthestPlanet].Sprite = bossSprite;
            levels[furthestPlanet].HiddenSprite = null;

            int stationIdx;
            do stationIdx = Random.Range(1, levels.Count);
            while (stationIdx == furthestPlanet);

            levels[stationIdx].Type = LevelType.SpaceStation;
            levels[stationIdx].Sprite = spaceStationSprite;
            levels[stationIdx].HiddenSprite = null;

            for (var i = 0; i < Random.Range(0, 2); i++)
            {
                int eliteIdx;
                do eliteIdx = Random.Range(1, levels.Count);
                while (eliteIdx == stationIdx || eliteIdx == furthestPlanet);

                levels[eliteIdx].Type = LevelType.Elite;
                levels[eliteIdx].HiddenSprite = hiddenElite;
            }

            data.PopulateData(levels.ToArray(), connections.ToArray());
        }

        private void RenderGalaxy()
        {
            var shownPlanets = new HashSet<int>();
            foreach (var connection in data.Connections)
            {
                if (!_revealMap && !data.VisitedPlanets.Contains(connection.Item1) &&
                    !data.VisitedPlanets.Contains(connection.Item2)) continue;

                shownPlanets.Add(connection.Item1);
                shownPlanets.Add(connection.Item2);

                var line = new GameObject("LineRenderer").AddComponent<LineRenderer>();
                line.transform.SetParent(transform);

                line.material = lineMaterial;
                line.startColor = line.endColor = Color.red;
                line.startWidth = line.endWidth = 3f;

                line.SetPosition(0, data.Levels[connection.Item1].WorldPosition);
                line.SetPosition(1, data.Levels[connection.Item2].WorldPosition);

            }

            foreach (var planet in shownPlanets)
            {
                var level = data.Levels[planet];
                var planetObj = Instantiate(planetPrefab, level.WorldPosition, Quaternion.identity, transform);
                planetObj.GetComponent<SpriteRenderer>().sprite = _revealMap || data.VisitedPlanets.Contains(planet)
                    ? level.Sprite
                    : level.HiddenSprite;
                if (level.Type == LevelType.Elite && (_revealMap || data.VisitedPlanets.Contains(planet)))
                {
                    planetObj.transform.GetChild(0).gameObject.SetActive(true);
                }

                var selectable = planetObj.GetComponent<Selectable>();
                selectable.selector = selector;
            }
        }
    }
}
