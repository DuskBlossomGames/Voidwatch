using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public GameObject leftBar,rightBar,bottomBar;
    public List<GameObject> components;
    public List<GameObject> upgrades;
    public UpgradeGridGenerator gridGenerator;

    private List<GameObject> _currComponents;

    private void Start()
    {
        int pxWidth = Screen.width;
        int pxHeight = Screen.height;
        float lSideLBar = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        float rSideLBar = -1.5f;

        float lmid = (lSideLBar+ rSideLBar)/2;


        float dp = 2f / (gridGenerator.size - 1);
        float y = 1f;
        foreach (var component in components)
        {
            y -= component.GetComponent<UpgradeItem>().height / 2 * dp;
            GameObject spawn = Instantiate(component, new Vector3(lmid, y, -2), Quaternion.identity, transform);
            UpgradeItem upgrade = spawn.GetComponent<UpgradeItem>();
            y -= upgrade.height / 2 * dp + .2f;

           
            upgrade.gridGenerator = gridGenerator;
            upgrade.targetPos = spawn.transform.localPosition;
            upgrade.Ready();
        }
    }
}
