using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Static_Objects_or_Misc
{
    public class ForceFieldHandler : MonoBehaviour
    {

        private readonly List<GameObject> _generators = new(4); 
    
        private void Start()
        {
            var prefab = transform.GetChild(0).gameObject;
            _generators.Add(prefab);
            for (var i = 1; i < 4; i++)
            {
                var generator = Instantiate(prefab, transform);
                _generators.Add(generator);
            
                var rot = i*90;
                generator.transform.localEulerAngles = new Vector3(0, 0, rot);
                generator.transform.localPosition = new Vector2(
                    -0.555f * Mathf.Sin(rot*Mathf.Deg2Rad),
                    0.555f * Mathf.Cos(rot*Mathf.Deg2Rad));
            }
        
        
        }

        private void Update()
        {
            if (!_generators.Any(g => g))
            {
                Destroy(gameObject);
            }
        }
    }
}
