using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Util.UI
{
    // adapted from https://medium.com/@lehuynhthang99/unity-ui-image-gradients-from-boring-to-blooming-80b0c6c67b9c
    public class HorizontalGradientUIMesh : MaskableGraphic
    {
        public Sprite sprite;
        
        [Serializable]
        public struct GradientPoint
        {
            public float normalizedPosition;
            public Color vertexColor;
        }

        [FormerlySerializedAs("_gradientPoints")] [SerializeField] public GradientPoint[] gradientPoints;

        private void AddQuad(VertexHelper vh, Vector2 corner1, Vector2 corner2, Vector2 uvCorner1, Vector2 uvCorner2, Color clrCorner1, Color clrCorner2)
        {
            var i = vh.currentVertCount;

            var vert = new UIVertex
            {
                color = clrCorner1,
                position = corner1,
                uv0 = uvCorner1
            };

            vh.AddVert(vert);

            vert.color = clrCorner2;
            vert.position = new Vector2(corner2.x, corner1.y);
            vert.uv0 = new Vector2(uvCorner2.x, uvCorner1.y);
            vh.AddVert(vert);

            vert.color = clrCorner2;
            vert.position = corner2;
            vert.uv0 = uvCorner2;
            vh.AddVert(vert);

            vert.color = clrCorner1;
            vert.position = new Vector2(corner1.x, corner2.y);
            vert.uv0 = new Vector2(uvCorner1.x, uvCorner2.y);
            vh.AddVert(vert);

            vh.AddTriangle(i + 0, i + 2, i + 1);
            vh.AddTriangle(i + 3, i + 2, i + 0);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var bottomLeftCorner = new Vector2(0, 0) - rectTransform.pivot;

            var rectWidth = rectTransform.rect.width;
            var rectHeight = rectTransform.rect.height;
            bottomLeftCorner.x *= rectWidth;
            bottomLeftCorner.y *= rectHeight;

            var spriteUVInAtlas = sprite ? DataUtility.GetOuterUV(sprite) : Vector4.zero;
            var horizontalMap = new Vector2(spriteUVInAtlas.x, spriteUVInAtlas.z);
            var verticalMap = new Vector2(spriteUVInAtlas.y, spriteUVInAtlas.w);

            for (var i = gradientPoints.Length - 1; i > 0; i--)
            {
                var firstGradientPoint = gradientPoints[i];
                var secondGradientPoint = gradientPoints[i - 1];
                var corner1 = new Vector2(bottomLeftCorner.x + firstGradientPoint.normalizedPosition * rectWidth, bottomLeftCorner.y + 0);
                var corner2 = new Vector2(bottomLeftCorner.x + secondGradientPoint.normalizedPosition * rectWidth, bottomLeftCorner.y + rectHeight);
                var uvCorner1 = new Vector2(firstGradientPoint.normalizedPosition, 0);
                var uvCorner2 = new Vector2(secondGradientPoint.normalizedPosition, 1);

                uvCorner1.x = Mathf.Lerp(horizontalMap.x, horizontalMap.y, uvCorner1.x);
                uvCorner2.x = Mathf.Lerp(horizontalMap.x, horizontalMap.y, uvCorner2.x);
                uvCorner1.y = Mathf.Lerp(verticalMap.x, verticalMap.y, uvCorner1.y);
                uvCorner2.y = Mathf.Lerp(verticalMap.x, verticalMap.y, uvCorner2.y);

                AddQuad(vh, corner1, corner2, uvCorner1, uvCorner2, firstGradientPoint.vertexColor, secondGradientPoint.vertexColor);
            }
        }
    }
}