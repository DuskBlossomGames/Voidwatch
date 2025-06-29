using UnityEngine;

namespace Menus.Util
{
    public class ToggleOnDropdown : MonoBehaviour
    {
        public GameObject toToggle;

        private void Start() { toToggle.SetActive(false); }
        private void OnDestroy() { toToggle.SetActive(true); }
    }
}