using UnityEngine;
using Util;

using static Static_Info.LevelSelectData;
namespace LevelSelect
{
    public class MapController : MonoBehaviour
    {
        public new Camera camera;
        public float camSizeDragRatio;
        public float scrollSpeed, minCamSize, maxCamSize;

        public MiniPlayerController playerMini;
        public Selector selector;
        public Material lineMaterial;

        private Vector2 _minScroll;
        private Vector2 _maxScroll;
        
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
            
            selector.OnSelectionChange += DrawPathToPlanet;
        }

        private void DrawPathToPlanet(Vector3? planet)
        {
            var current = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

            foreach (var line in GetComponentsInChildren<LineRenderer>())
            {
                Destroy(line.gameObject);
            }

            if (!planet.HasValue || current.WorldPosition == planet.Value) return;

            // TODO: undiscovered paths
            var path = MapUtil.GetShortestPath(LevelSelectDataInstance.Levels, current, planet.Value, LevelSelectDataInstance.VisitedPlanets);
            for (var i = 0; i < path.Length-1; i++)
            {
                var line = new GameObject("LineRenderer").AddComponent<LineRenderer>();
                line.transform.SetParent(transform);

                line.material = lineMaterial;
                line.startWidth = line.endWidth = 1.2f;
                line.startColor = line.endColor = Color.yellow;
                line.textureMode = LineTextureMode.RepeatPerSegment;
                line.sortingOrder = 1;

                line.textureScale = new Vector2(Vector2.Distance(path[i], path[i+1])/100 * 6, 0);
                line.SetPositions(new[] { path[i], path[i+1] });
            }
        }
        
        private Vector3 _lastMousePos;
        private bool _resettingSelector;
        
        private void OnMouseUp()
        {
            if (_resettingSelector) selector.SetPosition(null);
        }

        private void OnMouseDown()
        {
            _resettingSelector = true;
            _lastMousePos = Input.mousePosition;
        }
        private void OnMouseDrag()
        {
            _resettingSelector &= Input.mousePosition == _lastMousePos;
            
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