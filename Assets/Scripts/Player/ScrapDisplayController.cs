using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Static_Info.PlayerData;

public class ScrapDisplayController : MonoBehaviour
{
    public RectTransform parent;
    public TMP_Text total;
    public TMP_Text delta;

    private int _accum;
    public float accumWindow = 1f;
    public float slideTime = .2f;
    private float _slideprog = 0;
    private float _accumprog = 0;

    // Update is called once per frame
    void Update()
    {
        var PDI = PlayerDataInstance;
        if (_accumprog > 0 && _slideprog < 1)
        {
            _slideprog += Time.deltaTime / slideTime;
            if (_slideprog > 1) _slideprog = 1;
        }
        if (_accumprog < 0 && _slideprog > 0)
        {
            _slideprog -= Time.deltaTime / slideTime;
            if (_slideprog < 0) _slideprog = 0;

        } else if (_accumprog < 0 && _slideprog == 0)
        {
            PDI.Scrap += _accum;
            _accum = 0;
        }
        _accumprog -= Time.deltaTime / accumWindow;
        parent.anchoredPosition = new Vector3(-500 + 200*_slideprog, 160, 0);
    }

    public void Collect(int amt)
    {
        _accumprog = 1;
        _accum += amt;
        delta.text = $"+{_accum:# ### ###}";
        total.text = $"{PlayerDataInstance.Scrap:# ### ###} SCRAP";
    }
}
