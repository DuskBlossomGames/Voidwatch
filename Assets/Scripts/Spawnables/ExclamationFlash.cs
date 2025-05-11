using UnityEngine;

namespace Spawnables
{
    public class ExclamationFlash : MonoBehaviour
    {
        public Color[] colors;
        public float timePerColor;
        public bool bounce;
        
        private float _time;
        private SpriteRenderer _sr;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            
            var newColors = new Color[bounce ? (colors.Length-1)*2 : colors.Length];
            for (var i = 0; i < newColors.Length; i++)
            {
                newColors[i] = i < colors.Length ? colors[i] : colors[^(i - colors.Length + 1)];
            }
            colors = newColors;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            _time %= timePerColor * colors.Length;
            
            _sr.color = colors[(int) (_time / timePerColor)];
        }
    }
}