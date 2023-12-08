using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

// TODO: revisit what happens when one switching directly from one to another
namespace LevelSelect
{
    public class LevelInfoController : MonoBehaviour
    {
        public Selector selector;
        public LevelSelectData data;
        
        public float secondsToSlide;
        
        public float xPercent;
        public float yPercent;
        public new Camera camera;
        
        public Transform panelBody;
        public Transform panelEnd;
        
        public RectTransform planetName;
        public RectTransform clickInstructions;
        public Transform planetSprite;
        public Transform eliteOverlay;
        public RectTransform levelDescription;
        public RectTransform difficultyLabel;
        public Transform difficultyContainer;
        public RectTransform lootLabel;
        public Transform lootContainer;
        public RectTransform loreText;

        public Sprite[] difficultySprites;
        public Sprite[] eliteDifficultySprites;
        public Sprite[] lootSprites;
        
        private ExpandOnHover _spriteExpand;
        private SpriteRenderer _panelEndRenderer;

        private BoxCollider2D _collider;

        private Vector2 _textureScale;

        private int _prevSide = 1;
        private int _side = 1;
        
        private int? _previousSelection;
        private int? _selection;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _spriteExpand = planetSprite.GetComponent<ExpandOnHover>();
            var sprite = (_panelEndRenderer = panelEnd.GetComponent<SpriteRenderer>()).sprite;
            
            // idk why it's 256, but that's what I found in testing
            _textureScale =  new Vector2(sprite.textureRect.height, sprite.textureRect.width) / 256;

            selector.OnSelectionChange += planet =>
            {
                // ReSharper disable once AssignmentInConditionalExpression
                if (!planet.HasValue)
                {
                    _selection = null;
                    return;
                }

                LevelData level = null;
                for (var i = 0; i < data.Levels.Length; i++)
                {
                    if (data.Levels[i].WorldPosition != planet) continue;

                    level = data.Levels[i];
                    _selection = i;
                    break;
                }
                
                _side = data.Levels[_selection!.Value].WorldPosition.x > camera.transform.position.x ? -1 : 1;
                if (!_previousSelection.HasValue) _prevSide = _side;

                planetName.GetComponent<TextMeshPro>().text = level!.Name;
                eliteOverlay.gameObject.SetActive(level!.Type == LevelType.Elite && data.VisitedPlanets.Contains(_selection.Value));
                planetSprite.GetComponent<SpriteRenderer>().sprite = data.VisitedPlanets.Contains(_selection.Value) ? level!.Sprite : level!.HiddenSprite;
                // TODO: this can somehow get stuck sometimes... ?
                clickInstructions.gameObject.SetActive(
                    planetSprite.GetComponent<ExpandOnHover>().enabled = 
                        planetSprite.GetComponent<CircleCollider2D>().enabled = 
                            level!.Type != LevelType.Entrance);
                levelDescription.GetComponent<TextMeshPro>().text = level!.Type.Description;
                loreText.GetComponent<TextMeshPro>().text = level!.LoreText;

                // assumes 5 indicators that go from empty to half to full
                var difficulty = (int) (level!.DifficultyScore / data.MaxDifficultyScore * 10) / 2f;
                Debug.Log("score: " + level!.DifficultyScore + " / " + data.MaxDifficultyScore+" ("+difficulty+")");
                var loot = level!.Loot / 9 / 2f;

                for (var i = 0; i < 5; i++)
                {
                    var difficultySprite = (level!.Type == LevelType.Elite || level!.Type == LevelType.Boss ?
                        eliteDifficultySprites :
                        difficultySprites)[(int) (Mathf.Clamp01(difficulty--) * 2)];
                    var lootSprite = lootSprites[(int) (Mathf.Clamp01(loot--) * 2)];

                    difficultyContainer.GetChild(i).GetComponent<SpriteRenderer>().sprite = difficultySprite;
                    lootContainer.GetChild(i).GetComponent<SpriteRenderer>().sprite = lootSprite;
                }
            };

