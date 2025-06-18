using UnityEngine;

namespace Static_Objects_or_Misc
{
    public class MiniMapCamController : MonoBehaviour
    {
        public Transform player;

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * Mathf.Atan2(player.position.y, player.position.x));
        }
    }
}
