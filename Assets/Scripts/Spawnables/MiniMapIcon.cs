using System.Linq;
using NUnit.Framework.Internal;
using UnityEngine;

namespace Spawnables
{
    public class MiniMapIcon : MonoBehaviour
    {
        public Color border, particleColor;
        public Material borderMaterial;
        public float scale = 1, borderScale = 1.3f;
        
        void Start()
        {
            if (TryGetComponent<SpriteRenderer>(out var mySr))
            {
                var obj = new GameObject(gameObject.name + "-MMI")
                {
                    transform =
                    {
                        parent = transform,
                        localPosition = Vector3.zero,
                        localRotation = Quaternion.identity,
                        localScale = scale * Vector3.one
                    },
                    layer = LayerMask.NameToLayer("Minimap")
                };
                obj.transform.position += -10 * Vector3.forward;

                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = mySr.sprite;
                sr.color = mySr.color;
                sr.sortingOrder = mySr.sortingOrder;
                sr.sortingLayerID = mySr.sortingLayerID;
                sr.drawMode = mySr.drawMode;
                sr.maskInteraction = mySr.maskInteraction;

                if (border.a > 0)
                {
                    var outline = new GameObject("MMIO")
                    {
                        transform =
                        {
                            parent = obj.transform,
                            localPosition = Vector3.zero,
                            localRotation = Quaternion.identity,
                            localScale = Vector3.one * borderScale
                        },
                        layer = obj.layer
                    };
                    var oSr = outline.AddComponent<SpriteRenderer>();
                    oSr.sprite = sr.sprite;
                    oSr.color = border;
                    oSr.material = borderMaterial;
                    oSr.sortingOrder = sr.sortingOrder-1;
                    oSr.sortingLayerID = sr.sortingLayerID;
                    oSr.drawMode = sr.drawMode;
                    oSr.maskInteraction = sr.maskInteraction;
                }
            }

            if (TryGetComponent<ParticleSystem>(out _))
            {
                var obj = Instantiate(gameObject, transform, true);
                
                for (var i = 0; i < obj.transform.childCount; i++) Destroy(obj.transform.GetChild(i).gameObject);
                foreach (var comp in obj.GetComponents<Component>().Where(c =>
                             c is not ParticleSystem && c is not Transform && c is not ParticleSystemRenderer))
                {
                    Destroy(comp);
                }

                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = transform.lossyScale;
                obj.transform.position += -10 * Vector3.forward;
                obj.layer = LayerMask.NameToLayer("Minimap");
                obj.name = gameObject.name + "-MMIPS";

                var main = obj.GetComponent<ParticleSystem>().main;
                main.startColor = particleColor;
                obj.GetComponent<ParticleSystemRenderer>().sortingOrder -= 2; // make sure it's fully behind MMI and MMIO
            }
        }
    }
}
