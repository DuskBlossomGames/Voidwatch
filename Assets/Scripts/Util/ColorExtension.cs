using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    public static class ColorExtension
    {
        public static void SetAlpha(this Image image, float alpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }
        
        public static void SetAlpha(this SpriteRenderer image, float alpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }
        
        public static void SetAlpha(this TextMeshProUGUI image, float alpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }
    }
}