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
        
        public float xPercent;
        public float yPercent;
        public new Camera camera;

        public Transform panelBody;
        public Transform panelEnd;

        public String sceneName;
        
        private Vector2 _textureScale;
        private bool _shown;

        private int _selectedLevel;

        private void Awake()
        {
            var sprite = panelEnd.GetComponent<SpriteRenderer>().sprite;
            
            // idk why it's 256, but that's what I found in testing
            _textureScale =  new Vector2(sprite.textureRect.height, sprite.textureRect.width) / 256;

            selector.OnSelectionChange += planet =>
            {
                // ReSharper disable once AssignmentInConditionalExpression
                if (!(_shown = planet.HasValue)) return;
                
                for (var i = 0; i < data.Levels.Length; i++)
                {
                    if (data.Levels[i].WorldPosition != planet) continue;
                        
                    _selectedLevel = i;
                    break;
                }
                // TODO: make popup show from either left or right of screen
                // TODO: set planet info on popup
            };

            panelBody.GetComponentInChildren<Button>().OnClick += () =>
            {
                data.CurrentPlanet = _selectedLevel;
                SceneManager.LoadScene(sceneName);
            };
        }

        private void LateUpdate()
        {
            panelBody.gameObject.SetActive(_shown);
            panelEnd.gameObject.SetActive(_shown);
            if (!_shown) return;
            
            var camSize = camera.orthographicSize;
            var aspect = camera.aspect;
            
            var height = camSize * 2 * yPercent * _textureScale.y;
            transform.localScale = new Vector3(height, height, 1);

            var width = camSize * aspect * 2 * xPercent * _textureScale.x;
            var panelEndWidth = height / _textureScale.x;
            var panelBodyWidth = width - panelEndWidth;
            
            panelBody.localScale = new Vector3(panelBodyWidth / height * _textureScale.x, 1, 1);
            
            transform.localPosition = new Vector3(camSize * aspect - width / 2, 0, 1);
            panelBody.localPosition = new Vector3(panelEndWidth / 2 / height, 0, 0);
            panelEnd.localPosition = new Vector3(-panelBodyWidth / 2 / height, 0, 0);
        }
    }
}