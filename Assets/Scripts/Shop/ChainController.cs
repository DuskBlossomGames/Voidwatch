using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Shop
{
    public class ChainController : MonoBehaviour
    {
        public Sprite lightStick, darkStick;
        public Color circleEnableColor, circleDisableColor;
        public Color numeralEnableColor, numeralDisableColor;
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
                    _numerals[i].color = i < value ? numeralEnableColor : numeralDisableColor;
                    _texts[i].color = i+1 == value ? textEnableColor : textDisableColor;
                }
            }
        }

        private Image[] _chains;
        private Image[] _circles;
        private TextMeshProUGUI[] _numerals;
        private TextMeshProUGUI[] _texts;

        private void Awake()
        {
            _chains = transform.GetChild(3).GetComponentsInChildren<Image>();
            _circles = transform.GetChild(4).GetComponentsInChildren<Image>();
            var texts = transform.GetChild(4).GetComponentsInChildren<TextMeshProUGUI>();
            _numerals = texts.Where((_, i) => i % 2 == 0).ToArray();
            _texts = texts.Where((_, i) => i % 2 == 1).ToArray();
        }
        
        
    }
}