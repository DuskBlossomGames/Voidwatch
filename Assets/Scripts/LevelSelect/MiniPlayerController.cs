using System;
using System.Collections;
using System.Collections.Generic;
using Singletons.Static_Info;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using static Singletons.Static_Info.LevelSelectData;

namespace LevelSelect
{
    public class MiniPlayerController : MonoBehaviour
    {
        public float secondsPerOrbit;
        public MapController mapControler;
        
        private Vector2 _orbitPosition;
        private float _orbitRadius;
        private SpriteRenderer _bg;

        private LevelData _startPlanet;

        private void Awake()
        {
            StartCoroutine(EnableTrails());
            _bg = mapControler.GetComponent<SpriteRenderer>();
        }

        private IEnumerator EnableTrails()
        {
            yield return new WaitForSeconds(0.1f);
            foreach (var tr in GetComponentsInChildren<TrailRenderer>()) tr.enabled = true;
        }

        public void SetOrbitPosition(Vector2 position)
        {
            _orbitPosition = position;
        }
        
        public void SetOrbitRadius(float radius)
        {
            _startPlanet = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];
            _orbitRadius = radius + transform.localScale.x / 2;
        }

        private float OrbitRadius(LevelData planet) => _orbitRadius * planet.SpriteData.RadiusMult;

        private float _orbitAngle;

        public int TravelingTo { get; private set; } = -1;
        private void FixedUpdate()
        {
            if (TravelingTo != -1) return;
            
            _orbitAngle += 2 * Mathf.PI * Time.fixedDeltaTime / secondsPerOrbit % (2 * Mathf.PI);

            transform.SetLocalPositionAndRotation(
                _orbitPosition + new Vector2(
                    OrbitRadius(_startPlanet) * Mathf.Cos(_orbitAngle), 
                    OrbitRadius(_startPlanet) * Mathf.Sin(_orbitAngle)),
                Quaternion.Euler(0, 0, Mathf.Rad2Deg * _orbitAngle));
        }

        public void GoTo(Vector3 planetLoc, int planetIdx, string scene)
        {
            StartCoroutine(SetupCamera(planetLoc));
            StartCoroutine(DoGoTo(planetLoc, planetIdx, scene));
        }

        private IEnumerator SetupCamera(Vector3 planetLoc)
        {
            // setup camera
            Destroy(mapControler); // don't need anything else from it and don't want cam control
            
            var cam = Camera.main!;

            var vec = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet].WorldPosition - planetLoc;
            var targPos = vec/2 + planetLoc;
            targPos.z = cam.transform.position.z;
            var targSize = _orbitRadius + (Mathf.Abs(vec.x) > cam.aspect * Mathf.Abs(vec.y) ? Mathf.Abs(vec.x) / (2 * cam.aspect) : Mathf.Abs(vec.y) / 2);

            const float camTime = 1.5f;
            
            var camStartPos = cam.transform.position;
            var camStartSize = cam.orthographicSize;
            for (float t = 0; t < camTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();

                cam.orthographicSize = Mathf.SmoothStep(camStartSize, targSize, t / camTime);
                cam.transform.position = camStartPos + (targPos - camStartPos) * Mathf.SmoothStep(0, 1, t / camTime);
            }
        }

        private float DeltaAngle(float a, float b)
        {
            return UtilFuncs.Mod(Mathf.Deg2Rad * Mathf.DeltaAngle(a*Mathf.Rad2Deg, b*Mathf.Rad2Deg), 2*Mathf.PI);
        }

        private IEnumerator DoGoTo(Vector3 planetLoc, int planetIdx, string scene)
        {
            if (TravelingTo != -1) yield break;
            TravelingTo = planetIdx;
            
            var planetPath = MapUtil.GetShortestPath(LevelSelectDataInstance.Levels, 
                LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet], planetLoc,
                LevelSelectDataInstance.VisitedPlanets);

            if (planetPath != null)
            {
                List<Tuple<Vector2, float>> path = new();

                const float rotateStep = 0.08f;
                const float maxSpeedMult = 5.5f;

                const float planetExitRadMult = 1.95f;
                const float planetDivergePoint = 40 * Mathf.Deg2Rad;
                
                var current = _orbitPosition;
                var currentRot = _orbitAngle;
                for (var i = 1; i < planetPath.Length; i++)
                {
                    var orbitRadius = OrbitRadius(planetPath[i-1]);
                    var orbitalVelocity = 3.5f * 2 * Mathf.PI / secondsPerOrbit * orbitRadius;
                    
                    var target = (Vector2)planetPath[i].WorldPosition;
                    var exitRot = UtilFuncs.Mod(UtilFuncs.Angle(target - current), Mathf.PI * 2);

                    // rotate around current planet
                    var direction = i == 1 ? 1 : Mathf.Abs(exitRot - currentRot) < Mathf.PI ? 1 : -1;

                    for (var r = currentRot;
                         DeltaAngle(r, exitRot) >= 1.5f * rotateStep;
                         r = UtilFuncs.Mod(r + direction * rotateStep, Mathf.PI * 2))
                    {
                        var rad = orbitRadius;
                        var vel = orbitalVelocity;
                        if (DeltaAngle(r, exitRot) <= planetDivergePoint)
                        {
                            rad *= Mathf.Exp(3.5f * (1 - DeltaAngle(r, exitRot) / planetDivergePoint) - 3.5f) *
                                (planetExitRadMult - 1) + 1;
                            vel *= 1.5f;
                        }

                        path.Add(new Tuple<Vector2, float>(current + new Vector2(
                            rad * Mathf.Cos(r),
                            rad * Mathf.Sin(r)), vel));
                    }

                    // move to next planet
                    var startPos = current + (target - current).normalized * (orbitRadius * planetExitRadMult);
                    var endPos = target + (current - target).normalized *
                        (i == planetPath.Length - 1 ? OrbitRadius(planetPath[i]) / 4 : OrbitRadius(planetPath[i]));
                    
                    for (float t = 0; t < 1; t += 0.05f)
                    {
                        var multEvaluation = i < planetPath.Length - 1
                            ? Mathf.Exp(-Mathf.Pow(4 * t - 2, 2)) * (maxSpeedMult - 1) + 1
                            : Mathf.Exp(4 * t - 4) * (maxSpeedMult - 1) + 1;
                        path.Add(new Tuple<Vector2, float>(startPos + t * (endPos - startPos),
                            orbitalVelocity * multEvaluation));
                    }

                    current = target;
                    currentRot = UtilFuncs.Angle(endPos - target);
                }

                for (var i = 1; i < path.Count; i++)
                {
                    for (float t = 0;
                         t < 1;
                         t += Time.fixedDeltaTime /
                              (Vector2.Distance(path[i].Item1, path[i - 1].Item1) / path[i].Item2))
                    {
                        yield return new WaitForFixedUpdate();

                        transform.SetLocalPositionAndRotation(
                            Vector2.Lerp(path[i - 1].Item1, path[i].Item1, t),
                            Quaternion.Euler(0, 0,
                                -90 + Mathf.Rad2Deg * UtilFuncs.Angle(path[i].Item1 - path[i - 1].Item1)));
                    }
                }

                _bg.sortingOrder = 10000;

                yield return new WaitForSeconds(0.75f);
            }

            LevelSelectDataInstance.CurrentPlanet = planetIdx;
            SceneManager.LoadScene(scene);
        }
    }
}