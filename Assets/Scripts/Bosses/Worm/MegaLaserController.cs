using System;
using UnityEngine;

namespace Bosses.Worm
{
    public class MegaLaserController : MonoBehaviour
    {
        private class Anim
        {
            public Sprite[] Sprites;
            public int NumFrames;
        }

        public int lightningStartFrame, lightningEndFrame;
        public Texture2D laserBuildup, lightning, beamBuildup, beamBuildupTileable, beamLoop, beamLoopTileable;
        public float laserBuildupTime, beamBuildupTime, beamLoopTime;
        private Anim _laserBuildup, _lightning, _beamBuildup, _beamBuildupTileable, _beamLoop, _beamLoopTileable;

        private float _timeElapsed;
        private float _fps;
        private float _horizTiles;

        public SpriteRenderer originSr, laserSr;

        public float TimeToLightning => lightningStartFrame / _fps;
        
        private void SetupTexture(Texture2D texture, Anim anim, float heightToWidth=1)
        {
            var sliceWidth = heightToWidth * texture.height;
            
            anim.NumFrames = (int) (texture.width / sliceWidth);
            anim.Sprites = new Sprite[anim.NumFrames];
            for (var i = 0; i < anim.NumFrames; i++) anim.Sprites[i] = Sprite.Create(texture, new Rect(i*sliceWidth, 0, sliceWidth, texture.height), new Vector2(0.5f, 0.5f), texture.height);
        }
        
        public void Start()
        {
            SetupTexture(laserBuildup, _laserBuildup = new());   
            SetupTexture(lightning, _lightning = new(), 0.5f);   
            SetupTexture(beamBuildup, _beamBuildup = new());   
            SetupTexture(beamBuildupTileable, _beamBuildupTileable = new());   
            SetupTexture(beamLoop, _beamLoop = new());   
            SetupTexture(beamLoopTileable, _beamLoopTileable = new());

            _fps = _laserBuildup.NumFrames / laserBuildupTime;

            _timeElapsed = laserBuildupTime + beamBuildupTime + beamLoopTime; // just start it at the end
        }

        public void Shoot(float length)
        {
            _timeElapsed = 0;
            _horizTiles = (length-originSr.transform.lossyScale.y/2) / laserSr.transform.lossyScale.y;
            laserSr.transform.localPosition = new Vector3(0, 0.5f + 0.25f * _horizTiles, 0);
        }

        private void Update()
        {
            var currentFrame = (int) (_timeElapsed * _fps);

            Anim originAnim, laserAnim;
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