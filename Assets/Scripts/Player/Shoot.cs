using TMPro;
using UnityEngine;

namespace Player
{
    public class Shoot : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public float playRadius;
        public GameObject gravitySource;
        public TextMeshProUGUI bulletText;

        GunHandler _gun;
        private void Start()
        {
            _gun = GetComponent<GunHandler>();
        }
        private void Update()
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                _gun.Shoot(0);
            }
            bulletText.text = string.Format("Clips: {0}/{1}\nAmmo: {2}/{3}\nStatus: {4}", _gun.CurrClipCount(), _gun.clipCount, _gun.CurrClipCap(), _gun.clipCap,_gun.status);
        }
    }
}
