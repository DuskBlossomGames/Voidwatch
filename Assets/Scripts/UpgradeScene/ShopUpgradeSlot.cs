using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUpgradeSlot : MonoBehaviour
{
    public float currPrice;
    public float oldPrice;
    public string objName;
    public bool isNull = false;

    public float totAnimTime = .2f;
    public AnimationCurve openAnim;

    float _animTime;
    public uint state = 0;
    private uint _nstate = 0;

    RectTransform _SRT;

    private void Start()
    {
        _animTime = -1;
        _SRT = transform.GetChild(1).GetComponent<RectTransform>();
    }

    public void InvokeUpdate()
    {
        _nstate = 1;
    }

    private void Update()
    {
        _animTime -= Time.deltaTime;
        if(state == 0 && _nstate != 0)
        {
            state = _nstate;
            _nstate = 0;
            _animTime = .1f;
        }
        if(state == 1 && _animTime <= 0)
        {   
            state = 2;
            _animTime = totAnimTime;
        }
        if (_animTime < 0 && state == 2)
        {
            state = 3;
            _animTime = totAnimTime;
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = objName;
            if(currPrice != oldPrice)
            {
                transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
                transform.GetChild(0).GetChild(2).GetChild(0).GetComponent<Text>().text = currPrice.ToString();
                transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<Text>().text = oldPrice.ToString();
            } else
            {
                transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                transform.GetChild(0).GetChild(2).gameObject.SetActive(false);
                transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text = currPrice.ToString();

            }
        }
        if (_animTime < 0 && state == 3)
        {
            state = 0;
            _animTime = 0;
        }

        switch (state)
        {
            case 0: 
                break;
            case 1:
                break;
            case 2:
                _SRT.anchorMin = new Vector2(_SRT.anchorMin.x, openAnim.Evaluate(_animTime/totAnimTime));
                break;
            case 3:
                if (!isNull)
                {
                    _SRT.anchorMin = new Vector2(_SRT.anchorMin.x, openAnim.Evaluate((1 - _animTime / totAnimTime)));
                }
                break;
        }
    }
}
