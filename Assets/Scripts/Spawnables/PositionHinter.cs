using UnityEngine;

namespace Spawnables
{
    public class PositionHinter : MonoBehaviour
    {
        public Transform playerPos;
        public GameObject arrowPrefab;
        public int pixelBuffer;
        public bool rotate = true;

        private GameObject _arrow;
        private float _camWidth, _camHeight;
        private Vector3 _defaultScale;
        private void Start()
        {
            if (_arrow != null) return;
            
            playerPos = GameObject.FindGameObjectsWithTag("Player")[0].transform;
            _arrow = transform.Find("DirHinter")?.gameObject ?? Instantiate(arrowPrefab, transform);
            _arrow.name = "DirHinter";
            _arrow.GetComponent<SpriteRenderer>().enabled = false;
            _defaultScale = arrowPrefab.transform.localScale;
            //Debug.LogFormat("LossyScale: {0}", transform.lossyScale);
            _defaultScale = new Vector3(_defaultScale.x / transform.lossyScale.x,
                _defaultScale.y / transform.lossyScale.y,
                _defaultScale.z / transform.lossyScale.z);
        }
        void Update()
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            _camWidth = Screen.width;
            _camHeight = Screen.height;

            if (screenPos.x < pixelBuffer || screenPos.x > _camWidth - pixelBuffer || screenPos.y < pixelBuffer || screenPos.y > _camHeight - pixelBuffer)
            {
                //transform.name = string.Format("Pos: {0}", screenPos);
                _arrow.GetComponent<SpriteRenderer>().enabled = true;
                Vector3 npos = Camera.main.ScreenToWorldPoint(GenPos(playerPos.position,transform.position));
                npos.z = -1;
                _arrow.transform.position = npos;
                if (rotate)
                {
                    _arrow.transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * Mathf.Atan2(transform.position.y - playerPos.position.y, transform.position.x - playerPos.position.x));
                } else
                {
                    _arrow.transform.rotation = Camera.main.transform.rotation;
                }
            }
            else
            {
                _arrow.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        public void OnDestroy()
        {
            Destroy(_arrow);
        }

        Vector3 GenPos(Vector3 orig, Vector3 tar)
        {
            Vector3 camPos = Camera.main.WorldToScreenPoint(orig);
            Vector3 tarPos = Camera.main.WorldToScreenPoint(tar);
            Vector3 diff = tarPos - camPos;
            float xscale, yscale;
            if (tarPos.x < pixelBuffer)
            {
                xscale = (camPos.x - pixelBuffer) / diff.x;
            }
            else if(tarPos.x>_camWidth)
            {
                xscale = (_camWidth - pixelBuffer - camPos.x) / diff.x;
            } else
            {
                xscale = 1;
            }
            if (tarPos.y < 0)
            {
                yscale = (camPos.y - pixelBuffer) / diff.y;
            }
            else if (tarPos.y > _camHeight)
            {
                yscale = (_camHeight - pixelBuffer - camPos.y) / diff.y;
            }
            else
            {
                yscale = 1;
            }

            float scale = Mathf.Min(Mathf.Abs(xscale), Mathf.Abs(yscale));
            float distScale = .5f / (1.5f - scale);
            _arrow.transform.localScale = new Vector3(_defaultScale.x * distScale, _defaultScale.y * distScale, _defaultScale.z);

            return camPos + scale * diff;

        }
    }
}
