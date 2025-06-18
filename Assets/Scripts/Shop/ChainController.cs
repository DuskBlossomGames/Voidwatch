using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    public class ChainController : MonoBehaviour
    {
        public Sprite lightStick, darkStick;
        public Color circleEnableColor, circleDisableColor;
        public Color textEnableColor, textDisableColor;
        
        private int _unlocked;
        public int Unlocked
        {
            get => _unlocked;
            set
            {
                _unlocked = value;

                for (var i = 0; i < _chains.Length; i++)
                {
                    _chains[i].sprite = i < value ? lightStick : darkStick;
                    _circles[i].color = i < value ? circleEnableColor : circleDisableColor;
                    _texts[i].color = i < value ? textEnableColor : textDisableColor;
                }
            }
        }

        private Image[] _chains;
        private Image[] _circles;
        private TextMeshProUGUI[] _texts;

        private void Start()
        {
            _chains = transform.GetChild(0).GetComponentsInChildren<Image>();
            _circles = transform.GetChild(1).GetComponentsInChildren<Image>();
            _texts = transform.GetChild(1).GetComponentsInChildren<TextMeshProUGUI>();
        }
    }
}