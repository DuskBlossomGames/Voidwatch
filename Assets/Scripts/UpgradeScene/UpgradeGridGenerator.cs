using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeGridGenerator : MonoBehaviour
{
    public int size;
    public string layout;
    public bool inEditMode;

    public GameObject slotPrefab;

    private void Start()
    {
        float dp = 2f / (size - 1);
        float x, y;
        string clayout = string.Copy(layout);
        bool cState;

        for (int row = 0; row < size; row++)
        {
            y = 1 - dp * row;
            for (int col = 0; col < size; col++)
            {
                x = dp * col - 1;

                if (clayout[0] == '.')
                {
                    cState = false;
                }
                else if (clayout[0] == 'X')
                {
                    cState = true;

                }
                else
                {
                    cState = false;
                    Debug.LogError("Expected . or X got: " + clayout + " @ row: "+row+" col: "+col);
                }
                clayout = clayout[1..];

                GameObject spawn = Instantiate(slotPrefab, new Vector3(x, y, -1), Quaternion.identity, transform);
                spawn.GetComponent<SpriteRenderer>().enabled = cState;
                spawn.transform.localScale = new Vector3(dp, dp, 1);
            }
            if (clayout[0] != ';')
            {
                Debug.LogError("Expected ; got: " + clayout);
            }
            clayout = clayout[1..];
        }
    }
}
