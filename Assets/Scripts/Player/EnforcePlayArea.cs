using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        float clamp(float x, float low = 0, float high = 1)
        {
            return x<low?low:(x>high?high:x);
        }

        void Update()
        {
            if (gameObject.transform.position.sqrMagnitude > 75 * 75)
            {
                _outOfBoundsTime += Time.deltaTime;
            } else {
                _outOfBoundsTime -= Time.deltaTime;
            }

            _outOfBoundsTime = clamp(_outOfBoundsTime,0,timeTillAttack);

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