            planetSprite.GetComponent<Button>().OnClick += () =>
            {
                if (!_selection.HasValue) return;
                
                data.CurrentPlanet = _selection!.Value;
                SceneManager.LoadScene("LevelPlay");
            };
        }

        private float _slideTime;

        private void LateUpdate()
        {
            var camSize = camera.orthographicSize;
            var aspect = camera.aspect;
            var height = camSize * 2 * yPercent * _textureScale.y;
            var width = camSize * aspect * 2 * xPercent * _textureScale.x;
            
            transform.localScale = new Vector3(height, height, 1);

            var panelEndWidth = height / _textureScale.x;
            var panelBodyWidth = width - panelEndWidth;
            
            panelBody.localScale = new Vector3(panelBodyWidth / height * _textureScale.x, 1, 1);
            
            var goalPosition = _prevSide * (camSize * aspect - width / 2);
            var startPosition = goalPosition + _prevSide * width;

            var slideIntoView = _previousSelection.HasValue ? _previousSelection == _selection : _selection.HasValue;
            _slideTime = Mathf.Clamp01(_slideTime + (slideIntoView ? 1 : -1) * Time.deltaTime / secondsToSlide);
            if (_slideTime is 0 or 1)
            {
                _previousSelection = _selection;
                _prevSide = _side;
            }

            var time = _slideTime;
            if (!slideIntoView)
            {
                (goalPosition, startPosition) = (startPosition, goalPosition);
                time = 1-time;
            }

            transform.localPosition = new Vector3((goalPosition - startPosition)*Mathf.Log((11)*time+1, 12) + startPosition, 0, transform.localPosition.z);
            panelBody.localPosition = new Vector3(_prevSide * panelEndWidth / 2 / height, 0, 0);
            panelEnd.localPosition = new Vector3(_prevSide * -panelBodyWidth / 2 / height, 0, 0);
            
            _panelEndRenderer.flipX = _prevSide < 0;

            var xTranslate = _side * 7 / height; // 7 px border, just taken from sprite
            var xFactor = width / height;
            var yFactor = 1 / _textureScale.y;

            _collider.size = new Vector2(xFactor, yFactor);
            
            FillTransform(planetName, xTranslate, xFactor, yFactor, y: 0.35f, width: 0.65f);
            FillTransform(clickInstructions, xTranslate, xFactor, yFactor, y: 0.28125f, width: 0.3f, height: 0.03125f);
            FillTransform(planetSprite, xTranslate, xFactor, yFactor, y: 0.15f, width: _spriteExpand.CurrentMultiplier * 0.35f, height: _spriteExpand.CurrentMultiplier * 0.35f * xFactor / yFactor);
            FillTransform(levelDescription, xTranslate, xFactor, yFactor, y: -0.05f, width: 0.65f);
            FillTransform(difficultyLabel, xTranslate, xFactor, yFactor, x: -0.22f, y: -0.19f, width: 0.3f);
            FillTransform(lootLabel, xTranslate, xFactor, yFactor, x: 0.22f, y: -0.19f, width: 0.3f / 2.14f);
            FillTransform(difficultyContainer, xTranslate, xFactor, yFactor, x: -0.22f, y: -0.25f, width: 0.35f, height: 0.35f * xFactor / yFactor);
            FillTransform(lootContainer, xTranslate, xFactor, yFactor, x: 0.22f, y: -0.25f, width: 0.35f, height: 0.35f * xFactor / yFactor);
            FillTransform(loreText, xTranslate, xFactor, yFactor, y: -0.4f, width: 0.75f);
        }

        private static void FillTransform(Transform transform, float xTranslate, float xFactor, float yFactor, float x = 0, float y = 0, float width = 0, float height = 0)
        {
            var position = transform.localPosition;
            position.x = xTranslate + xFactor * x;
            position.y = yFactor * y;
            transform.localPosition = position;
            
            var size = transform is RectTransform rectTransform ? (Vector3) rectTransform.sizeDelta : transform.localScale;
            if (width != 0) size.x = xFactor * width;
            if (height != 0) size.y = yFactor * height;
            if (transform is RectTransform rect) rect.sizeDelta = size;
            else transform.localScale = size; 
        }
    }
}