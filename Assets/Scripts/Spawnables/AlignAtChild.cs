using UnityEngine;

namespace Spawnables
{
    public class AlignAtChild : MonoBehaviour
    {
        public int id;

        // Update is called once per frame
        void Update()
        {
            transform.position = transform.GetChild(id).position;
        }
    }
}
