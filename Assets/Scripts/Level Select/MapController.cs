using System;
using System.Collections.Generic;
using UnityEngine;

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
        
        private LineRenderer _lineRenderer;

        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            
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
            
            selector.OnSelectionChange += DrawPathToPlanet;
        }

        private void DrawPathToPlanet(Vector3? planet)
        {
            var current = data.Levels[data.CurrentPlanet];

            var enable = planet.HasValue && current.WorldPosition != planet.Value;
            _lineRenderer.enabled = enable;
            if (!enable) return;
            
            // TODO: split into individual line renderers between each point
            // TODO: set each renderer's texture scale based on distance between points
            _lineRenderer.SetPositions(GetShortestPath(current, planet.Value));
        }

        private Vector3[] GetShortestPath(LevelData start, Vector3 end)
        {
            if (start.WorldPosition == end) return new Vector3[] {};
            
            var visited = new List<Vector3>();
            var queue = new SortedList<double, (LevelData, double, List<Vector3>)>
            {
                { 0, (start, 0, new List<Vector3> { start.WorldPosition }) }
            };
            
            while (true)
            {
                var (current, distance, path) = queue.Values[0];
                queue.RemoveAt(0);

                foreach (var neighbor in current.Connections)
                {
                    var neighborData = data.Levels[neighbor];
                    var position = neighborData.WorldPosition;
                    
                    if (visited.Contains(position)) continue;
                    visited.Add(position);
                    
                    var newPath = new List<Vector3>(path) { neighborData.WorldPosition };
                    if (position == end) return newPath.ToArray();
                    
                    var newDistance = distance + Vector2.Distance(current.WorldPosition, neighborData.WorldPosition);
                    queue.Add(newDistance, (neighborData, newDistance, newPath));
                }
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