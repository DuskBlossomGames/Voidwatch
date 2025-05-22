using System;
using System.Collections.Generic;
using System.Linq;
using Static_Info;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
        public int minBossStartDist, minBossShopDist, minEliteDist;
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
            var connections = new List<Tuple<int, int>>();

            var planetScale = planetPrefab.transform.localScale;
            
            levels.Add(new LevelData {
                    Type = LevelType.Entrance,
                    Sprite = entranceSprite,
                    HiddenSprite = hiddenSprite,
                    Connections = new List<int>{1},
                    WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(0, 0) * planetScale * 2.25f + Random.insideUnitCircle * planetScale / 2),
                    Name = "Entrance",
                    LoreText = ""
            });

            for (var i = 0; i < 5; i++)
            {
                levels.Add(new LevelData {
                    Type = LevelType.Normal,
                    Sprite = sprites[Random.Range(0, sprites.Count)],
                    HiddenSprite = hiddenSprite,
                    Connections = new List<int>{i,i+2},
                    WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(i+1, 0) * planetScale * 2.25f + Random.insideUnitCircle * planetScale / 2),
                    Name = "Planet",
                    LoreText = ""
                });
                connections.Add(new Tuple<int, int>(i, i+1));
            }
            
            levels.Add(new LevelData {
                Type = LevelType.SpaceStation,
                Sprite = spaceStationSprite,
                HiddenSprite = hiddenSprite,
                Connections = new List<int>{5, 7},
                WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(6, 0) * planetScale * 2.25f + Random.insideUnitCircle * planetScale / 2),
                Name = "Space Station",
                LoreText = ""
            });
            connections.Add(new Tuple<int, int>(5, 6));
            
            levels.Add(new LevelData {
                Type = LevelType.Elite,
                Sprite = sprites[Random.Range(0, sprites.Count)],
                HiddenSprite = hiddenSprite,
                Connections = new List<int>{6},
                WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(7, 0) * planetScale * 2.25f + Random.insideUnitCircle * planetScale / 2),
                Name = "Elite Enemy",
                LoreText = ""
            });
            connections.Add(new Tuple<int, int>(6, 7));

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
                    if (level.Type == LevelType.SpaceStation || !LevelSelectDataInstance.VisitedPlanets.Contains(idx)) planetObj.GetComponent<Selectable>().clickable = true;
                    if (level.Type == LevelType.Elite) planetObj.transform.GetChild(0).gameObject.SetActive(true);
                }
                if (level.IsBoss) planetObj.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
}
