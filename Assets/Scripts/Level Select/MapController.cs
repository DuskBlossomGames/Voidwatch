using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level_Select
{
    public class MapController : MonoBehaviour
    {
        public LevelSelectData data;
        public new Camera camera;
        public float camSizeDragRatio;
        public float scrollSpeed, minCamSize, maxCamSize;

        public MiniPlayerController playerMini;
        public Selector selector;

        private Vector2 _minScroll;
        private Vector2 _maxScroll;

        private void Start()
        {
            data.OnPopulate += () =>
            {
                foreach (var level in data.Levels)
                {
                    _minScroll = Vector3.Min(level.WorldPosition, _minScroll);
                    _maxScroll = Vector3.Max(level.WorldPosition, _maxScroll);
                }

                var camTransform = camera.transform;
                
                var pos = data.Levels[data.CurrentPlanet].WorldPosition;
                pos.z = camTransform.position.z;

                camTransform.position = pos;
                playerMini.SetOrbitPosition(pos);
            };
        }
        
        private void OnMouseUpAsButton()
        {
            selector.SetPosition(null);
        }

        /* --------------------
         * |   CAMERA STUFF   |
         * --------------------*/
        
        private Vector3 _lastMousePos;
        private void OnMouseDown() { _lastMousePos = Input.mousePosition; }
        private void OnMouseDrag()
        {
            var camTransform = camera.transform;
            var camPos = camTransform.position + (_lastMousePos - Input.mousePosition) * camera.orthographicSize / camSizeDragRatio;

            camPos.x = Mathf.Clamp(camPos.x, _minScroll.x, _maxScroll.x);
            camPos.y = Mathf.Clamp(camPos.y, _minScroll.y, _maxScroll.y);
            
            camTransform.position = camPos;
            _lastMousePos = Input.mousePosition;
        }
        
        // TODO: scale stars accordingly?
        // TODO: zoom to mouse?
        private void Update()
        {
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + Input.mouseScrollDelta.y * scrollSpeed,
                minCamSize, maxCamSize);
        }
    }
}