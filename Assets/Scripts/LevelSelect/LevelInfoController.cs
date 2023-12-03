using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

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

        private SpriteRenderer _panelEndRenderer;

        private Vector2 _textureScale;

        private int _prevSide = 1;
        private int _side = 1;
        
        private int? _previousSelection;
        private int? _selection;

        private void Awake()
        {
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
                
                for (var i = 0; i < data.Levels.Length; i++)
                {
                    if (data.Levels[i].WorldPosition != planet) continue;
                        
                    _selection = i;
                    break;
                }
                
                // TODO: make popup show from either left or right of screen
                // TODO: set planet info on popup
                _side = data.Levels[_selection!.Value].WorldPosition.x > camera.transform.position.x ? -1 : 1;
                if (!_previousSelection.HasValue) _prevSide = _side;
            };

            panelBody.GetComponentInChildren<Button>().OnClick += () =>
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

            transform.localPosition = new Vector3((goalPosition - startPosition)*Mathf.Log((11)*time+1, 12) + startPosition, 0, 1);
            panelBody.localPosition = new Vector3(_prevSide * panelEndWidth / 2 / height, 0, 0);
            panelEnd.localPosition = new Vector3(_prevSide * -panelBodyWidth / 2 / height, 0, 0);
            
            _panelEndRenderer.flipX = _prevSide < 0;
        }
    }
}