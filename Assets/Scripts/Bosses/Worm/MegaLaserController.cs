using UnityEngine;
using Util;

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
        private BoxCollider2D[] _coll;

        public SpriteRenderer originSr, laserSr;

        public float TimeToLightning => lightningStartFrame / _fps;
        public bool IsShooting => _timeElapsed < laserBuildupTime + beamBuildupTime + beamLoopTime;
        
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

            _coll = GetComponentsInChildren<BoxCollider2D>();
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

        private float _startupTime = 0.5f; // wait for other things to set up like MMI and collision and stuff
        private void Update()
        {
            if (_startupTime > 0) { _startupTime -= Time.deltaTime; return; }
            
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

                _coll[0].enabled = _coll[1].enabled = true;
                _coll[1].size = new Vector2(_vertTiles, _horizTiles);
            }
            else
            {
                originSr.sprite = originSr.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = laserSr.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = laserSr.sprite = null;

                _coll[0].enabled = _coll[1].enabled = false;
                _coll[1].size = Vector2.zero;

                return;
            }

            originSr.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = originSr.sprite =
                originAnim.Sprites[originFrame % originAnim.NumFrames];
            laserSr.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = laserSr.sprite =
                laserAnim?.Sprites[laserFrame % laserAnim.NumFrames];
            laserSr.transform.GetChild(1).GetComponent<SpriteRenderer>().size =
                laserSr.size = new Vector2(_vertTiles, _horizTiles);
            
            _timeElapsed += Time.deltaTime;
        }
    }
}