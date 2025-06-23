using TMPro;
using UnityEngine;

namespace Menus
{
    public class SliderController : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public float multiplier = 1;
        
        public void SetValue(float value)
        {
            text.text = (value*multiplier).ToString("0.#");
        }
    }
}