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

    void Start() // used to be Awake, if problems start happening
    {
        float sizeX, sizeY;
        if (appearsAsSelf)
        {
            inheritsRotation = true;
            sizeX = 1;
            sizeY = 1;
            miniSprite = transform.GetComponent<SpriteRenderer>().sprite;
            miniColor = transform.GetComponent<SpriteRenderer>().color;

            _miniIcon = new GameObject(gameObject.name + "-SMMI");
        } else {
            sizeX = size / transform.lossyScale.x;
            sizeY = size / transform.lossyScale.y;
            _miniIcon = new GameObject(gameObject.name + "-MMI");
        }
        
        _miniIcon.transform.parent = transform;//adopt child
        _miniIcon.transform.localPosition = Vector3.zero;
        _miniIcon.transform.position += -10 * Vector3.forward;//spawn with offset z component
        _miniIcon.transform.localRotation = Quaternion.identity;
        _miniIcon.transform.localScale = new Vector3(sizeX, sizeY, 1);
        var renderer = _miniIcon.AddComponent<SpriteRenderer>();
        renderer.sprite = miniSprite;
        renderer.sortingOrder = transform.GetComponent<SpriteRenderer>().sortingOrder;
        renderer.sortingLayerID = transform.GetComponent<SpriteRenderer>().sortingLayerID;
        renderer.maskInteraction = transform.GetComponent<SpriteRenderer>().maskInteraction;
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
