using System;
using UnityEngine;
using Util;
using Debug = System.Diagnostics.Debug;

namespace Bosses.Worm
{
    public class MegaLaserController : MonoBehaviour
    {
        public int lightningStartFrame, lightningEndFrame;
        public Texture2D laserBuildup, lightning, beamBuildup, beamBuildupTileable, beamLoop, beamLoopTileable;
        public float laserBuildupTime, beamBuildupTime, beamLoopTime;
        private UtilFuncs.Anim _laserBuildup, _lightning, _beamBuildup, _beamBuildupTileable, _beamLoop, _beamLoopTileable;

        private float _timeElapsed;
        private float _fps;
        private float _horizTiles;

        public SpriteRenderer originSr, laserSr;

        public float TimeToLightning => lightningStartFrame / _fps;
        
        public void Start()
        {
            UtilFuncs.SetupTexture(laserBuildup, _laserBuildup = new());   
            UtilFuncs.SetupTexture(lightning, _lightning = new(), 0.5f);   
            UtilFuncs.SetupTexture(beamBuildup, _beamBuildup = new());   
            UtilFuncs.SetupTexture(beamBuildupTileable, _beamBuildupTileable = new());   
            UtilFuncs.SetupTexture(beamLoop, _beamLoop = new());   
            UtilFuncs.SetupTexture(beamLoopTileable, _beamLoopTileable = new());

            _fps = _laserBuildup.NumFrames / laserBuildupTime;

            _timeElapsed = laserBuildupTime + beamBuildupTime + beamLoopTime; // just start it at the end
        }

        public void Shoot(float length, float start=0)
        {
            _timeElapsed = start;
            SetLength(length);
        }

        public void SetLength(float length)
        {
            _horizTiles = (length-originSr.transform.lossyScale.y/2) / laserSr.transform.lossyScale.y;
            laserSr.transform.localPosition = new Vector3(0, 0.5f + 0.25f * _horizTiles, 0);
        }

        private void Update()
        {
            var currentFrame = (int) (_timeElapsed * _fps);

            UtilFuncs.Anim originAnim, laserAnim;
            int originFrame, laserFrame;
            float _vertTiles = 1;

            if (_timeElapsed < laserBuildupTime)
            {
                originAnim = _laserBuildup;
                laserAnim = currentFrame >= lightningStartFrame && currentFrame <= lightningEndFrame ? _lightning : null;

                originFrame = currentFrame;
                laserFrame = currentFrame - lightningStartFrame;
                _vertTiles = 0.5f;
            } else if (_timeElapsed - laserBuildupTime < beamBuildupTime)
            {
                originAnim = _beamBuildup;
                laserAnim = _beamBuildupTileable;

                originFrame = laserFrame = currentFrame - _laserBuildup.NumFrames;
            } else if (_timeElapsed - laserBuildupTime - beamBuildupTime < beamLoopTime)
            {
                originAnim = _beamLoop;
                laserAnim = _beamLoopTileable;
                
                originFrame = laserFrame = currentFrame - _laserBuildup.NumFrames - _beamLoop.NumFrames;
            }
            else
            {
                originSr.sprite = laserSr.sprite = null;
                return;
            }

            originSr.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = originSr.sprite =
                originAnim.Sprites[originFrame % originAnim.NumFrames];
            laserSr.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = laserSr.sprite =
                laserAnim?.Sprites[laserFrame % laserAnim.NumFrames];
            laserSr.transform.GetChild(1).GetComponent<SpriteRenderer>().size = laserSr.size = laserSr.transform.GetChild(0).GetComponent<BoxCollider2D>().size =
                new Vector2(_vertTiles, _horizTiles);
            
            _timeElapsed += Time.deltaTime;
        }
    }
}