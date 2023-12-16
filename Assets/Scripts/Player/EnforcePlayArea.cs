using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Spawnables.Player
{
    public class EnforcePlayArea : MonoBehaviour
    {
        private float _outOfBoundsTime = 0;

        public bool attackable;
        public float timeTillAttack = 1;
        public Gradient colorCurve;
        public GameObject warningText;
        public GameObject warningSlider;
        public GameObject warningBack;
        
        void Update()
        {
            if (gameObject.transform.position.sqrMagnitude > 75 * 75)
            {
                _outOfBoundsTime += Time.deltaTime * CustomRigidbody2D.Scaling;
            } else {
                _outOfBoundsTime -= Time.deltaTime * CustomRigidbody2D.Scaling;
            }

            _outOfBoundsTime = Mathf.Clamp(_outOfBoundsTime,0,timeTillAttack);

            if (_outOfBoundsTime > 0)
            {
                warningText.GetComponent<TextMeshProUGUI>().enabled = true;
                RectTransform rt = warningSlider.GetComponent<RectTransform>();
                float width = 500 * (1 - _outOfBoundsTime / timeTillAttack);
                rt.sizeDelta = new Vector2(width, 30f);
                warningSlider.GetComponent<Image>().enabled = true;
                warningSlider.GetComponent<Image>().color = colorCurve.Evaluate(_outOfBoundsTime / timeTillAttack);
                warningBack.GetComponent<Image>().enabled = true;
            
            } else {
                warningText.GetComponent<TextMeshProUGUI>().enabled = false;
                warningSlider.GetComponent<Image>().enabled = false;
                warningBack.GetComponent<Image>().enabled = false;
            }
            attackable = _outOfBoundsTime>=timeTillAttack;
        }
    }
}
