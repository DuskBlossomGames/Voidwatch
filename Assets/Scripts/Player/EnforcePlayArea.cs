using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnforcePlayArea : MonoBehaviour
{
    private float _outOfBoundsTime = 0;

    public bool attackable;
    public float timeTillAttack = 1;
    public Gradient colorCurve;
    public GameObject warningText;
    public GameObject warningSlider;
    public GameObject warningBack;

    float clamp01(float x)
    {
        return x<0?0:(x>1?1:x);
    }

    void Update()
    {
        if (gameObject.transform.position.sqrMagnitude > 75 * 75)
        {
            _outOfBoundsTime += Time.deltaTime;
        } else {
            _outOfBoundsTime -= Time.deltaTime;
        }

        _outOfBoundsTime = clamp01(_outOfBoundsTime);

        if (_outOfBoundsTime > 0)
        {
            warningText.GetComponent<TMPro.TextMeshProUGUI>().enabled = true;
            RectTransform rt = warningSlider.GetComponent<RectTransform>();
            float width = 500 * (1 - _outOfBoundsTime / timeTillAttack);
            rt.sizeDelta = new Vector2(width, 30f);
            warningSlider.GetComponent<Image>().enabled = true;
            warningSlider.GetComponent<Image>().color = colorCurve.Evaluate(_outOfBoundsTime / timeTillAttack);
            warningBack.GetComponent<Image>().enabled = true;
            attackable = true;
        } else {
            warningText.GetComponent<TMPro.TextMeshProUGUI>().enabled = false;
            warningSlider.GetComponent<Image>().enabled = false;
            warningBack.GetComponent<Image>().enabled = false;
            attackable = false;
        }
    }
}
