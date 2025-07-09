using System;
using System.Collections.Generic;
using System.Linq;
using Singletons.Static_Info;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Util;
using Random = UnityEngine.Random;
using static Singletons.Static_Info.LevelSelectData;
using static Singletons.Static_Info.PlayerData;

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
        
#if UNITY_EDITOR
        private void Update()
        {
            if (InputManager.GetKeyUp(KeyCode.LeftBracket))
            {
                for (var i = 1; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
                LevelSelectDataInstance.RevealAll();
                RenderGalaxy(true);
            }
        }
#endif

        private void Start()
        {
            Addressables.LoadAssetsAsync<Sprite>(spriteLabel, null).Completed += handle =>
            {
                // only re-generate if it doesn't already exist
                if (LevelSelectDataInstance.CurrentPlanet == -1) GenerateGalaxy(handle.Result);

                playerMini.SetOrbitRadius(planetPrefab.transform.localScale.x / 2 * 2f);
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
                    WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(0, 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                    Name = "Entrance",
                    LoreText = ""
            });

            int i;
            for (i = 0; i < (PlayerDataInstance.IsTutorial ? 1 : 5); i++)
            {
                levels.Add(new LevelData {
                    Type = LevelType.Normal,
                    Sprite = sprites[Random.Range(0, sprites.Count)],
                    HiddenSprite = hiddenSprite,
                    Connections = new List<int>{i,i+2},
                    WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(3*(i+1), 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                    Name = "Planet",
                    LoreText = ""
                });
                connections.Add(new Tuple<int, int>(i, i+1));
            }
            
            levels.Add(new LevelData {
                Type = LevelType.SpaceStation,
                Sprite = spaceStationSprite,
                HiddenSprite = hiddenSprite,
                Connections = new List<int>{i, i+2},
                WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(3*++i, 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                Name = "Space Station",
                LoreText = ""
            });
            connections.Add(new Tuple<int, int>(i-1, i));
            
            levels.Add(new LevelData {
                Type = PlayerDataInstance.IsTutorial ? LevelType.Tutorial : LevelType.Elite,
                Sprite = sprites[Random.Range(0, sprites.Count)],
                HiddenSprite = hiddenSprite,
                Connections = new List<int>{i},
                WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(3*++i, 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                Name = "Elite Enemy",
                LoreText = ""
            });
            connections.Add(new Tuple<int, int>(i-1, i));

            LevelSelectDataInstance.PopulateData(levels.ToArray(), connections.ToArray());
            LevelSelectDataInstance.CurrentPlanet = 1;
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

                if (reveal)
                {
                    var pos1 = LevelSelectDataInstance.Levels[connection.Item1].WorldPosition;
                    var pos2 = LevelSelectDataInstance.Levels[connection.Item2].WorldPosition;
                    
                    var line = new GameObject("LineRenderer").AddComponent<LineRenderer>();
                    line.transform.SetParent(transform);
                    
                    line.material = lineMaterial;
                    line.startColor = line.endColor = new Color(0.2f, 0.2f, 0.2f);
                    line.startWidth = line.endWidth = 2;
                    line.textureMode = LineTextureMode.RepeatPerSegment;
                    line.textureScale = new Vector2(Vector2.Distance(pos1, pos2)/100 * 3, 0);

                    line.SetPosition(0, pos1);
                    line.SetPosition(1, pos2);
                }
            }

            foreach (var (level, idx) in LevelSelectDataInstance.Levels.Select((l,i) => (l,i)))
            {
                var planetObj = Instantiate(planetPrefab, level.WorldPosition, Quaternion.identity, transform);
                planetObj.SetActive(true);
                planetObj.GetComponent<SpriteRenderer>().sprite = revealed.Contains(idx)
                    ? level.Sprite
                    : level.HiddenSprite;

                planetObj.GetComponent<PlanetController>().Clickable =
                    revealed.Contains(idx) && (level.Type == LevelType.SpaceStation ||
                    !LevelSelectDataInstance.VisitedPlanets.Contains(idx));
                planetObj.GetComponent<PlanetController>().LevelIdx = idx;

                if (revealed.Contains(idx) && level.Type != LevelType.SpaceStation &&
                    LevelSelectDataInstance.VisitedPlanets.Contains(idx))
                {
                    planetObj.GetComponent<SpriteRenderer>().color = new Color(0.6f, 0.6f, 0.6f);
                }

                if (!revealed.Contains(idx))
                {
                    planetObj.GetComponent<SpriteRenderer>().color = new Color(0.4f, 0.4f, 0.4f);
                }
            }
        }
    }
}
