using System;
using System.Collections.Generic;
using System.Linq;
using Analytics;
using Extensions;
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
        public Sprite eliteSprite, hiddenSprite, spaceStationSprite, entranceSprite;
        public MiniPlayerController playerMini;
        public MapController mapController;

        private Dictionary<Sprite, List<Pair<string, LevelLoreData>>> _availableNames;
        public Triple<Sprite, Pair<string, LevelLoreData>[], float>[] planetNames;
        public string[] spaceStationNames;
        public LoreData entranceLore, stationLore;
        public LevelLoreData carcLore;
        public float carcPlanetScale;
        private List<string> _spaceStationNames;
        
#if UNITY_EDITOR
        private void Update()
        {
            if (InputManager.GetKeyUp(KeyCode.LeftBracket))
            {
                for (var i = 2; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
                LevelSelectDataInstance.RevealAll();
                RenderGalaxy(true);
            }
        }
#endif

        private void Start()
        {
            _availableNames = planetNames.ToDictionary(p=>p.a, t=>new List<Pair<string, LevelLoreData>>(t.b));
            _spaceStationNames = new List<string>(spaceStationNames);
            

            // only re-generate if it doesn't already exist
            if (LevelSelectDataInstance.CurrentPlanet == -1) GenerateGalaxy();

            playerMini.SetOrbitRadius(planetPrefab.transform.localScale.x / 2 * 2f);
            RenderGalaxy();
            mapController.Instantiate();
            
            AnalyticsManager.LogEvent(new VisitScreenEvent { ScreenId = "level_select"});
        }

        // TODO: eventually all planets should have lore so this should be vestigial
        private Pair<string, LevelLoreData> GetPlanet(Sprite sprite)
        {
            var list = new List<Pair<string, LevelLoreData>>(_availableNames[sprite]);
            
            list.RemoveAll(p => p.b.localName.Length == 0);
            if (list.Count == 0) return _availableNames[sprite].Pop(Random.Range(0, _availableNames[sprite].Count));
            
            var ret = list[Random.Range(0, list.Count)];
            _availableNames[sprite].RemoveAll(p=>p.a == ret.a);

            return ret;
        }
        
        private void GenerateGalaxy()
        {
            var levels = new List<LevelData>();
            var connections = new List<Tuple<int, int>>();

            var planetScale = planetPrefab.transform.localScale;
            
            levels.Add(new LevelData {
                    Type = LevelType.Entrance,
                    SpriteData = new LevelSpriteData
                    {
                        Sprite = entranceSprite,
                        HiddenSprite = hiddenSprite
                    },
                    Connections = new List<int>{1},
                    WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(0, 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                    Title = "Entrance Portal",
                    LoreData =  entranceLore,
                    Description = "No way home",
                    Difficulty = Difficulty.Entrance
            });

            int i;
            for (i = 0; i < (PlayerDataInstance.IsTutorial ? 1 : 5); i++)
            {
                var sprite = planetNames.ElementAt(PlayerDataInstance.IsTutorial ? 0 : Random.Range(0, planetNames.Length));
                var lore = GetPlanet(sprite);
                levels.Add(new LevelData {
                    Type = LevelType.Normal,
                    SpriteData = new LevelSpriteData
                    {
                        Sprite = sprite,
                        HiddenSprite = hiddenSprite,
                        RadiusMult = sprite
                    },
                    Connections = new List<int>{i,i+2},
                    WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(3*(i+1), 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                    Title = lore,
                    LoreData = lore.b.localName.Length > 0 ? lore : null,
                    Description = "Cult of the Void controlled"
                });
                connections.Add(new Tuple<int, int>(i, i+1));
            }
            
            levels.Add(new LevelData {
                Type = LevelType.SpaceStation,
                SpriteData = new LevelSpriteData
                {
                    Sprite = spaceStationSprite,
                    HiddenSprite = hiddenSprite,
                    RadiusMult = 1.5f,
                },
                Connections = new List<int>{i, i+2},
                WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(3*++i, 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                Title = _spaceStationNames.Pop(Random.Range(0, _spaceStationNames.Count)),
                LoreData = stationLore,
                Description = "A respite for the weary",
                Metadata = ShopMetadata.Generate()
            });
            connections.Add(new Tuple<int, int>(i-1, i));

            if (PlayerDataInstance.IsTutorial)
            {
                var sprite = planetNames.ElementAt(0);
                var lore = GetPlanet(sprite);
                levels.Add(new LevelData {
                    Type = LevelType.Tutorial,
                    SpriteData = new LevelSpriteData
                    {
                        Sprite = sprite,
                        HiddenSprite = hiddenSprite,
                        RadiusMult = sprite
                    },
                    Connections = new List<int>{i},
                    WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(3*++i, 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                    Title = lore,
                    LoreData = lore.b.localName.Length > 0 ? lore : null,
                    Description = "Complete the tutorial"
                });
            }
            else
            {
                levels.Add(new LevelData {
                    Type = LevelType.Elite,
                    SpriteData = new LevelSpriteData
                    {
                        Sprite = eliteSprite,
                        HiddenSprite = hiddenSprite,
                        RadiusMult = carcPlanetScale
                    },
                    Connections = new List<int>{i},
                    WorldPosition = planetPrefab.transform.localPosition + (Vector3) (new Vector2(3*++i, 0) * planetScale * 2f + Random.insideUnitCircle * planetScale / 2),
                    Title = "Deep Space",
                    LoreData = carcLore,
                    Description = "Home of the Carcadon"
                });
            }
            connections.Add(new Tuple<int, int>(i-1, i));

            LevelSelectDataInstance.PopulateData(levels.ToArray(), connections.ToArray());
            LevelSelectDataInstance.CurrentPlanet = PlayerDataInstance.IsTutorial ? 1 : 0;
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

                    var dir = (pos2 - pos1).normalized;
                    var distance = planetPrefab.transform.localScale.x / 4;
                    line.SetPosition(0, pos1 + dir * distance);
                    line.SetPosition(1, pos2 - dir * distance);
                }
            }

            foreach (var (level, idx) in LevelSelectDataInstance.Levels.Select((l,i) => (l,i)))
            {
                var planetObj = Instantiate(planetPrefab, level.WorldPosition, Quaternion.identity, transform);
                planetObj.SetActive(true);
                planetObj.GetComponent<SpriteRenderer>().sprite = revealed.Contains(idx)
                    ? level.SpriteData.Sprite
                    : level.SpriteData.HiddenSprite;
                if (revealed.Contains(idx)) planetObj.transform.localScale *= level.SpriteData.RadiusMult;

                var travellable = (revealed.Contains(idx) && !LevelSelectDataInstance.VisitedPlanets.Contains(idx)) ||
                                 (level.Type == LevelType.SpaceStation && LevelSelectDataInstance.CurrentPlanet == idx);
                level.Travellable = travellable;
                planetObj.GetComponent<PlanetController>().Clickable = revealed.Contains(idx);
                if (!travellable)
                {
                    planetObj.GetComponent<ScaleUI>().maxScaleMult = 1.1f;
                    planetObj.GetComponent<ScaleUI>().time = Random.Range(1.8f, 2.2f);
                }
                planetObj.GetComponent<PlanetController>().LevelIdx = idx;

                if (revealed.Contains(idx) && !travellable)
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
