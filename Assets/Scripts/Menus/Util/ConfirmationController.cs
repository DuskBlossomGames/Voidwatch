using UnityEngine;

namespace Menus.Util
{
    public class ConfirmationController : MonoBehaviour
    {
        private GameObject _normal;
        private GameObject _confirm;

        private void Awake()
        {
            _normal = transform.GetChild(0).gameObject;
            _confirm = transform.GetChild(1).gameObject;
        }

        // if ever out of sight, return to default state
        private void OnDisable() { SetConfirming(false); }

        public void SetConfirming(bool confirming)
        {
            _normal.SetActive(!confirming);
            _confirm.SetActive(confirming);
        }
    }
}