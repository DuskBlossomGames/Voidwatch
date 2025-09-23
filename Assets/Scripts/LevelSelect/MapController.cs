using System.Collections;
using Analytics;
using UnityEngine;
using Util;
using static Singletons.Static_Info.LevelSelectData;
namespace LevelSelect
{
    public class MapController : MonoBehaviour
    {
        public new Camera camera;
        public float scrollSpeed, minCamSize, maxCamSize;

        public MiniPlayerController playerMini;
        public Selector selector;

        private Vector2 _minScroll;
        private Vector2 _maxScroll;

        private bool _enabled;
        
        public void Instantiate()
        {
            foreach (var level in LevelSelectDataInstance.Levels)
            {
                _minScroll = Vector3.Min(level.WorldPosition, _minScroll);
                _maxScroll = Vector3.Max(level.WorldPosition, _maxScroll);
            }

            var camTransform = camera.transform;
            
            var pos = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet].WorldPosition;
            pos.z = camTransform.position.z;

            camTransform.position = pos;
            playerMini.SetOrbitPosition(pos);
            
            StartCoroutine(EnableLater());
        }

        private IEnumerator EnableLater()
        {
            yield return new WaitForSeconds(1f);
            _enabled = true;
            _lastMousePos = InputManager.mousePosition;
        }

        
        private Vector3 _lastMousePos;
        private bool _resettingSelector;

        private bool _sentMapPan, _sentMapZoom;
        
        private void OnMouseUp()
        {
            if (!_enabled) return;
            
            if (_resettingSelector) selector.SetPosition(null);
        }

        private void OnMouseDown()
        {
            if (!_enabled) return;
            
            _resettingSelector = true;
            _lastMousePos = InputManager.mousePosition;
        }
        private void OnMouseDrag()
        {
            if (!_enabled) return;
            
            _resettingSelector &= InputManager.mousePosition == _lastMousePos;
            
            var camTransform = camera.transform;
            var camPos = camTransform.position + (camera.ScreenToWorldPoint(_lastMousePos) -
                                                  camera.ScreenToWorldPoint(InputManager.mousePosition));

            camPos.x = Mathf.Clamp(camPos.x, _minScroll.x, _maxScroll.x);
            camPos.y = Mathf.Clamp(camPos.y, _minScroll.y, _maxScroll.y);
            
            camTransform.position = camPos;

            if (!_sentMapPan && _lastMousePos != InputManager.mousePosition)
            {
                AnalyticsManager.LogEvent(new PanMapEvent());
                _sentMapPan = true;
            }
            
            _lastMousePos = InputManager.mousePosition;
        }
        
        private void Update()
        {
            if (!_enabled) return;
            
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - InputManager.mouseScrollDelta.y * scrollSpeed,
                minCamSize, maxCamSize);

            if (!_sentMapZoom && InputManager.mouseScrollDelta.y != 0)
            {
                AnalyticsManager.LogEvent(new ZoomMapEvent());
                _sentMapZoom = true;
            }
        }
    }
}