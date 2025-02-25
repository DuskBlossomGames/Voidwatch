using System;
using System.Timers;
using UnityEngine;
using Random = UnityEngine.Random;
using Timer = Util.Timer;

namespace Player
{
    public class FollowPlayer : MonoBehaviour
    {
        public GameObject player;

        public float baseSize = 12;
        public float jump = 0.01f;
        public float shipPull = .35f;
        public float mouseZoomScale = 5;
        public float camLaziness = 1f;

        [NonSerialized] public bool Enabled = true;

        public bool suppres = false;
        
        private Vector3 _camOffset;
        
        private Camera _cameraComponent;
        private Camera _mainCamera;
        private Vector3 _moddedOffset;
        private float _timeSubStep;
        private Vector3 _oldOffset;

        private Timer _screenShake = new();
        private Vector2 _shakeOffset;
        public Vector2 ShakeOffset => _shakeOffset;
        private float _shakeIntensity;

        private void Start()
        {
            _mainCamera = Camera.main;
            _cameraComponent = transform.GetComponent<Camera>();
        }

        public void ScreenShake(float duration, float intensity)
        {
            _screenShake.Value = duration;
            _shakeIntensity = intensity;
        }

        private void FixedUpdate()
        {
            _screenShake.FixedUpdate();
            if (!_screenShake.IsFinished)
            {
                transform.position -= (Vector3) _shakeOffset;
                _shakeOffset = _shakeIntensity * Random.insideUnitCircle;
                transform.position += (Vector3) _shakeOffset;
            } else if (_shakeOffset.sqrMagnitude != 0)
            {
                transform.position -= (Vector3) _shakeOffset;
                _shakeOffset = Vector3.zero;
            }

            if (!Enabled) return;
            
            var playerPosition = player.transform.position;
            var cameraPosition = transform.position;
            
            //_cameraComponent.backgroundColor = GenColorFromCoords(playerPosition,100);
            
            var playerPos = new Vector3(playerPosition.x, playerPosition.y, cameraPosition.z);
            var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (suppres) mousePos = playerPos;
            var mouseToPlayer = mousePos - playerPos;
            var sqM = (mouseToPlayer - (cameraPosition - playerPos)).sqrMagnitude/camLaziness;
            var mouseInf = mouseToPlayer.sqrMagnitude / 10;
            var scale = (sqM + mouseInf) / (1 + sqM + mouseInf);
            if (suppres) scale = 1;
            //camOffset = Lerp((transform.position - playerpos), mouse2player * shipPull, jump * scale);

            //transform.position = playerpos + camOffset;
            _camOffset = Lerp(cameraPosition, Lerp(playerPos,mousePos, shipPull), jump * scale * scale);

            _timeSubStep = 0;
            //_moddedOffset = _camOffset;
            transform.position = _camOffset;
            transform.rotation = Quaternion.Euler(new Vector3(0,0,-90+Mathf.Rad2Deg*Mathf.Atan2(playerPos.y,playerPos.x)));
            player.GetComponent<Rigidbody2D>().angularVelocity = 0;


            sqM = (mousePos-playerPos).sqrMagnitude/500;
            scale = sqM / (1 + sqM);
            _mainCamera.orthographicSize = baseSize +  mouseZoomScale * scale;
        }
        
        private static int Hash(int input)
        {
            return (
                19 * (input & 1)
                + 17 * (input & 2)
                + 13 * (input & 4)
                + 11 * (input & 8)
                + 7  * (input & 16)
                + 5  * (input & 32)
                + 3  * (input & 64)
                + 2  * (input & 128)
                + 1  * (input & 256)
            ) % 256;
        }

        private static Color Lerp(Color a, Color b, float t)
        {
            return a * (1 - t) + b * t;
        }
        private static Vector3 Lerp(Vector3 a, Vector3 b, float t) {
            return a * (1 - t) + b * t;
        }
        private static Color GenColorFromCoords(Vector2 pos, float cellSize)
        {
            var xl = Mathf.FloorToInt(pos.x / cellSize);
            var xu = Mathf.CeilToInt(pos.x / cellSize);
            var yl = Mathf.FloorToInt(pos.y / cellSize);
            var yu = Mathf.CeilToInt(pos.y / cellSize);

            var xt = xl != xu ? (pos.x - cellSize * xl) / cellSize : 0;
            var yt = yl != yu ? (pos.y - cellSize * yl) / cellSize : 0;

            var vll = Hash(Hash(Hash(xl) + yl) + 7);
            var vlu = Hash(Hash(Hash(xl) + yu) + 7);
            var vul = Hash(Hash(Hash(xu) + yl) + 7);
            var vuu = Hash(Hash(Hash(xu) + yu) + 7);

            var cll = Color.HSVToRGB(vll / 256f, .2f, .2f);
            var clu = Color.HSVToRGB(vlu / 256f, .2f, .2f);
            var cul = Color.HSVToRGB(vul / 256f, .2f, .2f);
            var cuu = Color.HSVToRGB(vuu / 256f, .2f, .2f);

            //Debug.Log(vll);

            return Lerp(
                Lerp(cll,cul,xt),
                Lerp(clu,cuu,xt),
                yt);

        }
    }
}
