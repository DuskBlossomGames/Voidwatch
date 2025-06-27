using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    public class GraphicsGroup : Graphic
    {
        [Space(10)]
        [Header("Group Settings")]
        public Pair<Graphic,bool>[] graphics;
        
        public Selectable selectable;

        protected override void Awake()
        {
            color = Color.clear;
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            var press = targetColor == selectable.colors.pressedColor;
            
            foreach (var g in graphics.Where(g => !press || g)) g.a.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
        }
    }
}