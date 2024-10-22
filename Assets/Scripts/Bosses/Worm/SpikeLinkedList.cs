using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeLinkedList : MonoBehaviour
{
    public SpikeLinkedList? previous = null;
    public SpikeLinkedList? next = null;

    public float timebetweensegments;
    public float animtime;
    public bool isAnim = false;
    public AnimationCurve brightness;

    private SpriteRenderer _sprite;
    private float _prog;

    public void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        if (isAnim)
        {
            _prog += Time.deltaTime / animtime;
            if (_prog > 1)
            {
                _prog = 0;
                isAnim = false;
            }
            float value = brightness.Evaluate(_prog);
            _sprite.color = new Color(value, value, value, 1);
        }
    }

    public IEnumerator TriggerDown()
    {
        isAnim = true;
        yield return new WaitForSeconds(timebetweensegments);
        if (next != null)
        {
            StartCoroutine(next!.TriggerDown());
        }
    }
}
