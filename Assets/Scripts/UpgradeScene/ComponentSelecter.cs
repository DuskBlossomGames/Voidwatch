using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scriptable_Objects.Upgrades;

public class ComponentSelecter : MonoBehaviour
{
    public TextTable.Select select;

    public enum ComponentState
    {
        Weapons = 1,
        Engines = 2,
        Shields = 3,
    }

    public ComponentState compstate;
    public TextTable next;
    public TextTable TabR;
    private Text[] _texts;

    public List<BaseUpgrade> upgrades;
    public List<BaseComponent> components;

    private void Start()
    {
        next.components = components;
        TabR.upgrades = upgrades;

        _texts = new Text[3];
        for (int i = 0; i < 3; i++)
        {
            _texts[i] = transform.GetChild(i).GetComponent<Text>();
        }
    }

    private void Update()
    {
        if (select == TextTable.Select.Current)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                compstate = (ComponentState)(((int)compstate == 1) ? 1 : (int)compstate - 1);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                compstate = (ComponentState)(((int)compstate == 3) ? 3 : (int)compstate + 1);
            }

            for (int i = 0; i < 3; i++)
            {
                bool isSel = (int)compstate == i + 1;
                _texts[i].text = (isSel ? ">" : "") + ((ComponentState)i + 1).ToString() + (isSel ? "<" : "");
            }
            if (Input.GetKeyDown(KeyCode.Return) && next != null)
            {
                select = TextTable.Select.Selected;
                next.selectState = TextTable.Select.Current;
                next.ack = false;
            }
        }

    }
}
