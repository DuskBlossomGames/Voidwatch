using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapIcon : MonoBehaviour
{
    public bool appearsAsSelf = false;
    public Sprite miniSprite;
    public Color miniColor;
    public float size = 5;
    public bool inheritsRotation;
    public Vector3 staticRot;

    private GameObject _miniIcon;

    //TODO: implement uncertain icons (position is "blurry")

    void Start()
    {
        float sizeX, sizeY;
        if (appearsAsSelf)
        {
            inheritsRotation = true;
            sizeX = transform.lossyScale.x;
            sizeY = transform.lossyScale.x;
            miniSprite = transform.GetComponent<SpriteRenderer>().sprite;
            miniColor = transform.GetComponent<SpriteRenderer>().color;

            _miniIcon = new GameObject(gameObject.name + "-SMMI");
        } else {
            sizeX = sizeY = size;
            _miniIcon = new GameObject(gameObject.name + "-MMI");
        }

        sizeX /= transform.lossyScale.x; //divide off the scale of parent to make size indep of parent
        sizeY /= transform.lossyScale.y; 

        _miniIcon.transform.parent = transform;//adopt child
        _miniIcon.transform.localPosition = Vector3.zero;
        _miniIcon.transform.position += -10 * Vector3.forward;//spawn with offset z component
        _miniIcon.transform.localRotation = Quaternion.identity;
        _miniIcon.transform.localScale = new Vector3(sizeX, sizeY, 1);
        var renderer = _miniIcon.AddComponent<SpriteRenderer>();
        renderer.sprite = miniSprite;
        renderer.color = miniColor;
        _miniIcon.layer = LayerMask.NameToLayer("Minimap");
    }

    // Update is called once per frame
    void Update()
    {
        if (!inheritsRotation)
        {
            _miniIcon.transform.rotation = Quaternion.Euler(staticRot);
        }
    }
}
