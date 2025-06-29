using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions
{
    public static class 
        ColorExtension
    {
        public static void SetAlpha(this Image obj, float alpha)
        {
            obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, alpha);
        }
        
        public static void SetAlpha(this RawImage obj, float alpha)
        {
            obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, alpha);
        }
        
        public static void SetAlpha(this SpriteRenderer obj, float alpha)
        {
            obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, alpha);
        }
        
        public static void SetAlpha(this TextMeshProUGUI obj, float alpha)
        {
            obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, alpha);
        }
        
        public static void SetAlpha(this TextMeshPro obj, float alpha)
        {
            obj.color = new Color(obj.color.r, obj.color.g, obj.color.b, alpha);
        }
    }
}