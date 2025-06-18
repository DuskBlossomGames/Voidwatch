using System.Collections.Generic;
using UnityEngine;

namespace Spawnables
{
    public class FloppyController : MonoBehaviour
    {
        public float segmentLength;
        public float velocityDamping;
        public float percSizeDecay;
        public uint numSegments;
        public GameObject segmentPrefab;

        private List<Vertex> _vertices = new();
        private List<GameObject> _edges = new();
        private List<Vector2> _oldEdgePos = new();

        public class Vertex
        {
            public Vector2 prevPos;
            public Vector2 currPos;
            public float segLen;
            public float damping;
            public float prevDt;

            public Vertex(Vector2 position, float segmentLenth, float dampingFactor)
            {
                currPos = prevPos = position;
                segLen = segmentLenth;
                damping = dampingFactor;
                prevDt = 1;
            }

            public void UpdatePos(float dt)
            {
                dt = .01f;
                float velscale = Mathf.Pow(1 - damping, dt);
                Vector2 aproxVel = (currPos - prevPos) / prevDt;
                Vector2 newPos = currPos + velscale * aproxVel * dt;
                prevPos = currPos;
                currPos = newPos;
                prevDt = dt;
            } 
        }

        public void Start()
        {
            for (int i = 0; i <= numSegments; i++)
            {
                var spawnPos = new Vector2(0, i);
                var segLen = segmentLength * Mathf.Pow(1 - percSizeDecay / 100, i);
                _vertices.Add(new Vertex(spawnPos, segLen, velocityDamping));
                if (i > 0)
                {
                    _edges.Add(Instantiate(segmentPrefab, parent: transform));
                    _edges[i - 1].GetComponent<Rigidbody2D>().mass *= segLen * segLen;
                    _oldEdgePos.Add(_edges[i - 1].transform.position);
                }
            }
        }

        public void Update()
        {
            _vertices[0].currPos = transform.position;
            for (int i = 0; i < numSegments; i++)
            {
                var shift = (Vector2)_edges[i].transform.position - _oldEdgePos[i];
                _vertices[i].currPos += shift;
                _vertices[i+1].currPos += shift;
            }
            for (int i = 0; i < numSegments; i++)
            {
                var vert = _vertices[i + 1];
                vert.UpdatePos(Time.deltaTime);
            }
            for (int i = 0; i < numSegments; i++)
            {
                var vert = _vertices[i+1];
                //vert.UpdatePos(Time.deltaTime);
                ConstrainEdge(i);
                _edges[i].transform.position = _oldEdgePos[i] = vert.currPos;
            
            }
        }

        public void ConstrainEdge(int idx)
        {
            var dir = (_vertices[idx + 1].currPos - _vertices[idx].currPos).normalized;
            _vertices[idx + 1].currPos = _vertices[idx].currPos + _vertices[idx].segLen * dir;
        }
    }
}
