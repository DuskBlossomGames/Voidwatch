using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatelinManager : MonoBehaviour
{
    public int maxchildren;
    public int currchildren;

    float _timer;
    bool _red;
    void Update()
    {
        if (_timer <= 0)
        {
            _timer = .02f;
            _red = false;
            for (int i = maxchildren; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i-maxchildren).gameObject);
                _red = true;
            }
            if (_red)
            {
                transform.GetChild(transform.childCount - 1).GetComponent<PlatelinController>().spawndelay += .5f;
            }
        } else
        {
            _timer -= Time.deltaTime;
            currchildren = transform.childCount;
            if (currchildren == 0)
            {
                Destroy(gameObject);
            }
        }
    }
    
}
