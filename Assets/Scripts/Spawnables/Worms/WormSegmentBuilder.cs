using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormSegmentBuilder : MonoBehaviour
{
    public int length;
    public Sprite headSprite;
    public Sprite bodySprite;
    public Sprite tailSprite;

    public GameObject segmentPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (length < 2)
        {
            throw new System.Exception(string.Format("Length: {0} is less than 2, the minimum length", length));
        }

        GameObject child, oldChild;
        Vector3 relPos = Vector3.zero;
        child = Instantiate(segmentPrefab, transform);
        child.GetComponent<SpriteRenderer>().sprite = headSprite;
        child.GetComponent<WormSegment>().form = WormSegment.Form.Head;
        child.GetComponent<WormDamageable>().root = gameObject;
        child.transform.localPosition = relPos;
        oldChild = child;
        relPos.x += 1;
        for (int i = 0; i < length-2; i++)
        {
            child = Instantiate(segmentPrefab, transform);
            child.GetComponent<SpriteRenderer>().sprite = bodySprite;
            child.GetComponent<WormSegment>().form = WormSegment.Form.Middle;
            child.GetComponent<WormSegment>().prev = oldChild;
            oldChild.GetComponent<WormSegment>().next = child;
            child.transform.localPosition = relPos;
            child.GetComponent<WormDamageable>().root = gameObject;
            oldChild = child;
            relPos.x += 1;
        }
        child = Instantiate(segmentPrefab, transform);
        child.GetComponent<SpriteRenderer>().sprite = tailSprite;
        child.GetComponent<WormSegment>().form = WormSegment.Form.Tail;
        child.GetComponent<WormSegment>().prev = oldChild;
        oldChild.GetComponent<WormSegment>().next = child;
        child.transform.localPosition = relPos;
        child.GetComponent<WormDamageable>().root = gameObject;
    }

}
