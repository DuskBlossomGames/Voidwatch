using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Spawnables.Player
{
    public class EnforcePlayArea : MonoBehaviour
    {
        private readonly Timer _outOfBoundsTimer = new();

        public bool attackable;
        public float timeTillAttack = 1;
        public Gradient colorCurve;
        public GameObject warningText;
        public GameObject warningSlider;
        public GameObject warningBack;

        private void Start()
        {
            _outOfBoundsTimer.Value = timeTillAttack;
        }

        void Update()
        {
            var boundary = GameObject.FindGameObjectWithTag("Circle");
            var range = boundary.transform.localScale.x / 2;
            _outOfBoundsTimer.Update(gameObject.transform.position.sqrMagnitude > range * range ? -1 : 1);

            if (_outOfBoundsTimer.IsActive)
            {
                warningText.GetComponent<TextMeshProUGUI>().enabled = true;
                RectTransform rt = warningSlider.GetComponent<RectTransform>();
                float width = 500 * (_outOfBoundsTimer.Value / timeTillAttack);
                rt.sizeDelta = new Vector2(width, 30f);
                warningSlider.GetComponent<Image>().enabled = true;
                warningSlider.GetComponent<Image>().color = colorCurve.Evaluate(1 - _outOfBoundsTimer.Value / timeTillAttack);
                warningBack.GetComponent<Image>().enabled = true;
            
            } else {
                warningText.GetComponent<TextMeshProUGUI>().enabled = false;
                warningSlider.GetComponent<Image>().enabled = false;
                warningBack.GetComponent<Image>().enabled = false;
            }
            
            attackable = _outOfBoundsTimer.IsFinished;
        }
    }
}
