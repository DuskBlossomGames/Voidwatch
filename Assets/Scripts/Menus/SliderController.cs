using TMPro;
using UnityEngine;

namespace Menus
{
    public class SliderController : MonoBehaviour
    {
        public TextMeshProUGUI text;
        
        public void SetValue(float value) { text.text = value.ToString("0"); }
    }
}