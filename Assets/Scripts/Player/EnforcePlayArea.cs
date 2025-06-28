using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Player
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

        private FollowPlayer _fp;
        private float _range;
        private float _mult;
        
        private void Start()
        {
            _outOfBoundsTimer.Value = timeTillAttack;
            _fp = Camera.main!.GetComponent<FollowPlayer>();
            
            var boundary = GameObject.FindGameObjectWithTag("Circle");
            _range = boundary.transform.localScale.x / 2;
            _mult = 4 / Mathf.Pow(200 - _range/3f, 3);
        }

        public void Reset() => _outOfBoundsTimer.Value = _outOfBoundsTimer.MaxValue;
        
        void Update()
        {
            var notSafe = ((Vector2)gameObject.transform.position).sqrMagnitude > _range * _range;
            _outOfBoundsTimer.Update(notSafe ? -1 : 1);

            if (notSafe)
            {
                _fp.ScreenShake(Time.fixedDeltaTime+0.0001f,
                    _mult * Mathf.Pow(((Vector2)gameObject.transform.position).magnitude - _range/3f, 3));
            }
            
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
