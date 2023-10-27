using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPlayer : MonoBehaviour
{
    int Hash(int input)
    {
        return (
      19 * (input & 1)
    + 17 * (input & 2)
    + 13 * (input & 4)
    + 11 * (input & 8)
    + 7  * (input & 16)
    + 5  * (input & 32)
    + 3  * (input & 64)
    + 2  * (input & 128)
    + 1  * (input & 256)
        ) % 256;
    }

    Color Lerp(Color A, Color B, float t)
    {
        return A * (1 - t) + B * t;
    }
    Color genColorfromCoords(Vector2 pos, float cellsize)
    {
        int xl = Mathf.FloorToInt(pos.x / cellsize);
        int xu = Mathf.CeilToInt(pos.x / cellsize);
        int yl = Mathf.FloorToInt(pos.y / cellsize);
        int yu = Mathf.CeilToInt(pos.y / cellsize);

        float xt = xl != xu ? (pos.x - cellsize * xl) / cellsize : 0;
        float yt = yl != yu ? (pos.y - cellsize * yl) / cellsize : 0;

        int vll = Hash(Hash(Hash(xl) + yl) + 7);
        int vlu = Hash(Hash(Hash(xl) + yu) + 7);
        int vul = Hash(Hash(Hash(xu) + yl) + 7);
        int vuu = Hash(Hash(Hash(xu) + yu) + 7);

        Color cll = Color.HSVToRGB(vll / 256f, .2f, .2f);
        Color clu = Color.HSVToRGB(vlu / 256f, .2f, .2f);
        Color cul = Color.HSVToRGB(vul / 256f, .2f, .2f);
        Color cuu = Color.HSVToRGB(vuu / 256f, .2f, .2f);

        //Debug.Log(vll);

        return Lerp(
            Lerp(cll,cul,xt),
            Lerp(clu,cuu,xt),
            yt);

    }

    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetComponent<Camera>().backgroundColor = genColorfromCoords(player.transform.position,100);
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y,transform.position.z);
    }
}
