using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class FollowPlayer : MonoBehaviour
    {
        public GameObject player;

        public float jump = 0.01f;
        public float shipPull = .35f;
        public float camLaziness = 1f;
        
        private Vector3 _camOffset;
        
        private Camera _cameraComponent;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
            _cameraComponent = transform.GetComponent<Camera>();
        }

        private void FixedUpdate()
        {
            var playerPosition = player.transform.position;
            var cameraPosition = transform.position;
            
            _cameraComponent.backgroundColor = GenColorFromCoords(playerPosition,100);
            
            var playerPos = new Vector3(playerPosition.x, playerPosition.y, cameraPosition.z);
            var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var mouseToPlayer = mousePos - playerPos;
            var sqM = (mouseToPlayer - (cameraPosition - playerPos)).sqrMagnitude/camLaziness;
            var mouseInf = mouseToPlayer.sqrMagnitude / 10;
            var scale = (sqM + mouseInf) / (1 + sqM + mouseInf);
            //camOffset = Lerp((transform.position - playerpos), mouse2player * shipPull, jump * scale);

            //transform.position = playerpos + camOffset;
            _camOffset = Lerp(cameraPosition, Lerp(playerPos,mousePos, shipPull), jump * scale * scale);

            transform.position = _camOffset;
            transform.rotation = Quaternion.Euler(new Vector3(0,0,-90+Mathf.Rad2Deg*Mathf.Atan2(playerPos.y,playerPos.x)));


            sqM = (mousePos-playerPos).sqrMagnitude/500;
            scale = sqM / (1 + sqM);
            _mainCamera.orthographicSize = 12 +  5 * scale;
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
