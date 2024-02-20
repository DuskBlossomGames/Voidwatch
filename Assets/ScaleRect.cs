using UnityEngine;

public class ScaleRect : MonoBehaviour
{
    float _screenscale;
    public bool isClone;

    void Ready()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            var obj = transform.GetChild(i).gameObject.AddComponent<ScaleRect>();
            obj.isClone = true;
            obj.Ready();
        }

        _screenscale = (float)Camera.main.pixelWidth / 1920;
        _screenscale = Mathf.Min(_screenscale, (float)Camera.main.pixelHeight / 1080);


        RectTransform rect = GetComponent<RectTransform>();
        rect.offsetMin *= _screenscale;
        rect.offsetMax *= _screenscale;
    }

    private void Start()
    {
        if (!isClone)
        {
            Ready();
        }
    }
}