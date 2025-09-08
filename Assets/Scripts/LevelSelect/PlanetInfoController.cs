using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using Extensions;
using Singletons.Static_Info;
using TMPro;
using UnityEngine;
using Util;
using static Singletons.Static_Info.LevelSelectData;

namespace LevelSelect
{
    public class PlanetInfoController : MonoBehaviour
    {
        public Sprite infoSprite, loreSprite;
        
        public Selector selector;
        public float yOffset;
        public float fadeTime;
        public GameObject planetPrefab;
        public float camLoreSize;
        
        [NonSerialized] public PlanetController SelectedPlanet;
        
        private TextMeshPro _title, _description, _difficulty, _travel, _shift;
        private SpriteRenderer _background;
        private ScaleListener _scaleListener;

        private TextMeshPro _loreMain, _loreRight;
        private string _loreMainFormat, _loreMainReducedFormat, _loreRightFormat;

        private LevelData _level;
        private Vector3 _startPos, _baseScale;
        
        private readonly Timer _fadeTimer = new();
        private bool _shown, _hasLore, _showingLore;
        private bool _hideLore;

        private void Awake()
        {
            _fadeTimer.Value = fadeTime;
            _fadeTimer.SetValue(0);

            _title = transform.GetChild(0).GetComponentsInChildren<TextMeshPro>()[0];
            _description = transform.GetChild(0).GetComponentsInChildren<TextMeshPro>()[1];
            _difficulty = transform.GetChild(0).GetComponentsInChildren<TextMeshPro>()[2];
            _travel = transform.GetChild(0).GetComponentsInChildren<TextMeshPro>()[3];
            _shift = transform.GetChild(0).GetComponentsInChildren<TextMeshPro>()[4];

            _loreMain = transform.GetChild(1).GetComponentsInChildren<TextMeshPro>()[0];
            _loreMainFormat = _loreMain.text.Replace("|", "");
            _loreMainReducedFormat = new Regex("\\|(.|\n)*\\|").Replace(_loreMain.text, "").Replace("{3}", "{1}");
            _loreRight = transform.GetChild(1).GetComponentsInChildren<TextMeshPro>()[1];
            _loreRightFormat = _loreRight.text;
            
            _background = GetComponent<SpriteRenderer>();
            _scaleListener = GetComponent<ScaleListener>();
            
            _baseScale = transform.localScale;
            
            selector.OnSelectionChange += pos =>
            {
                _shown = pos != null;
                if (!_shown) return;
                _hideLore = true;

                _level = LevelSelectDataInstance.Levels.First(l => l.WorldPosition == pos);
                
                transform.position = _startPos = pos!.Value + (yOffset + planetPrefab.transform.localScale.x * (_level.SpriteData.RadiusMult-1)) * Vector3.up;
                transform.position += _showingLore ? 1.5f * _baseScale.y * Vector3.up : Vector3.zero;

                _title.text = _level.Title;
                _description.text = _level.Description;
                _difficulty.text = _level.Difficulty.Text;
                _difficulty.color = _level.Difficulty.Color;
                _travel.text = _level.Travellable ? "Click to Travel" : "Cannot Travel";
                _travel.color = Color.HSVToRGB(0, 0, _level.Travellable ? 1 : 0.75f);

                _hasLore = _level.LoreData != null;
                _shift.text = _hasLore ? "Shift for Info" : "Info Unavailable";
                _shift.color = Color.HSVToRGB(0, 0, _hasLore ? 1 : 0.75f);

                if (_level.LoreData == null) return;
                if (_level.LoreData.GetType() == typeof(LevelLoreData))
                {
                    var lld = (LevelLoreData) _level.LoreData;
                    _loreMain.text = string.Format(_loreMainFormat, _level.Title.ToUpper(), lld.discovered,
                        lld.voidInfluence, lld.lore);
                    _loreRight.enabled = true;
                    _loreRight.text = string.Format(_loreRightFormat, lld.localName.ToUpper());
                }
                else
                {
                    _loreMain.text = string.Format(_loreMainReducedFormat, _level.Title.ToUpper(), _level.LoreData.lore);
                    _loreRight.enabled = false;
                }
            };
        }
        private void Update()
        {
            _fadeTimer.Update(_shown ? 1 : -1);
            _title.SetAlpha(_fadeTimer.Progress);
            _description.SetAlpha(_fadeTimer.Progress);
            _difficulty.SetAlpha(_fadeTimer.Progress);
            _background.SetAlpha(_fadeTimer.Progress);
            _travel.SetAlpha(_fadeTimer.Progress);
            _shift.SetAlpha(_fadeTimer.Progress);
            _loreMain.SetAlpha(_fadeTimer.Progress);
            _loreRight.SetAlpha(_fadeTimer.Progress);
            if (!_shown && _fadeTimer.IsFinished && _showingLore) _hideLore = true;

            bool? lore = _hideLore || InputManager.GetKeyUp(KeyCode.LeftShift) ? false : InputManager.GetKeyDown(KeyCode.LeftShift) ? true : null;
            if (lore != null && ((_shown && _hasLore) || !lore.Value))
            {
                _hideLore = false;
                
                _showingLore = lore.Value;
                _background.sprite = _showingLore ? loreSprite : infoSprite;
                _scaleListener.SetScaleMult(_showingLore ? 4 : 1);
                transform.position = _startPos + (_showingLore ? 1.5f*_baseScale.y*Vector3.up : Vector3.zero);
                
                transform.GetChild(0).gameObject.SetActive(!_showingLore);
                transform.GetChild(1).gameObject.SetActive(_showingLore);

                if (_showingLore)
                {
                    var cam = Camera.main!;
                    if (cam.orthographicSize < camLoreSize) StartCoroutine(ExpandCamera());
                }
            }
        }

        private IEnumerator ExpandCamera()
        {
            var cam = Camera.main!;
            var start = cam.orthographicSize;
            for (float t = 0; t < 0.7f; t += Time.fixedDeltaTime)
            {
                cam.orthographicSize = Mathf.SmoothStep(start, camLoreSize, t / 0.7f);
                yield return new WaitForFixedUpdate();
            }
        }

        private void OnMouseUpAsButton()
        {
            if (!_showingLore) SelectedPlanet.OnMouseUpAsButton();
        }
    }
}